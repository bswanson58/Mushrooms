using System;
using System.Windows;
using System.Windows.Media;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.Utility;

namespace Mushrooms.Dialogs {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ColorSelectionViewModel : DialogAwareBase {
        public  const string    cSelectedColor = "selected color";

        // the center point of the color wheel
        private const double    cCenterPoint = 500;
        // the radius of the track to follow with the selector
        private const double    cColorWheelRadius = 425;
        // the radius of the selector
        private const double    cSelectorRadius = 75;

        private float           mSelectorLeft;
        private float           mSelectorTop;

        public  Color           SelectedColor { get; private set; }

        public ColorSelectionViewModel() {
            Title = "Color Selection";

            SelectorTop = 0;
            SelectorLeft = 425;
            SelectedColor = Colors.White;

            UpdateSelectedColor();
            UpdateSelectorPosition();
        }

        public override void OnDialogOpened( IDialogParameters parameters ) {
            SelectedColor = parameters.GetValue<Color>( cSelectedColor );

            UpdateSelectorPosition();
            RaisePropertyChanged( () => SelectedColor );
        }

        public float SelectorLeft {
            get => mSelectorLeft;
            set {
                mSelectorLeft = value;

                UpdateSelectedColor();
            }
        }

        public float SelectorTop {
            get => mSelectorTop;
            set {
                mSelectorTop = value;

                UpdateSelectedColor();
            }
        }

        private void UpdateSelectedColor() {
            var angle = CalculateAngle( new Point( cCenterPoint, cCenterPoint ), 
                                        new Point( SelectorTop + cSelectorRadius, SelectorLeft + cSelectorRadius ));
            var hsl = new HslColor { A = 1.0, H = angle, S = 1.0, L = 0.5 };

            SelectedColor = hsl.ToRgb();

            RaisePropertyChanged( () => SelectedColor );
        }

        private void UpdateSelectorPosition() {
            var hsl = new HslColor( SelectedColor );
            var selectorPoint = CalculatePoint( new Point( cCenterPoint, cCenterPoint ), cColorWheelRadius, hsl.H );

            mSelectorLeft = (float)( selectorPoint.X - cSelectorRadius );
            mSelectorTop = (float)( selectorPoint.Y - cSelectorRadius );

            RaisePropertyChanged( () => SelectorLeft );
            RaisePropertyChanged( () => SelectorTop );
        }


        private double CalculateAngle( Point origin, Point visitor ) {
            var radian = Math.Atan2( visitor.Y - origin.Y, visitor.X - origin.X );
            var angle = ( radian * ( 180 / Math.PI ) + 360 ) % 360;

            return ( angle + 270 ) % 360;
        }

        private Point CalculatePoint( Point origin, double radius, double angle ) {
            angle = ( angle + 90 ) % 360;
            // convert angle from degrees to radians
            var radians = ( Math.PI * angle ) / 180.0;
            var pointY = radius * Math.Cos( radians );
            var pointX = radius * Math.Sin( radians );

            return new Point( origin.X + pointX, origin.Y + pointY );
        }

        protected override DialogParameters CreateClosingParameters() =>
            new() {
                { cSelectedColor, SelectedColor }
            };
    }
}
