using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Mushrooms.Entities;
using Mushrooms.Models;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.Platform;
using ReusableBits.Wpf.Utility;

namespace Mushrooms.Dialogs {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ColorSelectionViewModel : DialogAwareBase {
        public  const string    cColorPalette = "color palette";
        public  const string    cUpdateCallback = "update callback";

        private Action<ScenePalette>    mUpdateCallback;

        // the center point of the color wheel
        private const double    cCenterPoint = 500;
        // the radius of the track to follow with the selector
        private const double    cColorWheelRadius = 425;
        // the radius of the selector
        private const double    cSelectorRadius = 75;

        private ScenePalette    mPalette;
        private float           mSelectorLeft;
        private float           mSelectorTop;
        private float           mSaturationLevel;
        private float           mLuminosityLevel;

        public  Color           HueColor { get; private set; }
        public  Color           SaturationColor { get; private set; }
        public  Color           LuminosityColor {  get; private set; }
        public  Color           FinalColor {  get; private set; }

        public  ICommand        BalancedLuminosity { get; }
        public  ICommand        FullSaturation { get; }

        public  ObservableCollection<ColorViewModel>  Swatches { get; }

        public ColorSelectionViewModel() {
            Title = "Color Selection";

            mPalette = ScenePalette.Default;
            Swatches = new ObservableCollection<ColorViewModel>();

            BalancedLuminosity = new DelegateCommand( OnBalancedLuminosity );
            FullSaturation = new DelegateCommand( OnFullSaturation );

            mSelectorTop = 0;
            mSelectorLeft = 425;
            mSaturationLevel = 100;

            mUpdateCallback = _ => { };
        }

        public override void OnDialogOpened( IDialogParameters parameters ) {
            mPalette = parameters.GetValue<ScenePalette>( cColorPalette ) ?? ScenePalette.Default;
            var callback = parameters.GetValue<Action<ScenePalette>>( cUpdateCallback );

            if( callback != null ) {
                mUpdateCallback = callback;
            }

            Swatches.Clear();

            if( mPalette.Palette.Any()) {
                InitializeColor( mPalette.Palette.First());

                Swatches.AddRange( mPalette.Palette.Select( c => new ColorViewModel( c, OnSwatchSelected, _ => { })));
                Swatches.First().IsSelected = true;
            }
        }

        public LinearGradientBrush SaturationBrush {
            get {
                var startColor = new HslColor( HueColor ) { S = 0.0 };
                var endColor = new HslColor( HueColor ) { S = 1.0 };

                var brush = new LinearGradientBrush( endColor.ToRgb(), startColor.ToRgb(), 90 );
                brush.GradientStops.Add( new GradientStop( endColor.ToRgb(), 0.14 ));
                brush.GradientStops.Add( new GradientStop( startColor.ToRgb(), 0.86 ));

                return brush;
            }
        }
         
        public LinearGradientBrush LuminosityBrush {
            get {
                var startColor = new HslColor( HueColor ) { L = 0.0, S = 1.0 };
                var midColor = new HslColor( HueColor ) { L = 0.5, S = 1.0 };
                var endColor = new HslColor( HueColor ) { L = 1.0, S = 1.0 };

                var brush = new LinearGradientBrush( endColor.ToRgb(), startColor.ToRgb(), 90 );
                brush.GradientStops.Add( new GradientStop( endColor.ToRgb(), 0.14 ));
                brush.GradientStops.Add( new GradientStop( midColor.ToRgb(), 0.5 ));
                brush.GradientStops.Add( new GradientStop( startColor.ToRgb(), 0.86 ));

                return brush;
            }
        }

        private void OnSwatchSelected( ColorViewModel color ) {
            InitializeColor( mPalette.Palette.First());
        }

        public float SelectorLeft {
            get => mSelectorLeft;
            set {
                mSelectorLeft = value;

                UpdateHueColor();
                UpdateFinalColor();
            }
        }

        public float SelectorTop {
            get => mSelectorTop;
            set {
                mSelectorTop = value;

                UpdateHueColor();
                UpdateFinalColor();
            }
        }

        private void OnFullSaturation() {
            SaturationLevel = 0;

            RaisePropertyChanged( () => SaturationLevel );
        }

