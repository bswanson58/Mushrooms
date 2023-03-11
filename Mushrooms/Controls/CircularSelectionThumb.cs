using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ReusableBits.Wpf.Utility;

// thumbs: https://www.codeproject.com/Articles/22952/WPF-Diagram-Designer-Part-1
// source: https://math.stackexchange.com/questions/127613/closest-point-on-circle-edge-from-point-outside-inside-the-circle

namespace Mushrooms.Controls {
    internal class CircularSelectionThumb : Thumb {
        public  float   EllipseSize { get; set; }
        public  float   ThumbSize { get; set; }

        public CircularSelectionThumb() {
            DragDelta += MoveThumb_DragDelta;
        }

        private void MoveThumb_DragDelta( object sender, DragDeltaEventArgs e ) {
            if( DataContext is FrameworkElement containerElement ) {
                var canvas = containerElement.FindParent<Canvas>();

                if( canvas != null ) {
                    var mousePosition = Mouse.GetPosition( canvas );
                    var ellipseCenter = new Point( EllipseSize / 2.0, EllipseSize / 2.0 );
                    var ellipsePoint = LocateClosestEllipsePoint( ellipseCenter, mousePosition );

                    Canvas.SetLeft( containerElement, ellipsePoint.X - ( ThumbSize / 2.0 ));
                    Canvas.SetTop( containerElement, ellipsePoint.Y - ( ThumbSize / 2.0 ));
                }
            }
        }

        private Point LocateClosestEllipsePoint( Point ellipseCenter, Point mousePoint ) {
            var numeratorX = mousePoint.X - ellipseCenter.X;
            var numeratorY = mousePoint.Y - ellipseCenter.Y;
            var denominator = Math.Sqrt( Math.Pow( mousePoint.X - ellipseCenter.X, 2.0 ) + 
                                         Math.Pow( mousePoint.Y - ellipseCenter.Y, 2.0 ));
            var radius = ( EllipseSize / 2.0 ) - ( ThumbSize / 2.0 );

            var circleX = ellipseCenter.X + ( radius * ( numeratorX / denominator ));
            var circleY = ellipseCenter.Y + ( radius * ( numeratorY / denominator ));

            return new ( circleX, circleY );
        }
    }
}
