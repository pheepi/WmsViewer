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

namespace WmsViewer
{
    using CRSNames = Stack<string>;
    using ImageFormats = SortedDictionary<ImageFormat, object>;

    public enum ProtocolVersion
    {
        v1_0_0 = 0,
        v1_0_7 = 1,
        v1_1_0 = 2,
        v1_1_1 = 3,
        v1_3_0 = 4
    }

    public enum ImageFormat
    {
        PNG = 0,
        PNG8 = 1,
        PNG24 = 2,
        PNG32 = 3,
        JPEG = 4,
        TIFF = 5,
        BMP = 6,
        GIF = 7
    }

    [Serializable]
    public class Wms
    {
        public class WmsCreator
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WmsCreator" /> class.
            /// </summary>
            public WmsCreator()
            {
                Title = string.Empty;
                Description = string.Empty;
                Version = ProtocolVersion.v1_3_0;
                MaxWidth = int.MaxValue;
                MaxHeight = int.MaxValue;
            }

            /// <summary>
            /// Gets or sets title name of WMS.
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets short description of the service.
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Gets version of the WMS protocol.
            /// </summary>
            public ProtocolVersion Version { get; private set; }

            /// <summary>
            /// Gets or sets maximum width of image.
            /// </summary>
            public int MaxWidth { get; set; }

            /// <summary>
            /// Gets or sets maximum height of image.
            /// </summary>
            public int MaxHeight { get; set; }

            /// <summary>
            /// Gets or sets HTTP address of access point to WMS.
            /// </summary>
            public Uri ServiceAddress { get; set; }

            /// <summary>
            /// Gets or sets root layer creator of the WMS.
            /// </summary>
            public Layer.LayerCreator Layer { get; set; }

            public bool SetVersion(string version)
            {
                // version is readonly.
                Debug.Assert(version != null, "Version can be determined only by a string!");
                for (var i = 0; i < WMS_VERSIONS.Length; ++i)
                {
                    if (WMS_VERSIONS[i].Equals(version))
                    {
                        Version = (ProtocolVersion)i;
                        return true;
                    }
                }

                return false;
            }

            public Layer.IWmsLayerCreator MakeLayerCreator()
            {
                Layer = new Layer.LayerCreator(this);
                return Layer;
            }

            public int AddCrs(string crs)
            {
                // crs is readonly.
                Debug.Assert(crs != null, "New CRS must have a name!");
                var position = 0;
                foreach (var name in systems)
                {
                    if (name.Equals(crs))
                    {
                        return position;
                    }
                    ++position;
                }

                systems.Push(crs);
                return position;
            }

            public void AddImageFormat(string format)
            {
                // format is readonly.
                for (var i = 0; i < IMAGE_FORMATS.Length; ++i)
                {
                    if (IMAGE_FORMATS[i].Equals(format))
                    {
                        imageFormats.Add((ImageFormat)i, null);
                        break;
                    }
                }
            }

            public void AddObsoleteImageFormat(string format)
            {
                // format is readonly.
                for (int i = 0; i < IMAGE_FORMATS_OBSOLETE.Length; ++i)
                {
                    if (IMAGE_FORMATS[i].Equals(format))
                    {
                        imageFormats.Add((ImageFormat)i, null);
                    }
                }
            }

            public ImageFormat GetBestSupportedFormat()
            {
                // This object is readonly.
                Debug.Assert(ExistsAnyFormat(), "There needs to be any image format!");
                foreach (var format in imageFormats.Keys)
                {
                    return format;
                }

                return ImageFormat.PNG;     // Never returned;
            }

            public bool ExistsAnyFormat()
            {
                return imageFormats.Count > 0;
            }

            public Wms Create()
            {
                var wms = new Wms();
                Debug.Assert(ServiceAddress != null, "Every WMS has its source address!");
                wms.ServiceAddress = ServiceAddress;
                wms.Title = Title;
                wms.Description = Description;
                wms.version = Version;
                wms.MaxWidth = MaxWidth;
                wms.MaxHeight = MaxHeight;
                Debug.Assert(systems.Count > 0, "There is no CRS in creator!");
                wms.systems = systems.ToArray();
                wms.crsId = SetPreferredCrs();
                Debug.Assert(imageFormats.Count > 0, "There is no image format in creator!");
                var imageFormatsArray = new ImageFormat[imageFormats.Count];
                wms.ImageFormats = imageFormatsArray;
                imageFormats.Keys.CopyTo(imageFormatsArray, 0);
                Debug.Assert(Layer != null, "Every WMS creator must also create a root layer!");
                wms.Layer = Layer.Create(wms);

                return wms;
            }

            private static readonly string[] PREFERRED_CRS = { "EPSG:4326", "CRS:84" };

            private int SetPreferredCrs()
            {
                foreach (var crs in PREFERRED_CRS)
                {
                    var position = 0;
                    foreach (var system in systems)
                    {
                        if (system.Equals(crs))
                        {
                            return position;
                        }

                        ++position;
                    }
                }

                return 0;       // No preferred CRS, choose the first one.
            }

