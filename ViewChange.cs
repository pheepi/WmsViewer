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

using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace WmsViewer
{
    public abstract class ViewChange
    {
        public abstract void Change();

        public abstract void Undo();

        protected CentricView Viewport { get; private set; }

        protected ViewChange(CentricView view)
        {
            Debug.Assert(view != null, "We need viewport to change it!");

            Viewport = view;
        }
    }

    public class ZoomInAction : ViewChange
    {
        public ZoomInAction(CentricView view, Point point)
            : base(view)
        {
            Debug.Assert(point != null, "Location is needed to zoom in!");

            location = point;
        }

        public override void Change()
        {
            Viewport.ZoomIn(location);
        }

        public override void Undo()
        {
            Viewport.ZoomOut(location);
        }

        private Point location;
    }

    public class ZoomOutAction : ViewChange
    {
        public ZoomOutAction(CentricView view, Point point)
            : base(view)
        {
            Debug.Assert(point != null, "Location is needed to zoom out!");

            location = point;
        }

        public override void Change()
        {
            Viewport.ZoomOut(location);
        }

        public override void Undo()
        {
            Viewport.ZoomIn(location);
        }

        private Point location;
    }

    public class MoveAction : ViewChange
    {
        public MoveAction(CentricView view, Point point)
            : base(view)
        {
            Debug.Assert(point != null, "Difference of points is needed to moving!");

            difference = point;
        }

        public override void Change()
        {
            Viewport.Move(difference.X, difference.Y);
        }

        public override void Undo()
        {
            Viewport.Move(-difference.X, -difference.Y);
        }

        private Point difference;
    }

    public class PreviousViewports
    {
        public void ChangeAndStore(ViewChange action)
        {
            Debug.Assert(action != null, "Action must be valid!");

            action.Change();
            if (current < actions.Count)
            {
                actions.RemoveRange(current, actions.Count - current);
            }

            actions.Add(action);
            ++current;
        }

        public bool Undo()
        {
            if (current > 0)
            {
                --current;
                var action = actions[current];
                action.Undo();
                return true;
            }

            return false;
        }

        public bool Redo()
        {
            if (current < actions.Count)
            {
                var action = actions[current];
                ++current;
                action.Change();
                return true;
            }

            return false;
        }

        public void Reset()
        {
            actions.Clear();
            current = 0;
        }

        private List<ViewChange> actions = new List<ViewChange>();

        /// <summary>
        /// Free position for new action.
        /// </summary>
        private int current = 0;
    }
}
