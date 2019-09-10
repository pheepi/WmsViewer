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
using System.Windows.Forms;

namespace WmsViewer
{
    public class LayerSettings : Panel
    {
        public LayerSettings(PictureBox pictureCanvas)
            : base()
        {
            Debug.Assert(pictureCanvas != null, "Every change of layer must be redrawn!");

            canvas = pictureCanvas;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            supportedCrsLabel = new Label();
            supportedCrs = new ComboBox();
            layerTitleLabel = new Label();
            layerTitle = new Label();
            layerDescriptionLabel = new Label();
            layerDescription = new Label();
            downloadOneImage = new CheckBox();
            autoUpdate = new CheckBox();
            imageFormatsLabel = new Label();
            imageFormats = new ComboBox();
            transparent = new CheckBox();
            transparentColorLabel = new Label();
            transparentColor = new TextBox();
            imageAddressLabel = new Label();
            imageAddress = new TextBox();
            SuspendLayout();
            // 
            // supportedCrsLabel
            // 
            supportedCrsLabel.AutoSize = true;
            supportedCrsLabel.Location = new Point(6, 11);
            supportedCrsLabel.MinimumSize = new Size(105, 13);
            supportedCrsLabel.Name = "supportedCrsLabel";
            supportedCrsLabel.Size = new Size(105, 13);
            supportedCrsLabel.TabIndex = 0;
            supportedCrsLabel.Text = Properties.Resources.SupportedCrs;
            supportedCrsLabel.TextAlign = ContentAlignment.TopRight;
            // 
            // supportedCrs
            // 
            supportedCrs.DropDownStyle = ComboBoxStyle.DropDownList;
            supportedCrs.FormattingEnabled = true;
            supportedCrs.Location = new Point(117, 8);
            supportedCrs.MinimumSize = new Size(97, 21);
            supportedCrs.Name = "supportedCrs";
            supportedCrs.Size = new Size(97, 21);
            supportedCrs.TabIndex = 0;
            // 
            // layerTitleLabel
            // 
            layerTitleLabel.AutoSize = true;
            layerTitleLabel.Location = new Point(6, 37);
            layerTitleLabel.MinimumSize = new Size(105, 13);
            layerTitleLabel.Name = "layerTitleLabel";
            layerTitleLabel.Size = new Size(105, 13);
            layerTitleLabel.TabIndex = 0;
            layerTitleLabel.Text = Properties.Resources.LayerTitle;
            layerTitleLabel.TextAlign = ContentAlignment.TopRight;
            // 
            // layerTitle
            // 
            layerTitle.Anchor = (AnchorStyles)((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right);
            layerTitle.AutoEllipsis = true;
            layerTitle.Location = new Point(114, 37);
            layerTitle.MinimumSize = new Size(16, 13);
            layerTitle.Name = "layerTitle";
            layerTitle.Size = new Size(103, 13);
            layerTitle.TabIndex = 0;
            // 
            // layerDescriptionLabel
            // 
            layerDescriptionLabel.AutoSize = true;
            layerDescriptionLabel.Location = new Point(6, 61);
            layerDescriptionLabel.MinimumSize = new Size(105, 13);
            layerDescriptionLabel.Name = "layerDescriptionLabel";
            layerDescriptionLabel.Size = new Size(105, 13);
            layerDescriptionLabel.TabIndex = 0;
            layerDescriptionLabel.Text = Properties.Resources.LayerDescription;
            layerDescriptionLabel.TextAlign = ContentAlignment.TopRight;
            // 
            // layerDescription
            // 
            layerDescription.Anchor = (AnchorStyles)((AnchorStyles.Top | AnchorStyles.Left)
                | AnchorStyles.Right);
            layerDescription.AutoEllipsis = true;
            layerDescription.Location = new Point(114, 61);
            layerDescription.MinimumSize = new Size(16, 13);
            layerDescription.Name = "layerDescription";
            layerDescription.Size = new Size(103, 13);
            layerDescription.TabIndex = 0;
            // 
            // downloadOneImage
            // 
            downloadOneImage.AutoSize = true;
            downloadOneImage.Checked = true;
            downloadOneImage.CheckState = CheckState.Checked;
            downloadOneImage.Location = new Point(14, 84);
            downloadOneImage.MinimumSize = new Size(187, 17);
            downloadOneImage.Name = "downloadOneImage";
            downloadOneImage.Size = new Size(187, 17);
            downloadOneImage.TabIndex = 1;
            downloadOneImage.Text = Properties.Resources.DownloadOneImage;
            downloadOneImage.CheckedChanged += new EventHandler(DownloadOneImageChecked);
            // 
            // autoUpdate
            // 
            autoUpdate.AutoSize = true;
            autoUpdate.Checked = true;
            autoUpdate.CheckState = CheckState.Checked;
            autoUpdate.Location = new Point(14, 107);
            autoUpdate.MinimumSize = new Size(145, 17);
            autoUpdate.Name = "autoUpdate";
            autoUpdate.Size = new Size(145, 17);
            autoUpdate.TabIndex = 2;
            autoUpdate.Text = Properties.Resources.AutoUpdate;
            autoUpdate.CheckedChanged += new EventHandler(AutoUpdateChecked);
            // 
            // imageFormatsLabel
            // 
            imageFormatsLabel.AutoSize = true;
            imageFormatsLabel.Location = new Point(6, 133);
            imageFormatsLabel.MinimumSize = new Size(105, 13);
            imageFormatsLabel.Name = "imageFormatsLabel";
            imageFormatsLabel.Size = new Size(105, 13);
            imageFormatsLabel.TabIndex = 0;
            imageFormatsLabel.Text = Properties.Resources.ImageFormat;
            imageFormatsLabel.TextAlign = ContentAlignment.TopRight;
            // 
            // imageFormats
            // 
            imageFormats.DropDownStyle = ComboBoxStyle.DropDownList;
            imageFormats.FormattingEnabled = true;
            imageFormats.Location = new Point(117, 130);
            imageFormats.MinimumSize = new Size(97, 21);
            imageFormats.Name = "imageFormats";
            imageFormats.Size = new Size(97, 21);
            imageFormats.TabIndex = 3;
            imageFormats.SelectionChangeCommitted += new EventHandler(ImageFormatChanged);
            // 
            // transparent
            // 
            transparent.AutoSize = true;
            transparent.Checked = true;
            transparent.CheckState = CheckState.Checked;
            transparent.Location = new Point(14, 157);
            transparent.MinimumSize = new Size(83, 17);
            transparent.Name = "transparent";
            transparent.Size = new Size(83, 17);
            transparent.TabIndex = 4;
            transparent.Text = Properties.Resources.Transparent;
            transparent.CheckedChanged += new EventHandler(TransparentChecked);
            // 
            // transparentColorLabel
            // 
            transparentColorLabel.AutoSize = true;
            transparentColorLabel.Location = new Point(6, 185);
            transparentColorLabel.MinimumSize = new Size(105, 13);
            transparentColorLabel.Name = "transparentColorLabel";
            transparentColorLabel.Size = new Size(105, 13);
            transparentColorLabel.TabIndex = 0;
            transparentColorLabel.Text = Properties.Resources.TransparentColor;
            transparentColorLabel.TextAlign = ContentAlignment.TopRight;
            // 
            // transparentColor
            // 
            transparentColor.Location = new Point(117, 182);
            transparentColor.MinimumSize = new Size(60, 20);
            transparentColor.Name = "transparentColor";
            transparentColor.ReadOnly = true;
            transparentColor.Size = new Size(60, 20);
            transparentColor.TabIndex = 5;
            transparentColor.Click += new EventHandler(ColorClicked);
            // 
            // imageAddressLabel
            // 
            imageAddressLabel.AutoSize = true;
            imageAddressLabel.Location = new Point(6, 211);
            imageAddressLabel.MinimumSize = new Size(105, 13);
            imageAddressLabel.Name = "imageAddressLabel";
            imageAddressLabel.Size = new Size(105, 13);
            imageAddressLabel.TabIndex = 0;
            imageAddressLabel.Text = Properties.Resources.ImageAddress;
            imageAddressLabel.TextAlign = ContentAlignment.TopRight;
            // 
            // imageAddress
            // 
            imageAddress.Anchor = (AnchorStyles)((AnchorStyles.Top | AnchorStyles.Left)
                | AnchorStyles.Right);
            imageAddress.Location = new Point(117, 208);
            imageAddress.MinimumSize = new Size(16, 20);
            imageAddress.Name = "imageAddress";
            imageAddress.ReadOnly = true;
            imageAddress.Size = new Size(97, 20);
            imageAddress.TabIndex = 6;
            // 
            // LayerSettings
            // 
            Anchor = (AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom)
                | AnchorStyles.Left) | AnchorStyles.Right);
            Controls.Add(supportedCrsLabel);
            Controls.Add(supportedCrs);
            Controls.Add(layerTitleLabel);
            Controls.Add(layerTitle);
            Controls.Add(layerDescriptionLabel);
            Controls.Add(layerDescription);
            Controls.Add(downloadOneImage);
            Controls.Add(autoUpdate);
            Controls.Add(imageFormatsLabel);
            Controls.Add(imageFormats);
            Controls.Add(transparent);
            Controls.Add(transparentColorLabel);
            Controls.Add(transparentColor);
            Controls.Add(imageAddressLabel);
            Controls.Add(imageAddress);
            MinimumSize = new Size(228, 236);
            Name = "layerSettings";
            Size = new Size(228, 236);
            ResumeLayout(false);
            PerformLayout();
        }