            /// <summary>
            /// Names of all supported CRSs of the layer.
            /// </summary>
            private CRSNames systems = new CRSNames();

            /// <summary>
            /// All image formats supported by the program as well as service.
            /// </summary>
            private ImageFormats imageFormats = new ImageFormats();
        }

        public const string CAPABILITY_ADDRESS_QUERY = "service=WMS&request=GetCapabilities";

        public static bool AbleToBeTransparent(ImageFormat imageFormat)
        {
            Debug.Assert(ImageFormatsCount == ABLE_TO_BE_TRANSPARENT.Length,
                "There is no correct information about transparency!");
            return ABLE_TO_BE_TRANSPARENT[(int)imageFormat];
        }

        public Uri ServiceAddress { get; private set; }      // HTTP address of access point to WMS

        public Layer Layer { get; private set; }      // Root layer of the WMS

        public string Version
        {
            get
            {
                Debug.Assert(VersionsCount == WMS_VERSIONS.Length,
                    "There is not correct number of version strings!");
                return WMS_VERSIONS[(int)version];
            }
        }

        public int CRSId
        {
            get
            {
                Debug.Assert(crsId >= 0 && crsId < systems.Length, "ID of current CRS is wrong!");
                return crsId;
            }
            set
            {
                crsId = value;
                Debug.Assert(crsId >= 0 && crsId < systems.Length, "ID of set CRS is wrong!");
            }
        }

        /// <summary>
        /// Gets all image formats supported by the program as well as service.
        /// </summary>
        public ImageFormat[] ImageFormats { get; private set; }

        /// <summary>
        /// Gets title name of WMS.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets short description of the service.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets maximum width of image.
        /// </summary>
        public int MaxWidth { get; private set; }

        /// <summary>
        /// Gets maximum height of image.
        /// </summary>
        public int MaxHeight { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether images are downloaded from internet or not.
        /// </summary>
        public bool IsDownloadingEnabled { get; set; }

        public int CRSCount
        {
            get
            {
                return systems.Length;
            }
        }

        public string GetCRSName(int id)
        {
            // This object is readonly.
            Debug.Assert(id >= 0 && id < systems.Length, "ID of CRS is wrong!");
            return systems[id];
        }

        public string CRSVariableName
        {
            get
            {
                if (version > ProtocolVersion.v1_1_1)
                {
                    return "crs";
                }
                else
                {
                    return "srs";
                }
            }
        }

        public bool IsCrsReversed
        {
            get
            {
                Debug.Assert(crsId >= 0 && crsId < systems.Length, "ID of current CRS is wrong!");
                // TODO: Complicated, confirmed for 4326 and 4258, but not others (102067, for instance)
                return version >= ProtocolVersion.v1_3_0
                    && systems[crsId].StartsWith(CRS_DATABASE_ABBR + ':',
                    StringComparison.OrdinalIgnoreCase);
            }
        }

        public string GetImageFormat(ImageFormat imageFormat)
        {
            // This object is readonly.
            if (version > ProtocolVersion.v1_0_7)
            {
                Debug.Assert(ImageFormatsCount == IMAGE_FORMATS.Length,
                    "There is not correct number of image format strings!");
                return IMAGE_FORMATS[(int)imageFormat];
            }
            else
            {
                Debug.Assert(ImageFormatsCount == IMAGE_FORMATS_OBSOLETE.Length,
                    "There is not correct number of obsolete image format strings!");
                return IMAGE_FORMATS_OBSOLETE[(int)imageFormat];
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="Wms" /> class from being created.
        /// </summary>
        private Wms()
        {
            Title = string.Empty;
            Description = string.Empty;
            MaxWidth = int.MaxValue;
            MaxHeight = int.MaxValue;
            IsDownloadingEnabled = true;
        }

        private static int VersionsCount
        {
            get
            {
                return Enum.GetValues(typeof(ProtocolVersion)).Length;
            }
        }

        private static int ImageFormatsCount
        {
            get
            {
                return Enum.GetValues(typeof(ImageFormat)).Length;
            }
        }

        private const string CRS_DATABASE_ABBR = "EPSG";
        private static readonly string[] WMS_VERSIONS = { "1.0.0", "1.0.7", "1.1.0", "1.1.1", "1.3.0" };
        private static readonly string[] IMAGE_FORMATS
            = { "image/png", "image/png8", "image/png24", "image/png32", "image/jpeg", "image/tiff",
                  "image/bmp", "image/gif" };
        private static readonly string[] IMAGE_FORMATS_OBSOLETE
            = { "PNG", "PNG8", "PNG24", "PNG32", "JPEG", "TIFF", "BMP", "GIF" };
        private static readonly bool[] ABLE_TO_BE_TRANSPARENT
            = { true, true, true, true, false, false, false, true };

        /// <summary>
        /// Version of the WMS protocol.
        /// </summary>
        private ProtocolVersion version = ProtocolVersion.v1_3_0;

        /// <summary>
        /// All supported CRSs.
        /// </summary>
        private string[] systems;

        /// <summary>
        /// Current chosen CRS.
        /// </summary>
        private int crsId;
    }
}
