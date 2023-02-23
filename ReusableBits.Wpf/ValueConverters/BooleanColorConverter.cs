using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ReusableBits.Wpf.ValueConverters {
    public class BooleanColorConverter : IValueConverter {
        public  Color   TrueColor { get; set; } = Colors.White;
        public  Color   FalseColor {  get; set; } = Colors.Transparent;

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
            if( value is bool showTrueColor ) {
                return showTrueColor ? TrueColor : FalseColor;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
            throw new NotImplementedException();
        }
    }
}