        public void Refresh(Layer newLayer)
        {
            layer = newLayer;
            if (newLayer != null)
            {
                FillCrsComboBox();
                layerTitle.Text = newLayer.Title;
                layerDescription.Text = newLayer.Description;
                downloadOneImage.Checked = newLayer.DownloadOneImage;
                autoUpdate.Checked = newLayer.AutoUpdate;
                FillImageFormatsComboBox();
                transparent.Checked = newLayer.IsTransparent && Wms.AbleToBeTransparent(newLayer.ImageFormat);
                transparentColor.Text = ColorTranslator.ToHtml(newLayer.BgColor);
                transparentColor.ForeColor = GetContrastColor(newLayer.BgColor);
                transparentColor.BackColor = newLayer.BgColor;

                Enabled = true;
                Enable();
            }
            else
            {
                Enabled = false;
                supportedCrs.Items.Clear();
                layerTitle.Text = string.Empty;
                layerDescription.Text = string.Empty;
                downloadOneImage.Checked = true;
                autoUpdate.Checked = true;
                imageFormats.Items.Clear();
                transparent.Checked = true;
                transparentColor.Text = string.Empty;
                transparentColor.ForeColor = SystemColors.WindowText;
                transparentColor.BackColor = SystemColors.Control;
            }

            RefreshImageAddress();
        }

