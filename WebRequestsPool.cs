/**
Copyright (c) 2012 - 2019, David Skorvaga
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace WmsViewer
{
    using AddressedImage = KeyValuePair<Uri, MapImage>;
    using ImageRequest = KeyValuePair<Layer, KeyValuePair<Uri, MapImage>>;
    using RequestsPool = Dictionary<Layer, KeyValuePair<Uri, MapImage>>;

    public class WebRequestsPool : IDisposable
    {
        public static WebRequestsPool Pool
        {
            get
            {
                if (webRequestPool == null)
                {
                    webRequestPool = new WebRequestsPool();
                }

                return webRequestPool;
            }
        }

        public static void ResetPool()
        {
            if (webRequestPool != null)
            {
                webRequestPool.Dispose();
                webRequestPool = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SetCanvas(PictureBox pictureCanvas)
        {
            canvas = pictureCanvas;
        }

        public void PutRequest(Layer layer, MapImage image, Uri uri)
        {
            Debug.Assert(layer != null, "New request must be identifiable by a layer!");
            Debug.Assert(image != null, "New request must be able to download image by some information!");
            Debug.Assert(uri != null, "Uri must be known before putting to the request pool!");

            try
            {
                // Protected access to pool
                lock (pool)
                {
                    var isRequestedLayerSame = false;
                    var isLayerInProgressChanged = false;
                    // Lock for the currently running task
                    lock (taskLocker)
                    {
                        // Current task could be NO_REQUEST
                        if (layer.Equals(currentTask.Key))
                        {
                            isRequestedLayerSame = true;
                            if (!currentTask.Value.Value.Equals(image))
                            {
                                // If here comes different request for currently processed layer,
                                // we need to abord the running one.
                                currentTask = NO_REQUEST;
                                isLayerInProgressChanged = true;
                            }
                        }
                    }

                    if (isRequestedLayerSame)
                    {
                        if (isLayerInProgressChanged)
                        {
                            // New request instead of the aborded one
                            var newImage = image;
                            image = null;
                            pool.Add(layer, new AddressedImage(uri, newImage));
                        }
                    }
                    else
                    {
                        AddressedImage addressedImage;
                        if (pool.TryGetValue(layer, out addressedImage))
                        {
                            if (!addressedImage.Value.Equals(image))
                            {
                                // New request rewrites the old one.
                                var newImage = image;
                                image = null;
                                var oldImage = pool[layer];
                                pool[layer] = new AddressedImage(uri, newImage);
                                oldImage.Value.Dispose();
                            }
                        }
                        else
                        {
                            var newImage = image;
                            image = null;
                            pool.Add(layer, new AddressedImage(uri, newImage));       // Brend new request
                        }
                    }
                }
            }
            finally
            {
                if (image != null)
                {
                    image.Dispose();
                }
            }

            waitEvent.Set();
        }

        public void RemoveRequest(Layer layer)
        {
            Debug.Assert(layer != null, "Removing request must be identifiable by a layer!");

            // Protected access to pool
            lock (pool)
            {
                var isRequestedLayerSame = false;
                // Lock for the currently running task
                lock (taskLocker)
                {
                    if (layer.Equals(currentTask.Key))
                    {
                        currentTask = NO_REQUEST;
                        isRequestedLayerSame = true;
                    }
                }

                if (!isRequestedLayerSame)
                {
                    pool.Remove(layer);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && downloader.IsAlive)
            {
                using (downloadingEvent)
                using (waitEvent)
                using (webClient)
                {
                    try
                    {
                        try
                        {
                            isRunning = false;
                            lock (pool)
                            {
                                foreach (var request in pool)
                                {
                                    request.Value.Value.Dispose();
                                }

                                pool.Clear();
                            }

                            lock (taskLocker)
                            {
                                currentTask = NO_REQUEST;
                            }
                        }
                        finally
                        {
                            waitEvent.Set();
                        }
                    }
                    finally
                    {
                        downloader.Join();
                    }
                }

                webClient = null;
                waitEvent = null;
                downloadingEvent = null;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="WebRequestsPool" /> class from being created.
        /// </summary>
        private WebRequestsPool()
        {
            webClient.DownloadProgressChanged
                += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
            webClient.DownloadDataCompleted += new DownloadDataCompletedEventHandler(DownloadDataCompleted);
            downloader = new Thread(DownloadRequests);
            downloader.Start();
        }

        private void DownloadRequests()
        {
            while (isRunning)
            {
                Uri uri = null;
                lock (pool)
                {
                    var nextRequest = NO_REQUEST;
                    foreach (var request in pool)
                    {
                        nextRequest = request;
                        pool.Remove(request.Key);
                        break;
                    }

                    lock (taskLocker)
                    {
                        currentTask = nextRequest;
                        if (!currentTask.Equals(NO_REQUEST))
                        {
                            uri = currentTask.Value.Key;
                        }
                    }
                }

                if (uri != null)
                {
                    try
                    {
                        webClient.DownloadDataAsync(uri);
                    }
                    catch (WebException)
                    {
                        // TODO: Write error to layer: connection failed
                        downloadingEvent.Set();
                    }

                    downloadingEvent.WaitOne();
                }
                else
                {
                    // There are no requests, waiting...
                    waitEvent.WaitOne();
                }
            }
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            lock (taskLocker)
            {
                if (currentTask.Equals(NO_REQUEST))
                {
                    webClient.CancelAsync();
                }
            }
        }

        private void DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                Image newImage = null;
                using (var stream = new MemoryStream(e.Result))
                {
                    try
                    {
                        newImage = Image.FromStream(stream);
                    }
                    catch (ArgumentException)
                    {
                        // TODO: Write error to layer: It is not valid image format
                    }
                }

                if (newImage != null)
                {
                    lock (taskLocker)
                    {
                        if (!currentTask.Equals(NO_REQUEST))
                        {
                            currentTask.Value.Value.AssignData(newImage);
                            currentTask.Key.SetImage(currentTask.Value.Value);
                            currentTask = NO_REQUEST;
                            var pictureCanvas = canvas;
                            if (pictureCanvas != null)
                            {
                                pictureCanvas.Invalidate(false);
                            }
                        }
                        else
                        {
                            newImage.Dispose();
                        }
                    }
                }
            }

            downloadingEvent.Set();
        }

        private static WebRequestsPool webRequestPool = null;
        private static readonly ImageRequest NO_REQUEST = new ImageRequest(null, new AddressedImage(null, null));

        private Thread downloader;
        private RequestsPool pool = new RequestsPool();
        private ImageRequest currentTask = NO_REQUEST;
        private readonly object taskLocker = new object();
        private WebClient webClient = new WebClient();
        private PictureBox canvas = null;
        private EventWaitHandle waitEvent = new AutoResetEvent(false);
        private EventWaitHandle downloadingEvent = new AutoResetEvent(false);
        private bool isRunning = true;
    }
}