        public float SaturationLevel {
            get => mSaturationLevel;
            set {
                mSaturationLevel = Math.Max( 1, Math.Min( 100, value ));

                UpdateSaturationColor();
                UpdateFinalColor();
            }
        }

        private void OnBalancedLuminosity() {
            LuminosityLevel = 50;

            RaisePropertyChanged( () => LuminosityLevel );
        }

        public float LuminosityLevel {
            get => mLuminosityLevel;
            set {
                mLuminosityLevel = Math.Max( 1, Math.Min( 100, value ));

                UpdateLuminosityColor();
                UpdateFinalColor();
            }
        }

        private void InitializeColor( Color color ) {
            var hsl = new HslColor( color ) { A = 1.0, S = 1.0, L = 0.5 };

            HueColor = hsl.ToRgb();

            UpdateSelectorPosition();
            UpdateSaturationLevel( color );
            UpdateLuminosityLevel( color );
            UpdateFinalColor();

            RaisePropertyChanged( () => HueColor );
            RaisePropertyChanged( () => SaturationBrush );
            RaisePropertyChanged( () => LuminosityBrush );
        }

        private void UpdateHueColor() {
            var angle = CalculateAngle( new Point( cCenterPoint, cCenterPoint ), 
                                        new Point( SelectorTop + cSelectorRadius, SelectorLeft + cSelectorRadius ));
            var hsl = new HslColor { A = 1.0, H = angle, S = 1.0, L = 0.5 };

            HueColor = hsl.ToRgb();

            UpdateSwatchSelection();
            UpdateSaturationColor();
            UpdateLuminosityColor();
            UpdateFinalColor();
            RaisePropertyChanged( () => HueColor );
            RaisePropertyChanged( () => SaturationBrush );
            RaisePropertyChanged( () => LuminosityBrush );
        }

        private void UpdateSwatchSelection() {
            foreach( var swatch in Swatches ) {
                swatch.SetSelection( swatch.SwatchColor.Equals( HueColor ));
            }
        }

        private void UpdateSaturationColor() {
            var hsl = new HslColor( HueColor ) { S = ( 100 - SaturationLevel ) / 100.0 };

            SaturationColor = hsl.ToRgb();

            RaisePropertyChanged( () => SaturationColor );
        }

        private void UpdateLuminosityColor() {
            var hsl = new HslColor( HueColor ) { L = ( 100 - LuminosityLevel ) / 100.0 };

            LuminosityColor = hsl.ToRgb();

            RaisePropertyChanged( () => LuminosityColor );
        }

        private void UpdateSelectorPosition() {
            var hsl = new HslColor( HueColor );
            var selectorPoint = CalculatePoint( new Point( cCenterPoint, cCenterPoint ), cColorWheelRadius, hsl.H );

            mSelectorLeft = (float)( selectorPoint.X - cSelectorRadius );
            mSelectorTop = (float)( selectorPoint.Y - cSelectorRadius );

            RaisePropertyChanged( () => SelectorLeft );
            RaisePropertyChanged( () => SelectorTop );
        }

        private void UpdateSaturationLevel( Color color ) {
            var hsl = new HslColor( color );

            SaturationLevel = (int)( 100 - ( hsl.S * 100 ));

            RaisePropertyChanged( () => SaturationLevel );
            UpdateSaturationColor();
        }

        private void UpdateLuminosityLevel( Color color ) {
            var hsl = new HslColor( color );

            LuminosityLevel = (int)( 100 - ( hsl.L * 100 ));

            RaisePropertyChanged( () => LuminosityLevel );
            UpdateLuminosityColor();
        }

        private void UpdateFinalColor() {
            var hueColor = new HslColor( HueColor );
            var hsl = new HslColor() {
                A = 1.0,
                H = hueColor.H,
                S = ( 100 - SaturationLevel ) / 100.0,
                L = ( 100 - LuminosityLevel ) / 100.0
            };

            FinalColor = hsl.ToRgb();

            RaisePropertyChanged( () => FinalColor );
            UpdateCallback();
        }

        private void UpdateCallback() {
            var palette = mPalette.Copy();

            palette.InsertPaletteColor( FinalColor );

            mUpdateCallback.Invoke( palette );
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

        protected override DialogParameters CreateClosingParameters() {
            mPalette.InsertPaletteColor( FinalColor );

            return new () { { cColorPalette, mPalette } };
        }
    }
}