        public void RefreshImageAddress()
        {
            if (layer != null)
            {
                var uri = layer.ImageAddress;
                if (uri != null)
                {
                    imageAddress.Text = uri.ToString();
                }
                else
                {
                    imageAddress.Text = string.Empty;
                }
            }
            else
            {
                imageAddress.Text = string.Empty;
            }
        }

        private void Enable()
        {
            Debug.Assert(layer != null, "We cannot decide whether the form is enabled without layer!");

            var enabled = layer.IsRequestedIndividually() && supportedCrs.SelectedIndex != -1;
            supportedCrs.Enabled = enabled;
            downloadOneImage.Enabled = enabled && !layer.IsLeaf();
            autoUpdate.Enabled = enabled;
            imageFormats.Enabled = enabled;
            Debug.Assert(!enabled || imageFormats.SelectedIndex != -1,
                "At least one image format must be selected!");
            transparent.Enabled = enabled && Wms.AbleToBeTransparent(layer.ImageFormat);
            transparentColor.Enabled = enabled;
            imageAddress.Enabled = enabled;
        }

        private void FillCrsComboBox()
        {
            supportedCrs.Items.Clear();
            supportedCrs.SelectedIndex = -1;
            for (var currentLayer = layer; currentLayer != null; currentLayer = currentLayer.Parent)
            {
                foreach (var id in currentLayer.CrsIds)
                {
                    supportedCrs.Items.Add(currentLayer.Wms.GetCRSName(id));
                    if (id == currentLayer.Wms.CRSId)
                    {
                        supportedCrs.SelectedIndex = supportedCrs.Items.Count - 1;
                    }
                }
            }
        }

