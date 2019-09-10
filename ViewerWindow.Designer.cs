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
using System.Drawing;
using System.Windows.Forms;

namespace WmsViewer
{
    public partial class ViewerWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise false.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components != null)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            canvas = new PictureBox();
            canvas.Size = new Size(530, 528);
            viewport = new CentricView(canvas.Width, canvas.Height, 0, 0, 1);
            invoker = new PreviousViewports();
            viewChangeButtons = new WmsViewer.ViewChangeButtons(viewport, invoker, canvas);
            newWmsButton = new Button();
            layerSettings = new WmsViewer.LayerSettings(canvas);
            wmsSettings = new WmsViewer.WmsSettings(canvas);
            layers = new TreeView();
            statusBar = new StatusStrip();
            coordinatesLabel = new ToolStripStatusLabel();
            ((ISupportInitialize)canvas).BeginInit();
            statusBar.SuspendLayout();
            SuspendLayout();
            // 
            // canvas
            // 
            canvas.Anchor = (AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom)
                | AnchorStyles.Left) | AnchorStyles.Right);
            canvas.BackColor = Color.White;
            canvas.Location = new Point(0, 0);
            canvas.Name = "canvas";
            canvas.TabIndex = 0;
            canvas.TabStop = false;
            canvas.Paint += new PaintEventHandler(CanvasRedraw);
            canvas.MouseClick += new MouseEventHandler(CanvasMouseClick);
            canvas.MouseEnter += new EventHandler(CanvasEnter);
            canvas.MouseDown += new MouseEventHandler(CanvasMouseDown);
            canvas.MouseUp += new MouseEventHandler(CanvasMouseUp);
            canvas.MouseWheel += new MouseEventHandler(CanvasMouseWheel);
            canvas.Resize += new EventHandler(CanvasResize);
            // 
            // viewChangeButtons
            // 
            viewChangeButtons.Anchor = (AnchorStyles)(AnchorStyles.Top | AnchorStyles.Right);
            viewChangeButtons.Location = new Point(532, 4);
            viewChangeButtons.Name = "viewChangeButtons1";
            viewChangeButtons.Size = new Size(150, 36);
            viewChangeButtons.TabIndex = 8;
            viewChangeButtons.Text = string.Empty;
            // 
            // newWmsButton
            // 
            newWmsButton.Anchor = (AnchorStyles)(AnchorStyles.Top | AnchorStyles.Right);
            newWmsButton.Location = new Point(686, 11);
            newWmsButton.Name = "newWmsButton";
            newWmsButton.Size = new Size(73, 23);
            newWmsButton.TabIndex = 4;
            newWmsButton.Text = Properties.Resources.NewWms;
            newWmsButton.Click += new EventHandler(NewWmsButtonClick);
            // 
            // layerSettings
            // 
            layerSettings.Anchor = (AnchorStyles)(AnchorStyles.Top | AnchorStyles.Right);
            layerSettings.BorderStyle = BorderStyle.FixedSingle;
            layerSettings.Enabled = false;
            layerSettings.Location = new Point(531, 216);
            layerSettings.MinimumSize = new Size(228, 236);
            layerSettings.Name = "layerSettings";
            layerSettings.Size = new Size(228, 236);
            layerSettings.TabIndex = 6;
            // 
            // wmsSettings
            // 
            wmsSettings.Anchor = (AnchorStyles)(AnchorStyles.Top | AnchorStyles.Right);
            wmsSettings.BorderStyle = BorderStyle.FixedSingle;
            wmsSettings.Enabled = false;
            wmsSettings.Location = new Point(531, 46);
            wmsSettings.MinimumSize = new Size(228, 164);
            wmsSettings.Name = "wmsSettings";
            wmsSettings.Size = new Size(228, 164);
            wmsSettings.TabIndex = 5;
            // 
            // layers
            // 
            layers.Anchor = (AnchorStyles)((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Right);
            layers.CheckBoxes = true;
            layers.Location = new Point(531, 458);
            layers.Name = "layers";
            layers.Size = new Size(228, 68);
            layers.TabIndex = 7;
            layers.AfterCheck += new TreeViewEventHandler(LayersNodeChecked);
            layers.AfterSelect += new TreeViewEventHandler(LayersSelectChanged);
            layers.KeyDown += new KeyEventHandler(LayersKeyDown);
            // 
            // statusBar
            // 
            statusBar.Items.AddRange(new ToolStripItem[] {
            coordinatesLabel});
            statusBar.Location = new Point(0, 527);
            statusBar.Name = "statusBar";
            statusBar.Size = new Size(760, 22);
            statusBar.TabIndex = 0;
            // 
            // coordinatesLabel
            // 
            coordinatesLabel.Name = "coordinatesLabel";
            coordinatesLabel.Size = new Size(0, 17);
            // 
            // ViewerWindow
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new Size(760, 549);
            Controls.Add(statusBar);
            Controls.Add(layers);
            Controls.Add(layerSettings);
            Controls.Add(wmsSettings);
            Controls.Add(viewChangeButtons);
            Controls.Add(newWmsButton);
            Controls.Add(canvas);
            Icon = Properties.Resources.Main;
            KeyPreview = true;
            MinimumSize = new Size(768, 576);
            Name = "ViewerWindow";
            Text = Properties.Resources.ApplicationTitle;
            FormClosing += new FormClosingEventHandler(ViewerWindowClosing);
            KeyDown += new KeyEventHandler(ViewerWindowKeyDown);
            Load += new EventHandler(ViewerWindowInit);
            ((ISupportInitialize)canvas).EndInit();
            statusBar.ResumeLayout(false);
            statusBar.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox canvas;
        private ViewChangeButtons viewChangeButtons;
        private Button newWmsButton;
        private WmsSettings wmsSettings;
        private LayerSettings layerSettings;
        private TreeView layers;
        private StatusStrip statusBar;
        private ToolStripStatusLabel coordinatesLabel;
    }
}
