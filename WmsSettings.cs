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
    public class WmsSettings : Panel
    {
        public WmsSettings(PictureBox pictureCanvas)
            : base()
        {
            Debug.Assert(pictureCanvas != null, "Every change of WMS must be redrawn!");

            canvas = pictureCanvas;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            serviceAddressLabel = new Label();
            serviceAddress = new TextBox();
            serviceTitleLabel = new Label();
            serviceTitle = new Label();
            serviceDescriptionLabel = new Label();
            serviceDescription = new Label();
            protocolVersionLabel = new Label();
            protocolVersion = new Label();
            crsLabel = new Label();
            crs = new ComboBox();
            downloadMaps = new CheckBox();
            SuspendLayout();
            // 
            // serviceAddressLabel
            // 
            serviceAddressLabel.AutoSize = true;
            serviceAddressLabel.Location = new Point(6, 16);
            serviceAddressLabel.MinimumSize = new Size(105, 13);
            serviceAddressLabel.Name = "serviceAddressLabel";
            serviceAddressLabel.Size = new Size(105, 13);
            serviceAddressLabel.TabIndex = 0;
            serviceAddressLabel.Text = Properties.Resources.ServiceAddress;
            serviceAddressLabel.TextAlign = ContentAlignment.TopRight;
            // 
            // serviceAddress
            // 
            serviceAddress.Anchor = (AnchorStyles)((AnchorStyles.Top | AnchorStyles.Left)
                | AnchorStyles.Right);
            serviceAddress.Location = new Point(117, 13);
            serviceAddress.MinimumSize = new Size(16, 20);
            serviceAddress.Name = "serviceAddress";
            serviceAddress.ReadOnly = true;
            serviceAddress.Size = new Size(97, 20);
            serviceAddress.TabIndex = 0;
            // 
            // serviceTitleLabel
            // 
            serviceTitleLabel.AutoSize = true;
            serviceTitleLabel.Location = new Point(6, 42);
            serviceTitleLabel.MinimumSize = new Size(105, 13);
            serviceTitleLabel.Name = "serviceTitleLabel";
            serviceTitleLabel.Size = new Size(105, 13);
            serviceTitleLabel.TabIndex = 0;
            serviceTitleLabel.Text = Properties.Resources.ServiceTitle;
            serviceTitleLabel.TextAlign = ContentAlignment.TopRight;
            // 
            // serviceTitle
            // 
            serviceTitle.Anchor = (AnchorStyles)((AnchorStyles.Top | AnchorStyles.Left)
                | AnchorStyles.Right);
            serviceTitle.AutoEllipsis = true;
            serviceTitle.Location = new Point(114, 42);
            serviceTitle.MinimumSize = new Size(16, 13);
            serviceTitle.Name = "serviceTitle";
            serviceTitle.Size = new Size(103, 13);
            serviceTitle.TabIndex = 0;
            // 
            // serviceDescriptionLabel
            // 
            serviceDescriptionLabel.AutoSize = true;
            serviceDescriptionLabel.Location = new Point(6, 66);
            serviceDescriptionLabel.MinimumSize = new Size(105, 13);
            serviceDescriptionLabel.Name = "serviceDescriptionLabel";
            serviceDescriptionLabel.Size = new Size(105, 13);
            serviceDescriptionLabel.TabIndex = 0;
            serviceDescriptionLabel.Text = Properties.Resources.ServiceDescription;
            serviceDescriptionLabel.TextAlign = ContentAlignment.TopRight;
            // 
            // serviceDescription
            // 
            serviceDescription.Anchor = (AnchorStyles)((AnchorStyles.Top | AnchorStyles.Left)
                | AnchorStyles.Right);
            serviceDescription.AutoEllipsis = true;
            serviceDescription.Location = new Point(114, 66);
            serviceDescription.MinimumSize = new Size(16, 13);
            serviceDescription.Name = "serviceDescription";
            serviceDescription.Size = new Size(103, 13);
            serviceDescription.TabIndex = 0;
            // 
            // protocolVersionLabel
            // 
            protocolVersionLabel.AutoSize = true;
            protocolVersionLabel.Location = new Point(6, 90);
            protocolVersionLabel.MinimumSize = new Size(105, 13);
            protocolVersionLabel.Name = "protocolVersionLabel";
            protocolVersionLabel.Size = new Size(105, 13);
            protocolVersionLabel.TabIndex = 0;
            protocolVersionLabel.Text = Properties.Resources.ProtocolVersion;
            protocolVersionLabel.TextAlign = ContentAlignment.TopRight;
            // 
            // protocolVersion
            // 
            protocolVersion.Anchor = (AnchorStyles)((AnchorStyles.Top | AnchorStyles.Left)
                | AnchorStyles.Right);
            protocolVersion.AutoSize = true;
            protocolVersion.Location = new Point(114, 90);
            protocolVersion.MinimumSize = new Size(0, 13);
            protocolVersion.Name = "protocolVersion";
            protocolVersion.Size = new Size(0, 13);
            protocolVersion.TabIndex = 0;
            // 
            // crsLabel
            // 
            crsLabel.AutoSize = true;
            crsLabel.Location = new Point(6, 114);
            crsLabel.MinimumSize = new Size(105, 13);
            crsLabel.Name = "crsLabel";
            crsLabel.Size = new Size(105, 13);
            crsLabel.TabIndex = 0;
            crsLabel.Text = Properties.Resources.Crs;
            crsLabel.TextAlign = ContentAlignment.TopRight;
            // 
            // crs
            // 
            crs.DropDownStyle = ComboBoxStyle.DropDownList;
            crs.FormattingEnabled = true;
            crs.Location = new Point(117, 111);
            crs.MinimumSize = new Size(94, 0);
            crs.Name = "crs";
            crs.Size = new Size(97, 21);
            crs.TabIndex = 1;
            crs.SelectionChangeCommitted += new EventHandler(CrsChanged);
            // 
            // downloadMaps
            // 
            downloadMaps.AutoSize = true;
            downloadMaps.Checked = true;
            downloadMaps.CheckState = CheckState.Checked;
            downloadMaps.Location = new Point(14, 138);
            downloadMaps.Name = "downloadMaps";
            downloadMaps.Size = new Size(163, 17);
            downloadMaps.TabIndex = 2;
            downloadMaps.Text = Properties.Resources.DownloadMaps;
            downloadMaps.CheckedChanged += new EventHandler(DownloadMapsChecked);
            // 
            // WmsSettings
            // 
            Anchor = (AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom)
                | AnchorStyles.Left) | AnchorStyles.Right);
            Controls.Add(serviceAddressLabel);
            Controls.Add(serviceAddress);
            Controls.Add(serviceTitleLabel);
            Controls.Add(serviceTitle);
            Controls.Add(serviceDescriptionLabel);
            Controls.Add(serviceDescription);
            Controls.Add(protocolVersionLabel);
            Controls.Add(protocolVersion);
            Controls.Add(crsLabel);
            Controls.Add(crs);
            Controls.Add(downloadMaps);
            MinimumSize = new Size(228, 164);
            Name = "wmsSettings";
            Size = new Size(228, 164);
            ResumeLayout(false);
            PerformLayout();
        }

        public void Refresh(Wms newWms)
        {
            wms = newWms;
            if (newWms != null)
            {
                serviceAddress.Text = newWms.ServiceAddress.ToString();
                serviceTitle.Text = newWms.Title;
                serviceDescription.Text = newWms.Description;
                protocolVersion.Text = newWms.Version;
                FillCrsComboBox();
                downloadMaps.Checked = newWms.IsDownloadingEnabled;
                Enabled = true;
            }
            else
            {
                Enabled = false;
                serviceAddress.Text = string.Empty;
                serviceTitle.Text = string.Empty;
                serviceDescription.Text = string.Empty;
                protocolVersion.Text = string.Empty;
                crs.Items.Clear();
                downloadMaps.Checked = true;
            }
        }

        private void FillCrsComboBox()
        {
            Debug.Assert(wms != null, "CRS combobox cannot be filled without WMS!");

            crs.Items.Clear();
            crs.SelectedIndex = -1;
            for (var i = 0; i < wms.CRSCount; ++i)
            {
                crs.Items.Add(wms.GetCRSName(i));
                if (i == wms.CRSId)
                {
                    crs.SelectedIndex = i;
                }
            }
        }

        private void CrsChanged(object sender, EventArgs e)
        {
            Debug.Assert(wms != null, "CRS ID cannot be changed without WMS!");
            Debug.Assert(crs.SelectedIndex != -1, "No CRS is selected!");

            wms.CRSId = crs.SelectedIndex;
            canvas.Invalidate(false);
        }

        private void DownloadMapsChecked(object sender, EventArgs e)
        {
            Debug.Assert(wms != null, "Maps downloading property cannot be changed without WMS!");

            wms.IsDownloadingEnabled = downloadMaps.Checked;
            canvas.Invalidate(false);
        }

        private Wms wms = null;
        private PictureBox canvas;

        private Label serviceAddressLabel;
        private TextBox serviceAddress;
        private Label serviceTitleLabel;
        private Label serviceTitle;
        private Label serviceDescriptionLabel;
        private Label serviceDescription;
        private Label protocolVersionLabel;
        private Label protocolVersion;
        private Label crsLabel;
        private ComboBox crs;
        private CheckBox downloadMaps;
    }
}
