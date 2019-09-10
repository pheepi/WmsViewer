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

namespace WmsViewer
{
    public interface IViewport : ICloneable
    {
        IViewport Copy();

        void ReduceViewBox(int maxWidth, int maxHeight);

        int Width { get; }      // Width of canvas

        int Height { get; }     // Height of canvas

        int XToCanvasX(double xCoord);

        int YToCanvasY(double yCoord);

        double LeftCorner { get; }

        double TopCorner { get; }

        double RightCorner { get; }

        double BottomCorner { get; }
    }

    public class CentricView : IViewport
    {
        public CentricView(int width, int height, double initialX, double initialY, double initialZoom)
        {
            Resize(width, height);
            x = initialX;
            y = initialY;
            zoom = initialZoom;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var view = obj as CentricView;
            if (view == null)
            {
                return false;
            }

            return Width.Equals(view.Width)
                && Height.Equals(view.Height)
                && x.Equals(view.x)
                && y.Equals(view.y)
                && zoom.Equals(view.zoom);
        }

        public override int GetHashCode()
        {
            return Width.GetHashCode()
                ^ Height.GetHashCode()
                ^ x.GetHashCode()
                ^ y.GetHashCode()
                ^ zoom.GetHashCode();
        }

        public object Clone()
        {
            return new CentricView(Width, Height, x, y, zoom);
        }

        public IViewport Copy()
        {
            return Clone() as CentricView;
        }

        public void ReduceViewBox(int maxWidth, int maxHeight)
        {
            if (Width > maxWidth)
            {
                Width = maxWidth;
            }

            if (Height > maxHeight)
            {
                Height = maxHeight;
            }
        }

        public int Width { get; private set; }      // Width of canvas

        public int Height { get; private set; }     // Height of canvas

        public double WidthInCoords
        {
            get
            {
                return Width / zoom;
            }
        }

        public double HeightInCoords
        {
            get
            {
                return Height / zoom;
            }
        }

        public void Resize(int width, int height)
        {
            Debug.Assert(width > 0, "Width of viewport must be greater than 0!");
            Debug.Assert(height > 0, "Height of viewport must be greater than 0!");

            Width = width;
            Height = height;
        }

        public int XToCanvasX(double xCoord)
        {
            return (Width / 2) + Convert.ToInt32((xCoord - x) * zoom);
        }

        public int YToCanvasY(double yCoord)
        {
            return (Height / 2) + Convert.ToInt32((y - yCoord) * zoom);
        }

        public double CanvasXToX(int xCoord)
        {
            return ((xCoord - (Width / 2.0)) / zoom) + x;
        }

        public double CanvasYToY(int yCoord)
        {
            return (((Height / 2.0) - yCoord) / zoom) + y;
        }

        public double LeftCorner
        {
            get
            {
                return CanvasXToX(0);
            }
        }

        public double TopCorner
        {
            get
            {
                return CanvasYToY(0);
            }
        }

        public double RightCorner
        {
            get
            {
                return CanvasXToX(Width);
            }
        }

        public double BottomCorner
        {
            get
            {
                return CanvasYToY(Height);
            }
        }

        public void ZoomIn(Point point)
        {
            Debug.Assert(point != null, "A point must be defined to zoom in!");

            zoom *= 2.0;
            x = LeftCorner + ((point.X / (double)Width) * WidthInCoords);
            y = TopCorner - ((point.Y / (double)Height) * HeightInCoords);
        }

        public void ZoomOut(Point point)
        {
            Debug.Assert(point != null, "A point must be defined to zoom out!");

            x = RightCorner - ((point.X / (double)Width) * WidthInCoords);
            y = BottomCorner + ((point.Y / (double)Height) * HeightInCoords);
            zoom /= 2.0;
        }

        public void Move(int deltaX, int deltaY)
        {
            x = CanvasXToX((Width / 2) - deltaX);
            y = CanvasYToY((Height / 2) - deltaY);
        }

        private double x = 0;       // x-coordinate (horizontal) of center of the screen
        private double y = 0;       // y-coordinate (verrtical) of center of the screen
        private double zoom = 1;        // 1 pixel = 1 coordinate unit * zoom
    }
}
