using System;
using System.Windows;
using System.Windows.Media;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.Utility;

namespace Mushrooms.Dialogs {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ColorSelectionViewModel : DialogAwareBase {
        public  const string    cSelectedColor = "selected color";

        private float           mSelectorLeft;
        private float           mSelectorTop;

        public  Color           SelectedColor { get; private set; }
        public  Color           OpposingColor { get; private set; }
        public  string          Angle { get; private set; }

        public ColorSelectionViewModel() {
            Title = "Color Selection";

            SelectorTop = 0;
            SelectorLeft = 450;
            SelectedColor = Colors.White;
            OpposingColor = Colors.Black;

            Angle = String.Empty;
            UpdateSelectedColor();
        }

        public override void OnDialogOpened( IDialogParameters parameters ) {
            SelectedColor = parameters.GetValue<Color>( cSelectedColor );
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
            var angle = CalculateAngle( new Point( 500, 500 ), new Point( SelectorTop + 50, SelectorLeft + 50 ));
            var hsl = new HslColor { A = 1.0, H = angle, S = 1.0, L = 0.5 };

            SelectedColor = hsl.ToRgb();

            hsl.H = ( angle + 180 ) % 360;

            OpposingColor = hsl.ToRgb();

            RaisePropertyChanged( () => SelectedColor );
            RaisePropertyChanged( () => OpposingColor );

            UpdateAngle();
        }

        private void UpdateAngle() {
            var angle = CalculateAngle( new Point( 500, 500 ), new Point( SelectorLeft + 50, SelectorTop + 50 ));

            Angle = $"{angle:F2}";
            RaisePropertyChanged( () => Angle );
        }

        private double CalculateAngle( Point origin, Point visitor ) {
            var radian = Math.Atan2( visitor.Y - origin.Y, visitor.X - origin.X );
            var angle = ( radian * ( 180 / Math.PI ) + 360 ) % 360;

            return ( angle + 270 ) % 360;
        }

        protected override DialogParameters CreateClosingParameters() =>
            new() {
                { cSelectedColor, SelectedColor }
            };
    }
}
