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
using System.Text;

namespace WmsViewer
{
    [Serializable]
    public class LeafLayer : Layer
    {
        public LeafLayer(string layerName, Wms wms)
            : base(wms)
        {
            Debug.Assert(!string.IsNullOrEmpty(layerName), "The leaf layer must have a name!");
            name = layerName;
        }

        public override IEnumerator<Layer> GetEnumerator()
        {
            return new List<Layer>().GetEnumerator();
        }

        public override bool DownloadOneImage
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        public override bool IsLeaf()
        {
            return true;
        }

        public override Layer FirstWithChildren(int number)
        {
            // Leaf layer has no child.
            if (number <= 0)
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
            // Current CRS is supported by the leaf layer or its parents and its name is returned.
            if (supportedByParents || IsCrsSupportedByLayer())
            {
                Debug.Assert(names != null, "The names parameter must be valid!");
                if (names.Length > 0)
                {
                    names.Append(NAME_DELIMITER);
                }

                names.Append(name);
            }
        }

        /// <summary>
        /// Delimiter of layer names in web address parameter.
        /// </summary>
        private const char NAME_DELIMITER = ',';

        /// <summary>
        /// Name of the layer (used for request to the service, useful to save it only in leafs).
        /// </summary>
        private string name = string.Empty;
    }
}
