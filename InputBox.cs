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

using System.Drawing;
using System.Windows.Forms;

namespace WmsViewer
{
    public class InputBox : Form
    {
        public static DialogResult ShowInputBox(string title, string promptText, ref string outputTest)
        {
            using (var inputBox = new InputBox())
            {
                inputBox.Text = title;
                inputBox.prompt.Text = promptText;
                inputBox.input.Text = outputTest;
                var dialogResult = inputBox.ShowDialog();
                outputTest = inputBox.input.Text;
                return dialogResult;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="InputBox" /> class from being created.
        /// </summary>
        private InputBox()
            : base()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            prompt = new Label();
            input = new TextBox();
            buttonOk = new Button();
            buttonCancel = new Button();
            SuspendLayout();
            // 
            // prompt
            // 
            prompt.AutoEllipsis = true;
            prompt.Location = new Point(9, 20);
            prompt.Name = "prompt";
            prompt.Size = new Size(378, 13);
            prompt.TabIndex = 0;
            prompt.UseMnemonic = false;
            // 
            // input
            // 
            input.Location = new Point(12, 36);
            input.Name = "input";
            input.Size = new Size(372, 20);
            input.TabIndex = 0;
            // 
            // buttonOk
            // 
            buttonOk.Anchor = (AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Right);
            buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonOk.Location = new Point(228, 72);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new Size(75, 23);
            buttonOk.TabIndex = 1;
            buttonOk.Text = Properties.Resources.Ok;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = (AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Right);
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new Point(309, 72);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 2;
            buttonCancel.Text = Properties.Resources.Cancel;
            // 
            // InputBox
            // 
            AcceptButton = buttonOk;
            CancelButton = buttonCancel;
            ClientSize = new Size(396, 107);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOk);
            Controls.Add(input);
            Controls.Add(prompt);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Icon = Properties.Resources.Main;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InputBox";
            StartPosition = FormStartPosition.CenterScreen;
            ResumeLayout(false);
            PerformLayout();
        }

        private Label prompt;
        private TextBox input;
        private Button buttonOk;
        private Button buttonCancel;
    }
}
