using System.Windows.Input;
using Microsoft.Maps.MapControl.WPF;

namespace OsrmRoutesEditor.Helpers
{
    /// <summary>
    /// The class implements a marker that can be moved with the mouse pointer
    /// </summary>
    public class DraggablePin : Pushpin
    {
        private readonly Map _map;
        private bool _isDragging;
        Location _center;

        public DraggablePin(Map map)
        {
            _map = map;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (_map != null)
                {
                    _center = _map.Center;

                    _map.ViewChangeOnFrame += _map_ViewChangeOnFrame;
                    _map.MouseUp += ParentMap_MouseButtonUp;
                    _map.MouseMove += ParentMap_MouseMove;
                    _map.TouchMove += _map_TouchMove;
                }

               base.OnMouseDown(e);
            }
        }

        void ParentMap_MouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Left Mouse Button released, stop dragging the Pushpin
            if (_map != null)
            {
                _map.ViewChangeOnFrame -= _map_ViewChangeOnFrame;
                _map.MouseUp -= ParentMap_MouseButtonUp;
                _map.MouseMove -= ParentMap_MouseMove;
                _map.TouchMove -= _map_TouchMove;
            }

            this._isDragging = false;
        }

        #region "Event Handler Methods"

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                this._isDragging = true;

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                this._isDragging = false;

            base.OnKeyUp(e);
        }

        void _map_ViewChangeOnFrame(object sender, MapEventArgs e)
        {
            if (_isDragging)
            {
                _map.Center = _center;
            }
        }

        void ParentMap_MouseMove(object sender, MouseEventArgs e)
        {
            var map = sender as Map;

            // Check if the user is currently dragging the Pushpin
            if (this._isDragging)
            {
                // If so, the Move the Pushpin to where the Mouse is.
                var mouseMapPosition = e.GetPosition(map);
                if (map != null)
                {
                    var mouseGeocode = map.ViewportPointToLocation(mouseMapPosition);
                    this.Location = mouseGeocode;
                }
            }
        }
        void _map_TouchMove(object sender, TouchEventArgs e)
        {
            var map = sender as Microsoft.Maps.MapControl.WPF.Map;
            // Check if the user is currently dragging the Pushpin
            if (this._isDragging)
            {
                // If so, the Move the Pushpin to where the Mouse is.
                var mouseMapPosition = e.GetTouchPoint(map);
                if (map != null)
                {
                    var mouseGeocode = map.ViewportPointToLocation(mouseMapPosition.Position);
                    this.Location = mouseGeocode;
                }
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;

            base.OnMouseLeave(e);
        }

        #endregion
    }
}
