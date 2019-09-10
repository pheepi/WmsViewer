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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Xml;

namespace WmsViewer
{
    public partial class ViewerWindow : Form
    {
        private const int WHEEL_DELTA = 120;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewerWindow" /> class.
        /// </summary>
        public ViewerWindow()
        {
            InitializeComponent();
        }

        private void ViewerWindowInit(object sender, EventArgs e)
        {
            Debug.Assert(sender == this, "Only window can invoke init event handler!");

            ShowCoordinates();
            var pool = WebRequestsPool.Pool;
            pool.SetCanvas(canvas);
        }

        private void ViewerWindowClosing(object sender, EventArgs e)
        {
            Debug.Assert(sender == this, "Only window can invoke closing event handler!");

            layers.BeginUpdate();
            try
            {
                foreach (TreeNode node in layers.Nodes)
                {
                    var layer = Layer.NodeToLayer(node);
                    using (layer)
                    {
                        layers.Nodes.Remove(node);
                    }
                }
            }
            finally
            {
                layers.EndUpdate();
            }

            WebRequestsPool.ResetPool();
        }

        private void ViewerWindowKeyDown(object sender, KeyEventArgs e)
        {
            Debug.Assert(sender == this, "Only window can invoke its own key down event handler!");

            if (e.Control)
            {
                if (e.KeyCode == Keys.Z)
                {
                    e.Handled = invoker.Undo();
                    if (e.Handled)
                    {
                        ShowCoordinates();
                        canvas.Invalidate(false);
                    }
                }
                else if (e.KeyCode == Keys.Y)
                {
                    e.Handled = invoker.Redo();
                    if (e.Handled)
                    {
                        ShowCoordinates();
                        canvas.Invalidate(false);
                    }
                }
            }
            else if (!e.Alt)
            {
                viewChangeButtons.ChangeAction(e.KeyCode);
            }
        }

        private void CanvasMouseClick(object sender, MouseEventArgs e)
        {
            Debug.Assert(sender == canvas, "Only canvas can invoke mouse click event handler!");

            if (e.Button == MouseButtons.Left)
            {
                if (viewChangeButtons.ClickAction(e.Location))
                {
                    ShowCoordinates();
                    canvas.Invalidate(false);
                }
            }
        }

        private void CanvasEnter(object sender, EventArgs e)
        {
            Debug.Assert(sender == canvas, "Only canvas can invoke mouse enter event handler!");

            canvas.Focus();
        }

        private void CanvasMouseDown(object sender, MouseEventArgs e)
        {
            Debug.Assert(sender == canvas, "Only canvas can invoke mouse down event handler!");

            if (e.Button == MouseButtons.Left)
            {
                if (viewChangeButtons.ClickDownAction(e.Location))
                {
                    ShowCoordinates();
                    canvas.Invalidate(false);
                }
            }
        }

        private void CanvasMouseUp(object sender, MouseEventArgs e)
        {
            Debug.Assert(sender == canvas, "Only canvas can invoke mouse up event handler!");

            if (e.Button == MouseButtons.Left)
            {
                if (viewChangeButtons.ClickUpAction(e.Location))
                {
                    ShowCoordinates();
                    canvas.Invalidate(false);
                }
            }
        }

        private void CanvasMouseWheel(object sender, MouseEventArgs e)
        {
            Debug.Assert(sender == canvas, "Only canvas can invoke its own mouse wheel event handler!");

            if (e.Delta != 0)
            {
                if (e.Delta > 0)
                {
                    for (int i = e.Delta; i > 0; i -= WHEEL_DELTA)
                    {
                        viewChangeButtons.ZoomIn(e.Location);
                    }
                }
                else
                {
                    for (int i = e.Delta; i < 0; i += WHEEL_DELTA)
                    {
                        viewChangeButtons.ZoomOut(e.Location);
                    }
                }

                ShowCoordinates();
                canvas.Invalidate(false);
            }
        }

        private void CanvasRedraw(object sender, PaintEventArgs e)
        {
            Debug.Assert(sender == canvas, "Only canvas can invoke its own redraw event handler!");

            for (var i = layers.Nodes.Count - 1; i >= 0; --i)
            {
                var node = layers.Nodes[i];
                var layer = Layer.NodeToLayer(node);
                // TODO: Use e.ClipRectangle to redraw only necessary part of canvas
                layer.Draw(viewport, e.Graphics, WebRequestsPool.Pool);
            }

            layerSettings.RefreshImageAddress();
        }

        private void CanvasResize(object sender, EventArgs e)
        {
            Debug.Assert(sender == canvas, "Only canvas can invoke resize event handler!");

            if (WindowState != FormWindowState.Minimized)
            {
                canvas.SuspendLayout();
                viewport.Resize(canvas.Width, canvas.Height);
                canvas.ResumeLayout(true);
                invoker.Reset();       // All positions are invalid after resize
                canvas.Invalidate();
            }
        }

        private void ShowCoordinates()
        {
            statusBar.Items[0].Text = string.Format(CultureInfo.CurrentUICulture,
                Properties.Resources.ScreenCoordinates + ": [{0}; {1}] - [{2}; {3}]",
                viewport.LeftCorner, viewport.BottomCorner, viewport.RightCorner, viewport.TopCorner);
        }

