using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Mushrooms.Controls {
    internal class VerticalSelectionThumb : Thumb {
        public  float   CanvasSize { get; set; }
        public  float   ThumbSize { get; set; }

        public VerticalSelectionThumb() {
            DragDelta += OnDragDelta;
        }

        private void OnDragDelta( object sender, DragDeltaEventArgs e ) {
            if( DataContext is Control control ) {
                double top = Canvas.GetTop( control );
                double height = Canvas.GetBottom( control ) - top;
                double change = Math.Max( 0, Math.Min( CanvasSize - ThumbSize, top + e.VerticalChange ));

                Canvas.SetTop( control, change );
            }
        }
    }
}
