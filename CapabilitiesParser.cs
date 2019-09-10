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
using System.Runtime.Serialization;
using System.Xml;

namespace WmsViewer
{
    public abstract class XmlParser
    {
        [Serializable]
        public class XmlParserException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="XmlParserException" /> class.
            /// </summary>
            public XmlParserException() : this(null) { }

            public XmlParserException(string message) : this(message, null) { }

            public XmlParserException(string message, Exception innerException)
                : base("The error has occurred when processing WMS capabilities"
                + ((message != null) ? (": " + message) : "!"), innerException)
            {
            }

            protected XmlParserException(SerializationInfo info, StreamingContext context)
                : base(info, context) { }
        }

        public void Parse(XmlReader reader, Wms.WmsCreator creator)
        {
            Debug.Assert(creator != null, "XML parser needs WMSCreator as parameter!");

            Initialization(creator);
            try
            {
                Debug.Assert(reader != null, "XML reader must be valid!");
                while (IsRunning && reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            ParseStartElement(reader);
                            break;
                        case XmlNodeType.EndElement:
                            ParseEndElement(reader);
                            break;
                        case XmlNodeType.Text:
                        case XmlNodeType.CDATA:
                            ParseText(reader);
                            break;
                        default:
                            break;
                    }
                }

                if (IsRunning)
                {
                    throw new XmlException("Unexpected end of XML file!");
                }
            }
            finally
            {
                Finalization();
            }
        }

        protected XmlParser()
        {
            IsRunning = true;
        }

        protected abstract void Initialization(Wms.WmsCreator wmsCreator);

        protected abstract void Finalization();

        protected abstract void ParseStartElement(XmlReader reader);

        protected abstract void ParseEndElement(XmlReader reader);

        protected abstract void ParseText(XmlReader reader);

        protected bool IsRunning { get; set; }
    }

    public class CapabilitiesParser : XmlParser
    {
        private enum Tag
        {
            NO_TAG = 0,
            CAPABILITY_FORMAT,
            SERVICE_INFO,
            CAPABILITY,
            REQUEST,
            GET_MAP,
            TITLE,
            ABSTRACT,
            WIDTH,
            HEIGHT,
            FORMAT
        }

        protected override void Initialization(Wms.WmsCreator wmsCreator)
        {
            creator = wmsCreator;
        }

        protected override void Finalization()
        {
            creator = null;
        }

        protected override void ParseStartElement(XmlReader reader)
        {
            Debug.Assert(reader != null, "XML capability reader must be initialized when parsing start element!");
            Debug.Assert(creator != null, "WMSCreater must be initialized when parsing start element!");

            if (stack[0u] == Tag.CAPABILITY_FORMAT)
            {
                if (stack[1u] == Tag.SERVICE_INFO)
                {
                    if (reader.Name.Equals("Title"))
                    {
                        stack[2u] = Tag.TITLE;
                    }
                    else if (reader.Name.Equals("Abstract"))
                    {
                        stack[2u] = Tag.ABSTRACT;
                    }
                    else if (creator.Version > ProtocolVersion.v1_1_1)
                    {
                        if (reader.Name.Equals("MaxWidth"))
                        {
                            stack[2] = Tag.WIDTH;
                        }
                        else if (reader.Name.Equals("MaxHeight"))
                        {
                            stack[2] = Tag.HEIGHT;
                        }
                    }
                }
                else if (stack[1u] == Tag.CAPABILITY)
                {
                    if (stack[2u] == Tag.REQUEST)
                    {
                        if (stack[3u] == Tag.GET_MAP)
                        {
                            if (stack[4u] == Tag.FORMAT && creator.Version <= ProtocolVersion.v1_0_7)
                            {
                                creator.AddObsoleteImageFormat(reader.Name);
                            }
                            else if (reader.Name.Equals("Format"))
                            {
                                stack[4u] = Tag.FORMAT;
                            }
                        }
                        else if ((reader.Name.Equals("GetMap") && creator.Version > ProtocolVersion.v1_0_7)
                            || (reader.Name.Equals("Map") && creator.Version <= ProtocolVersion.v1_0_7))
                        {
                            stack[3u] = Tag.GET_MAP;
                        }
                    }
                    else if (reader.Name.Equals("Request"))
                    {
                        stack[2u] = Tag.REQUEST;
                    }
                    else if (reader.Name.Equals("Layer"))
                    {
                        if (!creator.ExistsAnyFormat())
                        {
                            throw new NoImageFormatsException();
                        }

                        var layerParser = new LayerParser();
                        layerParser.Parse(reader, creator);
                    }
                }
                else if (reader.Name.Equals("Service"))
                {
                    stack[1u] = Tag.SERVICE_INFO;
                }
                else if (reader.Name.Equals("Capability"))
                {
                    stack[1u] = Tag.CAPABILITY;
                }
            }
            else if (reader.Name.Equals("WMS_Capabilities")
                || reader.Name.Equals("WMT_MS_Capabilities"))
            {
                var version = reader["version"];
                if (version != null)
                {
                    if (creator.SetVersion(version))
                    {
                        if ((reader.Name.Equals("WMS_Capabilities")
                            && creator.Version > ProtocolVersion.v1_1_1)
                            || (reader.Name.Equals("WMT_MS_Capabilities")
                            && creator.Version <= ProtocolVersion.v1_1_1))
                        {
                            stack[0u] = Tag.CAPABILITY_FORMAT;
                        }
                        else
                        {
                            throw new WrongRootTagException();
                        }
                    }
                    else
                    {
                        throw new VersionNotSupportedException();
                    }
                }
                else
                {
                    throw new VersionNotQuotedException();
                }
            }
            else
            {
                throw new WrongRootTagException();
            }
        }

        protected override void ParseEndElement(XmlReader reader)
        {
            Debug.Assert(reader != null, "XML capability reader must be initialized when parsing end element!");
            Debug.Assert(creator != null, "WMSCreater must be initialized when parsing end element!");

            if (stack[0u] == Tag.CAPABILITY_FORMAT)
            {
                if (stack[1u] == Tag.SERVICE_INFO)
                {
                    if (stack[2u] == Tag.TITLE)
                    {
                        if (reader.Name.Equals("Title"))
                        {
                            stack[2u] = Tag.NO_TAG;     // End of TITLE
                        }
                    }
                    else if (stack[2u] == Tag.ABSTRACT)
                    {
                        if (reader.Name.Equals("Abstract"))
                        {
                            stack[2u] = Tag.NO_TAG;     // End of ABSTRACT
                        }
                    }
                    else if (stack[2u] == Tag.WIDTH)        // Version 1.3.0 and higher
                    {
                        if (reader.Name.Equals("MaxWidth"))
                        {
                            stack[2u] = Tag.NO_TAG;     // End of WIDTH
                        }
                    }
                    else if (stack[2u] == Tag.HEIGHT)       // Version 1.3.0 and higher
                    {
                        if (reader.Name.Equals("MaxHeight"))
                        {
                            stack[2u] = Tag.NO_TAG;     // End of HEIGHT
                        }
                    }
                    else if (reader.Name.Equals("Service"))
                    {
                        stack[1u] = Tag.NO_TAG;     // End of SERVICE_INFO
                    }
                }
                else if (stack[1u] == Tag.CAPABILITY)
                {
                    if (stack[2u] == Tag.REQUEST)
                    {
                        if (stack[3u] == Tag.GET_MAP)
                        {
                            if (stack[4u] == Tag.FORMAT)
                            {
                                if (reader.Name.Equals("Format"))
                                {
                                    stack[4u] = Tag.NO_TAG;     // End of FORMAT
                                }
                            }
                            else if ((reader.Name.Equals("GetMap")
                                && creator.Version > ProtocolVersion.v1_0_7)
                                || (reader.Name.Equals("Map") && creator.Version <= ProtocolVersion.v1_0_7))
                            {
                                stack[3u] = Tag.NO_TAG;     // End of GET_MAP
                            }
                        }
                        else if (reader.Name.Equals("Request"))
                        {
                            stack[2u] = Tag.NO_TAG;     // End of REQUEST
                        }
                    }
                    else if (reader.Name.Equals("Capability"))
                    {
                        stack[1u] = Tag.NO_TAG;     // End of CAPABILITY
                    }
                }
                else if ((reader.Name.Equals("WMS_Capabilities") && creator.Version > ProtocolVersion.v1_1_1)
                    || (reader.Name.Equals("WMT_MS_Capabilities")
                    && creator.Version <= ProtocolVersion.v1_1_1))
                {
                    stack[0u] = Tag.NO_TAG;     // End of CAPABILITY_FORMAT
                    if (creator.Layer == null)
                    {
                        throw new NoRootLayerException();
                    }

                    creator.Layer.GeneralizeCrsIds();
                    if (!creator.Layer.HasCrs())
                    {
                        throw new NoGlobalCrsException();
                    }

                    IsRunning = false;
                }
            }
        }

        protected override void ParseText(XmlReader reader)
        {
            Debug.Assert(reader != null, "XML capability reader must be initialized when parsing text!");
            Debug.Assert(creator != null, "WMSCreater must be initialized when parsing text!");

            if (stack[1u] == Tag.SERVICE_INFO)
            {
                if (stack[2u] == Tag.TITLE)
                {
                    creator.Title = reader.Value;
                }
                else if (stack[2u] == Tag.ABSTRACT)
                {
                    creator.Description = reader.Value;
                }
                else if (stack[2u] == Tag.WIDTH)
                {
                    int width;
                    if (int.TryParse(reader.Value, out width))
                    {
                        creator.MaxWidth = width;
                    }
                    else
                    {
                        throw new InvalidMaxWidthException();
                    }
                }
                else if (stack[2u] == Tag.HEIGHT)
                {
                    int height;
                    if (int.TryParse(reader.Value, out height))
                    {
                        creator.MaxHeight = height;
                    }
                    else
                    {
                        throw new InvalidMaxHeightException();
                    }
                }
            }
            else if ((stack[4u] == Tag.FORMAT) && (creator.Version > ProtocolVersion.v1_0_7))
            {
                creator.AddImageFormat(reader.Value);
            }
        }

        [Serializable]
        private class WrongRootTagException : XmlParserException
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WrongRootTagException" /> class.
            /// </summary>
            public WrongRootTagException() : base("Root tag has wrong name!") { }

            protected WrongRootTagException(SerializationInfo info, StreamingContext context)
                : base(info, context) { }
        }

        [Serializable]
        private class VersionNotQuotedException : XmlParserException
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VersionNotQuotedException" /> class.
            /// </summary>
            public VersionNotQuotedException() : base("Version of WMS is not quoted!") { }

            protected VersionNotQuotedException(SerializationInfo info, StreamingContext context)
                : base(info, context) { }
        }

        [Serializable]
        private class VersionNotSupportedException : XmlParserException
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VersionNotSupportedException" /> class.
            /// </summary>
            public VersionNotSupportedException() : base("Version of WMS is not supported!") { }

            protected VersionNotSupportedException(SerializationInfo info, StreamingContext context)
                : base(info, context) { }
        }

        [Serializable]
        private class InvalidMaxWidthException : XmlParserException
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="InvalidMaxWidthException" /> class.
            /// </summary>
            public InvalidMaxWidthException()
                : base("Value of image maximal width is not a natural number!") { }

            protected InvalidMaxWidthException(SerializationInfo info, StreamingContext context)
                : base(info, context) { }
        }

        [Serializable]
        private class InvalidMaxHeightException : XmlParserException
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="InvalidMaxHeightException" /> class.
            /// </summary>
            public InvalidMaxHeightException()
                : base("Value of image maximal height is not a natural number!") { }

            protected InvalidMaxHeightException(SerializationInfo info, StreamingContext context)
                : base(info, context) { }
        }

        [Serializable]
        private class NoRootLayerException : XmlParserException
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="NoRootLayerException" /> class.
            /// </summary>
            public NoRootLayerException() : base("There does not exist root layer!") { }

            protected NoRootLayerException(SerializationInfo info, StreamingContext context)
                : base(info, context) { }
        }

        [Serializable]
        private class NoGlobalCrsException : XmlParserException
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="NoGlobalCrsException" /> class.
            /// </summary>
            public NoGlobalCrsException() : base("WMS service does not support any global CRS!") { }

            protected NoGlobalCrsException(SerializationInfo info, StreamingContext context)
                : base(info, context) { }
        }

        [Serializable]
        private class NoImageFormatsException : XmlParserException
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="NoImageFormatsException" /> class.
            /// </summary>
            public NoImageFormatsException() : base("WMS service does not support any image format!") { }

            protected NoImageFormatsException(SerializationInfo info, StreamingContext context)
                : base(info, context) { }
        }

        /// <summary>
        /// The maximum stack size to represent every possible position.
        /// </summary>
        private const int MAX_STACK_DEPTH = 5;

        private Wms.WmsCreator creator;
        private Tag[] stack = new Tag[MAX_STACK_DEPTH];
    }

    public class LayerParser : XmlParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LayerParser" /> class.
        /// </summary>
        public LayerParser()
            : this(null)
        {
        }

        public LayerParser(Layer.IWmsLayerCreator parentCreator)
        {
            parent = parentCreator;
        }

        private enum Tag
        {
            NO_TAG = 0,
            NAME,
            TITLE,
            ABSTRACT,
            CRS,
            BOUNDING_BOX,
            STYLE
        }

        protected override void Initialization(Wms.WmsCreator wmsCreator)
        {
            Debug.Assert(wmsCreator != null, "XML capabilities reader must be initialized when layer parsing starts!");

            if (parent != null)
            {
                layerCreator = parent.AddLayer();
            }
            else
            {
                layerCreator = wmsCreator.MakeLayerCreator();
            }
        }

        protected override void Finalization()
        {
            layerCreator = null;
        }

        protected override void ParseStartElement(XmlReader reader)
        {
            Debug.Assert(reader != null, "XML layer reader must be initialized when parsing start element!");

            if (tag != Tag.STYLE)
            {
                if (reader.Name.Equals("Layer"))
                {
                    var layerParser = new LayerParser(layerCreator);
                    layerParser.Parse(reader, layerCreator.WmsCreator);
                }
                else if (reader.Name.Equals("Name"))
                {
                    tag = Tag.NAME;
                }
                else if (reader.Name.Equals("Title"))
                {
                    tag = Tag.TITLE;
                }
                else if (reader.Name.Equals("Abstract"))
                {
                    tag = Tag.ABSTRACT;
                }
                else if ((reader.Name.Equals("CRS")
                    && layerCreator.WmsCreator.Version > ProtocolVersion.v1_1_1)
                    || (reader.Name.Equals("SRS")
                    && layerCreator.WmsCreator.Version <= ProtocolVersion.v1_1_1))
                {
                    tag = Tag.CRS;
                }
                else if (reader.Name.Equals("BoundingBox"))
                {
                    tag = Tag.BOUNDING_BOX;
                    // Not implemented
                }
                else if (reader.Name.Equals("Style"))
                {
                    tag = Tag.STYLE;
                    // Not implemented
                }
            }
        }

        protected override void ParseEndElement(XmlReader reader)
        {
            Debug.Assert(reader != null, "XML layer reader must be initialized when parsing end element!");

            if (tag == Tag.STYLE)
            {
                if (reader.Name.Equals("Style"))
                {
                    tag = Tag.NO_TAG;       // End of STYLE
                }
            }
            else
            {
                if (reader.Name.Equals("Layer"))
                {
                    if ((layerCreator.Name.Length <= 0) && (!layerCreator.HasSubLayer()))
                    {
                        // Leaf layer must have a name to request it!
                        throw new LeafLayerWithoutNameException();
                    }

                    IsRunning = false;
                }
                else if (reader.Name.Equals("Name"))
                {
                    tag = Tag.NO_TAG;       // End of NAME
                }
                else if (reader.Name.Equals("Title"))
                {
                    tag = Tag.NO_TAG;       // End of TITLE
                }
                else if (reader.Name.Equals("Abstract"))
                {
                    tag = Tag.NO_TAG;       // End of ABSTRACT
                }
                else if ((reader.Name.Equals("CRS")
                    && layerCreator.WmsCreator.Version > ProtocolVersion.v1_1_1)
                    || (reader.Name.Equals("SRS")
                    && layerCreator.WmsCreator.Version <= ProtocolVersion.v1_1_1))
                {
                    tag = Tag.NO_TAG;       // End of CRS
                }
                else if (reader.Name.Equals("BoundingBox"))
                {
                    tag = Tag.NO_TAG;       // End of BOUNDING_BOX
                }
            }
        }

        protected override void ParseText(XmlReader reader)
        {
            Debug.Assert(reader != null, "XML layer reader must be initialized when parsing text!");

            if (tag == Tag.NAME)
            {
                layerCreator.Name = reader.Value;
            }
            else if (tag == Tag.TITLE)
            {
                layerCreator.Title = reader.Value;
            }
            else if (tag == Tag.ABSTRACT)
            {
                layerCreator.Description = reader.Value;
            }
            else if (tag == Tag.CRS)
            {
                layerCreator.AddCrs(reader.Value);
            }
        }

        [Serializable]
        private class LeafLayerWithoutNameException : XmlParserException
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LeafLayerWithoutNameException" /> class.
            /// </summary>
            public LeafLayerWithoutNameException() : base("A leaf layer is without a name!") { }

            protected LeafLayerWithoutNameException(SerializationInfo info, StreamingContext context)
                : base(info, context) { }
        }

        private Layer.IWmsLayerCreator parent;
        private Layer.IWmsLayerCreator layerCreator;
        private Tag tag = Tag.NO_TAG;
    }
}
