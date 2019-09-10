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
    public class ViewChangeButtons : Panel
    {
        private abstract class ButtonType
        {
            public abstract Cursor CanvasCursor { get; }

            public virtual bool ClickAction(Point position)
            {
                return false;
            }

            public virtual bool ClickDownAction(Point position)
            {
                return false;
            }

            public virtual bool ClickUpAction(Point position)
            {
                return false;
            }

            public void ZoomIn(Point position)
            {
                invoker.ChangeAndStore(new ZoomInAction(viewport, position));
            }

            public void ZoomOut(Point position)
            {
                invoker.ChangeAndStore(new ZoomOutAction(viewport, position));
            }

            protected ButtonType(CentricView view, PreviousViewports invokerView)
            {
                Debug.Assert(view != null, "Viewport must be valid!");
                Debug.Assert(invokerView != null, "Invoker must be valid!");

                viewport = view;
                invoker = invokerView;
            }

            protected CentricView viewport;
            protected PreviousViewports invoker;
        }

        private class ArrowButton : ButtonType
        {
            public ArrowButton(CentricView viewport, PreviousViewports invoker) : base(viewport, invoker) { }

            public override Cursor CanvasCursor
            {
                get
                {
                    return Cursors.Arrow;
                }
            }
        }

        private class ZoomInButton : ButtonType
        {
            public ZoomInButton(CentricView viewport, PreviousViewports invoker) : base(viewport, invoker) { }

            public override Cursor CanvasCursor
            {
                get
                {
                    return Cursors.PanNorth;
                }
            }

            public override bool ClickAction(Point position)
            {
                ZoomIn(position);

                return true;
            }
        }

        private class ZoomOutButton : ButtonType
        {
            public ZoomOutButton(CentricView viewport, PreviousViewports invoker)
                : base(viewport, invoker) { }

            public override Cursor CanvasCursor
            {
                get
                {
                    return Cursors.PanSouth;
                }
            }

            public override bool ClickAction(Point position)
            {
                ZoomOut(position);

                return true;
            }
        }

        private class MoveButton : ButtonType
        {
            public MoveButton(CentricView viewport, PreviousViewports invoker) : base(viewport, invoker) { }

            public override Cursor CanvasCursor
            {
                get
                {
                    return Cursors.SizeAll;
                }
            }

            public override bool ClickDownAction(Point position)
            {
                previouPosition = position;

                return false;
            }

            public override bool ClickUpAction(Point position)
            {
                if (!previouPosition.Equals(Point.Empty))
                {
                    invoker.ChangeAndStore(new MoveAction(viewport,
                        new Point(position.X - previouPosition.X, position.Y - previouPosition.Y)));
                    previouPosition = Point.Empty;

                    return true;
                }
                else
                {
                    return false;
                }
            }

            private Point previouPosition = Point.Empty;
        }

        public ViewChangeButtons(CentricView viewport, PreviousViewports invoker, Control control)
            : base()
        {
            canvas = control;
            InitializeComponent(viewport, invoker);
            currentActionButton = arrowToggle;
        }

        public bool ClickAction(Point position)
        {
            return CurrentViewChange.ClickAction(position);
        }

        public bool ClickDownAction(Point position)
        {
            return CurrentViewChange.ClickDownAction(position);
        }

        public bool ClickUpAction(Point position)
        {
            return CurrentViewChange.ClickUpAction(position);
        }

        public void ZoomIn(Point position)
        {
            CurrentViewChange.ZoomIn(position);
        }

        public void ZoomOut(Point position)
        {
            CurrentViewChange.ZoomOut(position);
        }

        public void ChangeAction(Keys key)
        {
            switch (key)
            {
                case Keys.A:
                    ChangeAction(arrowToggle);
                    break;
                case Keys.I:
                    ChangeAction(zoomInToggle);
                    break;
                case Keys.O:
                    ChangeAction(zoomOutToggle);
                    break;
                case Keys.M:
                    ChangeAction(moveToggle);
                    break;
                default:
                    break;
            }
        }

        private void InitializeComponent(CentricView viewport, PreviousViewports invoker)
        {
            arrowToggle = new Button();
            zoomInToggle = new Button();
            zoomOutToggle = new Button();
            moveToggle = new Button();
            SuspendLayout();
            // 
            // arrowToggle
            // 
            arrowToggle.Anchor = (AnchorStyles)(AnchorStyles.Top | AnchorStyles.Right);
            arrowToggle.BackColor = SystemColors.ButtonHighlight;
            arrowToggle.BackgroundImage = Properties.Resources.Arrow;
            arrowToggle.BackgroundImageLayout = ImageLayout.Center;
            arrowToggle.Location = new Point(0, 0);
            arrowToggle.Name = "arrowToggle";
            arrowToggle.Size = new Size(36, 36);
            arrowToggle.TabIndex = 0;
            arrowToggle.Tag = new ArrowButton(viewport, invoker);
            arrowToggle.UseMnemonic = false;
            arrowToggle.UseVisualStyleBackColor = false;
            arrowToggle.Click += new EventHandler(ArrowToggleClick);
            // 
            // zoomInToggle
            // 
            zoomInToggle.Anchor = (AnchorStyles)(AnchorStyles.Top | AnchorStyles.Right);
            zoomInToggle.BackgroundImage = Properties.Resources.ZoomIn;
            zoomInToggle.BackgroundImageLayout = ImageLayout.Center;
            zoomInToggle.Location = new Point(38, 0);
            zoomInToggle.Name = "zoomInToggle";
            zoomInToggle.Size = new Size(36, 36);
            zoomInToggle.TabIndex = 1;
            zoomInToggle.Tag = new ZoomInButton(viewport, invoker);
            zoomInToggle.UseMnemonic = false;
            zoomInToggle.Click += new EventHandler(ZoomInToggleClick);
            // 
            // zoomOutToggle
            // 
            zoomOutToggle.Anchor = (AnchorStyles)(AnchorStyles.Top | AnchorStyles.Right);
            zoomOutToggle.BackgroundImage = Properties.Resources.ZoomOut;
            zoomOutToggle.BackgroundImageLayout = ImageLayout.Center;
            zoomOutToggle.Location = new Point(76, 0);
            zoomOutToggle.Name = "zoomOutToggle";
            zoomOutToggle.Size = new Size(36, 36);
            zoomOutToggle.TabIndex = 2;
            zoomOutToggle.Tag = new ZoomOutButton(viewport, invoker);
            zoomOutToggle.UseMnemonic = false;
            zoomOutToggle.Click += new EventHandler(ZoomOutToggleClick);
            // 
            // moveToggle
            // 
            moveToggle.Anchor = (AnchorStyles)(AnchorStyles.Top | AnchorStyles.Right);
            moveToggle.BackgroundImage = Properties.Resources.Drag;
            moveToggle.BackgroundImageLayout = ImageLayout.Center;
            moveToggle.Location = new Point(114, 0);
            moveToggle.Name = "dragToggle";
            moveToggle.Size = new Size(36, 36);
            moveToggle.TabIndex = 3;
            moveToggle.Tag = new MoveButton(viewport, invoker);
            moveToggle.UseMnemonic = false;
            moveToggle.Click += new EventHandler(DragToggleClick);
            // 
            // ViewChange
            // 
            Anchor = (AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Bottom)
            | AnchorStyles.Left)
            | AnchorStyles.Right);
            Controls.Add(moveToggle);
            Controls.Add(zoomOutToggle);
            Controls.Add(zoomInToggle);
            Controls.Add(arrowToggle);
            Name = "ViewChangeButtons";
            Text = string.Empty;
            Size = new Size(150, 36);
            ResumeLayout(false);
            PerformLayout();
        }

        private ButtonType CurrentViewChange
        {
            get
            {
                var buttonType = currentActionButton.Tag as ButtonType;
                Debug.Assert(buttonType != null, "Every button must have set type of view change!");

                return buttonType;
            }
        }

        private void ArrowToggleClick(object sender, EventArgs e)
        {
            Debug.Assert(sender == arrowToggle,
                "Only button \"arrowToggle\" can invoke this event handler!");

            ChangeAction(arrowToggle);
        }

        private void ZoomInToggleClick(object sender, EventArgs e)
        {
            Debug.Assert(sender == zoomInToggle,
                "Only button \"zoomInToggle\" can invoke this event handler!");

            ChangeAction(zoomInToggle);
        }

        private void ZoomOutToggleClick(object sender, EventArgs e)
        {
            Debug.Assert(sender == zoomOutToggle,
                "Only button \"zoomOutToggle\" can invoke this event handler!");

            ChangeAction(zoomOutToggle);
        }

        private void DragToggleClick(object sender, EventArgs e)
        {
            Debug.Assert(sender == moveToggle, "Only button \"dragToggle\" can invoke this event handler!");

            ChangeAction(moveToggle);
        }

        private void ChangeAction(Button button)
        {
            if (button != currentActionButton)
            {
                currentActionButton.BackColor = SystemColors.Control;
                currentActionButton = button;
                currentActionButton.BackColor = SystemColors.ButtonHighlight;
                canvas.Cursor = CurrentViewChange.CanvasCursor;
            }
        }

        private Control canvas;
        private Button currentActionButton;

        private Button arrowToggle;
        private Button zoomInToggle;
        private Button zoomOutToggle;
        private Button moveToggle;
    }
}