        private void FillImageFormatsComboBox()
        {
            imageFormats.Items.Clear();
            imageFormats.SelectedIndex = -1;
            foreach (var format in layer.Wms.ImageFormats)
            {
                imageFormats.Items.Add(layer.Wms.GetImageFormat(format));
                if (format == layer.ImageFormat)
                {
                    imageFormats.SelectedIndex = imageFormats.Items.Count - 1;
                }
            }
        }

        private static Color GetContrastColor(Color color)
        {
            return (Math.Abs(RGBToLuminance(color) - RGBToLuminance(Color.White))
                <= Math.Abs(RGBToLuminance(color) - RGBToLuminance(Color.Black)))
                ? Color.Black : Color.White;
        }

        private static byte RGBToLuminance(Color color)
        {
            // Based on the formula for converting RGB color space to YUV, YIQ, YCbCr, YPbPr or YDbDr,
            // where Y corresponds to luma.
            // The formula is based on standard BT.709 and luma is derived from luminosity function.
            // http://en.wikipedia.org/wiki/Luminosity_function
            // http://en.wikipedia.org/wiki/Rec._709
            // http://poynton.com/notes/video/Constant_luminance.html
            return (byte)(((2126 * color.R) + (7152 * color.G) + (722 * color.B)) / 10000);
        }

        private void DownloadOneImageChecked(object sender, EventArgs e)
        {
            Debug.Assert(layer != null, "One image downloading property cannot be changed without WMS!");

            layer.DownloadOneImage = downloadOneImage.Checked;
            canvas.Invalidate(false);
        }

        private void AutoUpdateChecked(object sender, EventArgs e)
        {
            Debug.Assert(layer != null, "Auto-update property cannot be changed without WMS!");

            layer.AutoUpdate = autoUpdate.Checked;
            canvas.Invalidate(false);
        }

        private void ImageFormatChanged(object sender, EventArgs e)
        {
            Debug.Assert(layer != null, "Image format cannot be changed without WMS!");
            Debug.Assert(imageFormats.SelectedIndex != -1, "No image format is selected!");

            layer.ImageFormat = layer.Wms.ImageFormats[imageFormats.SelectedIndex];
            Enable();
            canvas.Invalidate(false);
        }

        private void TransparentChecked(object sender, EventArgs e)
        {
            Debug.Assert(layer != null, "Transparency cannot be changed without WMS!");

            layer.IsTransparent = transparent.Checked;
            canvas.Invalidate(false);
        }

        private void ColorClicked(object sender, EventArgs e)
        {
            using (var dialog = new ColorDialog())
            {
                dialog.AllowFullOpen = true;
                dialog.AnyColor = true;
                dialog.FullOpen = true;
                if (dialog.ShowDialog() != DialogResult.Cancel)
                {
                    transparentColor.Text = ColorTranslator.ToHtml(dialog.Color);
                    transparentColor.ForeColor = GetContrastColor(dialog.Color);
                    transparentColor.BackColor = dialog.Color;
                    layer.BgColor = dialog.Color;
                    canvas.Invalidate(false);
                }
            }
        }

        private Layer layer = null;
        private PictureBox canvas;

        private Label supportedCrsLabel;
        private ComboBox supportedCrs;
        private Label layerTitleLabel;
        private Label layerTitle;
        private Label layerDescriptionLabel;
        private Label layerDescription;
        private CheckBox downloadOneImage;
        private CheckBox autoUpdate;
        private Label imageFormatsLabel;
        private ComboBox imageFormats;
        private CheckBox transparent;
        private Label transparentColorLabel;
        private TextBox transparentColor;
        private Label imageAddressLabel;
        private TextBox imageAddress;
    }
}
