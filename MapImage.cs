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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

namespace WmsViewer
{
    public class MapImage : IDisposable
    {
        public MapImage(string layerNames,
            int crsIdentifier,
            IViewport view,
            ImageFormat imageFormat,
            bool isTransparentImage,
            Color backgroundColor)
        {
            Debug.Assert(!string.IsNullOrEmpty(layerNames), "Image without layer names is empty!");
            layers = layerNames;
            Debug.Assert(crsIdentifier >= 0, "CRS ID cannot be negative!");
            crsId = crsIdentifier;
            Debug.Assert(view != null, "Image must have information about its position!");
            viewport = view;
            format = imageFormat;
            isTransparent = isTransparentImage;
            Debug.Assert(backgroundColor != null, "There must exists any transparent color!");
            bgColor = backgroundColor;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var image = obj as MapImage;
            if (image == null)
            {
                return false;
            }

            return layers.Equals(image.layers)
                && crsId.Equals(image.crsId)
                && viewport.Equals(image.viewport)
                && format.Equals(image.format)
                && isTransparent.Equals(image.isTransparent)
                && bgColor.Equals(image.bgColor);
        }

        public bool CRSEquals(MapImage image)
        {
            return crsId.Equals(image.crsId);
        }

        public override int GetHashCode()
        {
            return layers.GetHashCode()
                ^ crsId.GetHashCode()
                ^ viewport.GetHashCode()
                ^ format.GetHashCode()
                ^ isTransparent.GetHashCode()
                ^ bgColor.GetHashCode();
        }

        public void AssignData(Image imageData)
        {
            data = imageData;
        }

        public Uri GetImageAddress(Wms wms)
        {
            // This object is readonly.
            // WMS is readonly.
            Debug.Assert(wms != null, "Every layer must have a parent layer!");

            if (layers.Length > 0 && (crsId >= 0 && crsId < wms.CRSCount))
            {
                // Coordinates of requested map, left ...
                var left = viewport.LeftCorner.ToString(CultureInfo.InvariantCulture);
                // ... bottom ...
                var bottom = viewport.BottomCorner.ToString(CultureInfo.InvariantCulture);
                // ... right ...
                var right = viewport.RightCorner.ToString(CultureInfo.InvariantCulture);
                var top = viewport.TopCorner.ToString(CultureInfo.InvariantCulture);      // ... and top

                var isReversed = wms.IsCrsReversed;
                var minx = isReversed ? bottom : left;
                var miny = isReversed ? left : bottom;
                var maxx = isReversed ? top : right;
                var maxy = isReversed ? right : top;

                var address = string.Format(CultureInfo.InvariantCulture, IMAGE_ADDRESS_TEMPLATE,
                    wms.ServiceAddress,     // Address of the service
                    wms.Version,        // Version of the service
                    layers,     // Ordered list of layers (the first is the top-most)
                    wms.CRSVariableName,        // Variable name for CRS
                    wms.GetCRSName(crsId),      // CRS of requested map
                    minx, miny, maxx, maxy,
                    viewport.Width,     // Width of image in pixels
                    viewport.Height,        // Height of image in pixels
                    wms.GetImageFormat(format),     // Format of the map image
                    isTransparent.ToString(),       // String representation of transparency
                    // Transparent color in hexadecimal: red ...
                    bgColor.R.ToString("X2", CultureInfo.InvariantCulture),
                    bgColor.G.ToString("X2", CultureInfo.InvariantCulture),     // ... green ...
                    bgColor.B.ToString("X2", CultureInfo.InvariantCulture));        // ... and blue

                try
                {
                    var uri = new Uri(address);
                    return uri;
                }
                catch (UriFormatException)
                {
                    Debug.Fail("The address template is incorrect!");
                    return null;
                }
            }

            return null;
        }

        public void Draw(IViewport view, Graphics canvas)
        {
            // This object is readonly.
            Debug.Assert(view != null, "It is necessary to have viewport to draw!");
            Debug.Assert(canvas != null, "It is necessary to have canvas to draw!");

            if (viewport.Equals(view))
            {
                canvas.DrawImage(data, new Point(0, 0));
            }
            else
            {
                var x1 = Math.Max(view.LeftCorner, viewport.LeftCorner);
                var x2 = Math.Min(view.RightCorner, viewport.RightCorner);
                var y1 = Math.Max(view.BottomCorner, viewport.BottomCorner);
                var y2 = Math.Min(view.TopCorner, viewport.TopCorner);

                if ((x1 < x2) && (y1 < y2))
                {
                    // Image is at least partly on the screen.
                    var destLeft = view.XToCanvasX(x1);
                    var destTop = view.YToCanvasY(y2);
                    var srcLeft = viewport.XToCanvasX(x1);
                    var srcTop = viewport.YToCanvasY(y2);

                    canvas.DrawImage(data,
                        new Rectangle(destLeft, destTop, view.XToCanvasX(x2) - destLeft,
                            view.YToCanvasY(y1) - destTop),
                        new Rectangle(srcLeft, srcTop, viewport.XToCanvasX(x2) - srcLeft,
                            viewport.YToCanvasY(y1) - srcTop), GraphicsUnit.Pixel);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && data != null)
            {
                data.Dispose();
                data = null;
            }
        }

        /// <summary>
        /// Template address for request the map image.
        /// </summary>
        private const string IMAGE_ADDRESS_TEMPLATE = "{0}?service=WMS&version={1}&request=GetMap&layers={2}"
            + "&styles=&{3}={4}&bbox={5},{6},{7},{8}&width={9}&height={10}&format={11}"
            + "&transparent={12}&bgcolor=0x{13}{14}{15}";

        /// <summary>
        /// Name compound from the name of leaf layer or from all names of requested layers.
        /// </summary>
        private string layers = string.Empty;

        /// <summary>
        /// ID of any supported coordinate reference system.
        /// </summary>
        private int crsId = -1;

        /// <summary>
        /// Canvas, dimensions of image and coordinates in CRS.
        /// </summary>
        private IViewport viewport;

        /// <summary>
        /// Format of the image.
        /// </summary>
        private ImageFormat format = ImageFormat.PNG;

        /// <summary>
        /// True if image is transparent.
        /// </summary>
        private bool isTransparent = true;

        /// <summary>
        /// The transparent color if isTransparent is true and image format supports it.
        /// </summary>
        private Color bgColor = Color.White;

        /// <summary>
        /// Downloaded image.
        /// </summary>
        private Image data;
    }
}
