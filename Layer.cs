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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WmsViewer
{
    using CRSIds = SortedDictionary<int, object>;
    using LayerCreators = Queue<Layer.LayerCreator>;

    [Serializable]
    public abstract class Layer : IDisposable, IEnumerable<Layer>
    {
        public interface IWmsLayerCreator
        {
            /// <summary>
            /// Gets creator of WMS object.
            /// </summary>
            Wms.WmsCreator WmsCreator { get; }

            /// <summary>
            /// Gets or sets name of the layer (used for request to the service).
            /// </summary>
            string Name { get; set; }

            /// <summary>
            /// Gets or sets title name.
            /// </summary>
            string Title { get; set; }

            /// <summary>
            /// Gets or sets short description of the layer.
            /// </summary>
            string Description { get; set; }

            void AddCrs(string crs);

            IWmsLayerCreator AddLayer();

            bool HasSubLayer();

            Layer Create(Wms wms);
        }

        public class LayerCreator : IWmsLayerCreator
        {
            public LayerCreator(Wms.WmsCreator wmsCreator)
                : this(wmsCreator, null)
            {
                // Not used
            }

            public LayerCreator(Wms.WmsCreator wmsCreator, LayerCreator parent)
            {
                Debug.Assert(wmsCreator != null, "We need WmsCreator to create layers!");
                WmsCreator = wmsCreator;
                if (parent == null)
                {
                    WmsCreator.Layer = this;
                }

                Parent = parent;
                Name = string.Empty;
                Title = string.Empty;
                Description = string.Empty;
                ImageFormat = ImageFormat.PNG;
            }

            /// <inheritdoc />
            public Wms.WmsCreator WmsCreator { get; private set; }

            /// <summary>
            /// Gets parent of the layer creator.
            /// </summary>
            public LayerCreator Parent { get; private set; }

            /// <inheritdoc />
            public string Name { get; set; }

            /// <inheritdoc />
            public string Title { get; set; }

            /// <inheritdoc />
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets image format.
            /// </summary>
            public ImageFormat ImageFormat { get; set; }

            public void AddCrs(string crs)
            {
                // crs is readonly.
                Debug.Assert(crs != null, "CRS must have any value!");
                var crsId = WmsCreator.AddCrs(crs);

                // CRS can occur repeatedly in parents (even in layer itself).
                for (var layer = this; layer != null; layer = layer.Parent)
                {
                    foreach (var id in layer.crsIds.Keys)
                    {
                        if (id == crsId)
                        {
                            return;
                        }
                    }
                }

                crsIds.Add(crsId, null);
            }

            public bool HasCrs()
            {
                // This object is readonly.
                return crsIds.Count > 0;
            }

            public IWmsLayerCreator AddLayer()
            {
                var layer = new LayerCreator(WmsCreator, this);
                layers.Enqueue(layer);
                return layer;
            }

            public bool HasSubLayer()
            {
                // This object is readonly.
                return layers.Count > 0;
            }

            public Layer Create(Wms wms)
            {
                // This object is readonly.
                Layer layer;
                if (layers.Count > 0)
                {
                    layer = new BranchLayer(wms);
                }
                else
                {
                    layer = new LeafLayer(Name, wms);
                }

                try
                {
                    layer.node = new TreeNode(Title);
                    layer.node.Tag = layer;
                    layer.node.Checked = true;
                    layer.node.ToolTipText = Description;
                    var crsIdsArray = new int[crsIds.Count];
                    layer.CrsIds = crsIdsArray;
                    crsIds.Keys.CopyTo(crsIdsArray, 0);
                    layer.ImageFormat = WmsCreator.GetBestSupportedFormat();

                    foreach (var creator in layers)
                    {
                        var subLayer = creator.Create(wms);
                        layer.Nodes.Add(subLayer.node);
                    }
                }
                catch (Exception)
                {
                    layer.Dispose();
                    throw;
                }

                return layer;
            }

            public void GeneralizeCrsIds()
            {
                if (HasSubLayer())
                {
                    foreach (var layer in layers)
                    {
                        layer.GeneralizeCrsIds();
                    }

                    var commonCrsIds = new CRSIds(layers.Peek().crsIds);
                    var temporaryIds = new CRSIds();
                    foreach (var layer in layers)
                    {
                        foreach (var id in layer.crsIds)
                        {
                            object obj;
                            if (commonCrsIds.TryGetValue(id.Key, out obj))
                            {
                                temporaryIds.Add(id.Key, null);
                            }
                        }

                        var temporary = commonCrsIds;
                        commonCrsIds = temporaryIds;
                        temporaryIds = temporary;
                        temporaryIds.Clear();

                        if (commonCrsIds.Count <= 0)
                        {
                            break;
                        }
                    }

                    if (commonCrsIds.Count > 0)
                    {
                        foreach (var layer in layers)
                        {
                            foreach (var id in commonCrsIds)
                            {
                                layer.crsIds.Remove(id.Key);
                            }
                        }

                        foreach (var id in commonCrsIds)
                        {
                            crsIds[id.Key] = null;
                        }
                    }
                }
            }

            /// <summary>
            /// Ids of all supported CRSs of the layer.
            /// </summary>
            private CRSIds crsIds = new CRSIds();

            /// <summary>
            /// Creators of Layer object.
            /// </summary>
            private LayerCreators layers = new LayerCreators();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Erase(WebRequestsPool pool)
        {
            if (pool != null)
            {
                EraseImage(pool);
            }
            else
            {
                EraseImage();
            }

            var parent = Parent;
            // TODO: The root will never be erased, even if there are no leafs
            if (parent != null)
            {
                // Layer is root
                // At first, SubLayer must be removed from list of layers and then can be disposed.
                parent.Nodes.Remove(node);
                parent.EraseIfEmpty();
            }
        }

        public abstract IEnumerator<Layer> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static Layer NodeToLayer(TreeNode node)
        {
            // Node is readonly.
            Debug.Assert(node != null, "The node associated with layer cannot be null!");
            var obj = node.Tag;
            Debug.Assert(obj != null, "Every TreeNode must have associated Layer!");
            var layer = obj as Layer;
            Debug.Assert(layer != null, "Object in TreeNode.Tag is not Layer!");

            return layer;
        }

        public BranchLayer Parent
        {
            get
            {
                var parentNode = node.Parent;
                if (parentNode != null)
                {
                    var layer = NodeToLayer(parentNode);
                    var branchLayer = layer as BranchLayer;
                    Debug.Assert(branchLayer != null, "Every parent is branch layer!");
                    return branchLayer;
                }
                else
                {
                    return null;
                }
            }
        }

        public Layer Root
        {
            get
            {
                var parent = Parent;
                if (parent != null)
                {
                    return parent.Root;
                }
                else
                {
                    return this;        // Root is layer without parent.
                }
            }
        }

        public Wms Wms { get; private set; }

        /// <summary>
        /// Gets title name.
        /// </summary>
        public string Title
        {
            get
            {
                return node.Text;
            }
            private set
            {
                Debug.Assert(value != null, "Layer title must be a text!");
                node.Text = value;
            }
        }

        /// <summary>
        /// Gets short description of the layer.
        /// </summary>
        public string Description
        {
            get
            {
                return node.ToolTipText;
            }
            private set
            {
                Debug.Assert(value != null, "Layer description must be a text!");
                node.ToolTipText = value;
            }
        }

        /// <summary>
        /// Gets ids of all supported CRSs of the layer.
        /// </summary>
        public ICollection<int> CrsIds { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether only one request is sent to server for the layer
        /// with entire tree of its sublayers (there is only one image).
        /// </summary>
        public abstract bool DownloadOneImage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether image is updated after every change of viewport.
        /// </summary>
        public bool AutoUpdate { get; set; }

        /// <summary>
        /// Gets or sets image format.
        /// </summary>
        public ImageFormat ImageFormat { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether requested image will be transparent
        /// if image format supports it.
        /// </summary>
        public bool IsTransparent { get; set; }

        /// <summary>
        /// Gets or sets transparent color.
        /// </summary>
        public Color BgColor { get; set; }

        public bool IsRoot()
        {
            // This object is readonly.
            return Parent == null;
        }

        public abstract bool IsLeaf();

        public bool IsVisible
        {
            get
            {
                return node.Checked;       // If the image is displayed or not (checkbox is checked).
            }
        }

        public virtual Layer FirstWithChildren(int number)
        {
            var parent = Parent;
            if (parent != null)
            {
                return parent.FirstWithChildren(number);
            }
            else
            {
                return null;
            }
        }

        public Uri ImageAddress
        {
            get
            {
                var currentImage = image;
                if (currentImage != null)
                {
                    return currentImage.GetImageAddress(Wms);
                }
                else
                {
                    return null;
                }
            }
        }

        public MapImage ReleaseImage()
        {
            return ExchangeImage(null);
        }

        public void SetImage(MapImage newImage)
        {
            var oldImage = ExchangeImage(newImage);
            if (oldImage != null)
            {
                oldImage.Dispose();
            }
        }

        public void SetOrErase(MapImage newImage)
        {
            // It erases image if other one is present.
            var oldImage = Interlocked.CompareExchange<MapImage>(ref image, newImage, null);
            if (oldImage != null)
            {
                oldImage.Dispose();
            }
        }

        public MapImage ExchangeImage(MapImage newImage)
        {
            return Interlocked.Exchange<MapImage>(ref image, newImage);
        }

        public void EraseImage(WebRequestsPool pool)
        {
            Debug.Assert(pool != null, "It is needed RequestPool to complete image detetion!");

            pool.RemoveRequest(this);
            EraseImage();
        }

        public void EraseImage()
        {
            var oldImage = ReleaseImage();
            if (oldImage != null)
            {
                oldImage.Dispose();
            }
        }

        public virtual void EraseImagesInTree(WebRequestsPool pool)
        {
            EraseImage(pool);
        }

        public bool IsCrsSupportedByParents()
        {
            // This object is readonly.
            return IsCrsSupportedByParents(Wms.CRSId);
        }

        public bool IsCrsSupportedByParents(int crsId)
        {
            // This object is readonly.
            var parent = Parent;
            if (parent != null)
            {
                foreach (var id in parent.CrsIds)
                {
                    if (id == crsId)
                    {
                        return true;
                    }
                }

                return parent.IsCrsSupportedByParents(crsId);
            }
            else
            {
                return false;
            }
        }

        public bool IsCrsSupportedByLayer()
        {
            // This object is readonly.
            return IsCrsSupportedByLayer(Wms.CRSId);
        }

        public bool IsCrsSupportedByLayer(int crsId)
        {
            // This object is readonly.
            foreach (var id in CrsIds)
            {
                if (id == crsId)
                {
                    return true;
                }
            }

            return false;
        }

        public abstract void GetVisibleLayerNames(StringBuilder names, bool supportedByParents);

        public void AddToTreeView(TreeView tree)
        {
            Debug.Assert(tree != null, "TreeView to append does not exist!");

            tree.Nodes.Add(node);
        }

        public virtual void Draw(IViewport view, Graphics canvas, WebRequestsPool pool)
        {
            // Viewport is readonly.
            Debug.Assert(view != null, "Viewport is missing!");
            Debug.Assert(canvas != null, "Canvas is missing!");
            Debug.Assert(pool != null, "WebRequestsPool is necessary for image downloading!");

            var builder = new StringBuilder();
            GetVisibleLayerNames(builder, IsCrsSupportedByParents());

            if (builder.Length > 0)
            {
                var newView = view.Copy();
                newView.ReduceViewBox(Wms.MaxWidth, Wms.MaxHeight);
                var newImage = new MapImage(builder.ToString(), Wms.CRSId, newView, ImageFormat,
                    IsTransparent, BgColor);

                try
                {
                    // We cannot work with variable directly,
                    // because another thread could rewrite it during the work.
                    var oldImage = ReleaseImage();
                    try
                    {
                        if (IsVisible)
                        {
                            // Are we downloading?
                            if (AutoUpdate && Wms.IsDownloadingEnabled)
                            {
                                if ((oldImage == null) || (!newImage.Equals(oldImage)))
                                {
                                    var requested = newImage;
                                    newImage = null;        // Do not dispose the object
                                    pool.PutRequest(this, requested, requested.GetImageAddress(Wms));
                                    if (oldImage != null)
                                    {
                                        oldImage.Draw(view, canvas);
                                    }
                                }
                                else
                                {
                                    // If the situation changes and there is requested the same image
                                    // that already exists in the layer, outdated downloading is running
                                    // at this moment and it must be interrupted.
                                    pool.RemoveRequest(this);
                                    oldImage.Draw(view, canvas);
                                }
                            }
                            else
                            {
                                // Downloading must be interrupted.
                                pool.RemoveRequest(this);
                                if (oldImage != null)
                                {
                                    // Use an image with different CRS is nonsense.
                                    if (oldImage.CRSEquals(newImage))
                                    {
                                        oldImage.Draw(view, canvas);
                                    }
                                    else
                                    {
                                        oldImage.Dispose();
                                        oldImage = null;        // Erase old image
                                    }
                                }
                            }
                        }
                        else
                        {
                            if ((oldImage != null) && (!newImage.Equals(oldImage)))
                            {
                                oldImage.Dispose();
                                oldImage = null;        // Erase old image
                            }
                        }
                    }
                    finally
                    {
                        // If another thread save new image, we prefer it.
                        SetOrErase(oldImage);
                    }
                }
                finally
                {
                    if (newImage != null)
                    {
                        newImage.Dispose();
                    }
                }
            }
            else
            {
                EraseImage(pool);
            }
        }

        public bool IsRequestedIndividually()
        {
            // This object is readonly.
            for (var layer = Parent; layer != null; layer = layer.Parent)
            {
                if (layer.DownloadOneImage)
                {
                    return false;       // It is part of any parent request.
                }
            }

            return true;
        }

        protected Layer(Wms wms)
        {
            // This object is readonly.
            Debug.Assert(wms != null, "Layer cannot exist without WMS!");
            Wms = wms;
            CrsIds = null;
            AutoUpdate = true;
            ImageFormat = ImageFormat.PNG;
            IsTransparent = true;
            BgColor = Color.White;
            node.Checked = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Erase(null);
            }
        }

        protected TreeNodeCollection Nodes     // List of all sublayers (Their order matters)
        {
            get
            {
                return node.Nodes;
            }
        }

        /// <summary>
        /// Object holding information about the image downloaded from the server.
        /// </summary>
        [NonSerialized]
        private MapImage image = null;

        /// <summary>
        /// Tree node associated with layer (same tree structure).
        /// </summary>
        private TreeNode node = new TreeNode();
    }
}