        private void NewWmsButtonClick(object sender, EventArgs e)
        {
            Debug.Assert(sender == newWmsButton, "Only button \"newWms\" can invoke this event handler!");

            var uriText = string.Empty;
            if (InputBox.ShowInputBox(Properties.Resources.NewWmsWindowTitle,
                Properties.Resources.EnterNewWmsUri, ref uriText) == DialogResult.OK)
            {
                Cursor = Cursors.WaitCursor;
                Refresh();

                UriBuilder uriBuilder = null;
                string message = null;
                try
                {
                    uriBuilder = new UriBuilder(uriText);
                }
                catch (UriFormatException)
                {
                    message = Properties.Resources.InsertedTextNotValidUri;
                }

                if (uriBuilder != null)
                {
                    uriBuilder.Query = string.Empty;        // Specific query is used in each request.
                    message = CreateNewWms(uriBuilder.Uri);
                }

                if (message != null)
                {
                    MessageBox.Show(message, Properties.Resources.NewWmsWindowTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                }

                Cursor = Cursors.Default;
            }
        }

        private string CreateNewWms(Uri uri)
        {
            Debug.Assert(uri != null, "We cannot create new WMS without service URI!");

            // TODO: Rewrite downloading to WebRequestPool
            using (var webClient = new WebClient())
            {
                var uriBuilder = new UriBuilder(uri);
                // Parameters to get WMS capabilities
                uriBuilder.Query = Wms.CAPABILITY_ADDRESS_QUERY;

                byte[] data = null;
                try
                {
                    data = webClient.DownloadData(uriBuilder.Uri);
                }
                catch (WebException)
                {
                    return Properties.Resources.WmsCapabilitiesDownloadingError;
                }

                var contentType = webClient.ResponseHeaders.Get("Content-Type");
                // Additional information is omitted.
                if (contentType.StartsWith("application/vnd.ogc.wms_xml", StringComparison.OrdinalIgnoreCase)
                    || contentType.StartsWith("application/xml", StringComparison.OrdinalIgnoreCase)
                    || contentType.StartsWith("text/xml", StringComparison.OrdinalIgnoreCase))
                {
                    var settings = new XmlReaderSettings();
                    settings.ProhibitDtd = false;
                    settings.IgnoreComments = true;
                    settings.IgnoreProcessingInstructions = true;
                    settings.IgnoreWhitespace = true;
                    settings.ValidationType = ValidationType.Schema;
                    using (var stream = new MemoryStream(data))
                    using (var reader = XmlReader.Create(stream, settings))
                    {
                        var parser = new CapabilitiesParser();
                        var creator = new Wms.WmsCreator();
                        try
                        {
                            parser.Parse(reader, creator);
                        }
                        catch (XmlException)
                        {
                            return Properties.Resources.NotValidXmlFile;
                        }
                        catch (XmlParser.XmlParserException e)
                        {
                            return e.Message;
                        }

                        creator.ServiceAddress = uri;
                        var wms = creator.Create();
                        var layer = wms.Layer;

                        layers.BeginUpdate();
                        try
                        {
                            layer.AddToTreeView(layers);
                        }
                        finally
                        {
                            layers.EndUpdate();
                        }

                        canvas.Invalidate(false);
                    }
                }
                else
                {
                    return Properties.Resources.FileNotInXmlFormat;
                }
            }

            return null;
        }

        private void LayersSelectChanged(object sender, TreeViewEventArgs e)
        {
            Debug.Assert(sender == layers, "Only TreeView can invoke the select event handler!");

            var layer = Layer.NodeToLayer(e.Node);
            layerSettings.Refresh(layer);
            wmsSettings.Refresh(layer.Wms);
        }

        private void LayersKeyDown(object sender, KeyEventArgs e)
        {
            Debug.Assert(sender == layers, "Only TreeView can invoke the key down event handler!");

            var node = layers.SelectedNode;
            if (node != null)
            {
                if (e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown || e.KeyCode == Keys.Delete)
                {
                    e.Handled = true;

                    var parent = node.Parent;
                    layers.BeginUpdate();
                    try
                    {
                        if (e.KeyCode != Keys.Delete)
                        {
                            var nodes = (parent != null) ? parent.Nodes : layers.Nodes;
                            var idx = nodes.IndexOf(node);

                            if (e.KeyCode == Keys.PageUp)
                            {
                                // The node is not top-most.
                                if (idx != 0)
                                {
                                    nodes.RemoveAt(idx);
                                    nodes.Insert(idx - 1, node);
                                    canvas.Invalidate(false);
                                }
                            }
                            else
                            {
                                // The node is not buttom-most.
                                if (idx != nodes.Count - 1)
                                {
                                    nodes.RemoveAt(idx);
                                    nodes.Insert(idx + 1, node);
                                    canvas.Invalidate(false);
                                }
                            }

                            layers.SelectedNode = node;
                        }
                        else
                        {
                            var layer = Layer.NodeToLayer(node);
                            layer.Erase(WebRequestsPool.Pool);
                            if (parent == null)
                            {
                                layers.Nodes.Remove(node);
                            }

                            // There is no event invoked after unselect
                            if (layers.SelectedNode == null)
                            {
                                layerSettings.Refresh(null);
                                wmsSettings.Refresh(null);
                            }

                            canvas.Invalidate(false);
                        }
                    }
                    finally
                    {
                        layers.EndUpdate();
                    }
                }
            }
        }

        private void LayersNodeChecked(object sender, TreeViewEventArgs e)
        {
            canvas.Invalidate(false);
        }

        private CentricView viewport;
        private PreviousViewports invoker;
    }
}
