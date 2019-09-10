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
using System.Windows.Forms;

namespace WmsViewer
{
    [Serializable]
    public class BranchLayer : Layer
    {
        private class LayerEnumarator : IEnumerator<Layer>
        {
            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }

            public LayerEnumarator(TreeNodeCollection collection)
            {
                Debug.Assert(collection != null, "Layer enumerator needs collection to iterate!");

                iterator = collection.GetEnumerator();
            }

            public Layer Current
            {
                get
                {
                    return Layer.NodeToLayer(iterator.Current as TreeNode);
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public bool MoveNext()
            {
                return iterator.MoveNext();
            }

            public void Reset()
            {
                iterator.Reset();
            }

            private IEnumerator iterator;
        }

        public BranchLayer(Wms wms)
            : base(wms)
        {
            // Not used
        }

        public void EraseIfEmpty()
        {
            if (!HasSubLayers())
            {
                Dispose();
            }
        }

        public override void EraseImagesInTree(WebRequestsPool pool)
        {
            base.EraseImagesInTree(pool);
            for (int i = Nodes.Count; i > 0; --i)
            {
                var layer = NodeToLayer(Nodes[i - 1]);
                layer.EraseImagesInTree(pool);
            }
        }

        public override IEnumerator<Layer> GetEnumerator()
        {
            return new LayerEnumarator(Nodes);
        }

        public override bool DownloadOneImage
        {
            get
            {
                return downloadOneImage;
            }
            set
            {
                downloadOneImage = value;
            }
        }

        public override bool IsLeaf()
        {
            return false;
        }

        public override Layer FirstWithChildren(int number)
        {
            // Branch layer must have at least one child.
            if (Nodes.Count >= number)
            {
                return this;
            }
            else
            {
                return base.FirstWithChildren(number);
            }
        }

        public override void GetVisibleLayerNames(StringBuilder names, bool supportedByParents)
        {
            if (!supportedByParents)
            {
                supportedByParents = IsCrsSupportedByLayer();
            }

            foreach (TreeNode node in Nodes)
            {
                var layer = NodeToLayer(node);
                if (layer.IsVisible)
                {
                    layer.GetVisibleLayerNames(names, supportedByParents);
                }
            }
        }

        public override void Draw(IViewport view, Graphics canvas, WebRequestsPool pool)
        {
            // Viewport is readonly.
            if (DownloadOneImage)
            {
                foreach (TreeNode node in Nodes)
                {
                    var layer = NodeToLayer(node);
                    layer.EraseImagesInTree(pool);
                }

                base.Draw(view, canvas, pool);
            }
            else
            {
                EraseImage(pool);
                foreach (TreeNode node in Nodes)
                {
                    var layer = NodeToLayer(node);
                    layer.Draw(view, canvas, pool);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    while (HasSubLayers())
                    {
                        NodeToLayer(Nodes[Nodes.Count - 1]).Dispose();
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private bool HasSubLayers()
        {
            // This object is readonly.
            return Nodes.Count > 0;
        }

        /// <summary>
        /// Only one request is sent to server for the layer with entire tree
        /// of its sublayers (there is only one image).
        /// </summary>
        private bool downloadOneImage = true;
    }
}
