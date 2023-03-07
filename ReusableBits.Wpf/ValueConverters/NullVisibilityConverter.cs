using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReusableBits.Wpf.ValueConverters {
    // usage:
    //   <valueConverters:NullVisibilityConverter x:Key="NullVisibilityConverter" IsHidden="True" TriggerValue="False"/>

    public class NullVisibilityConverter : IValueConverter {
        //Set to true if you want to show control when boolean value is true
        //Set to false if you want to hide/collapse control when value is true
        public bool TriggerValue { get; set; }

        //Set to true if you just want to hide the control
        //else set to false if you want to collapse the control
        public bool IsHidden { get; set; }

        public NullVisibilityConverter() {
            TriggerValue = false;
            IsHidden = true;
        }

        private object GetVisibility( object ? value ) {
            var isNull = value == null;

            if(( isNull && TriggerValue && IsHidden ) ||
               ( !isNull && !TriggerValue && IsHidden )) {
                return Visibility.Hidden;
            }

            if(( isNull && TriggerValue && !IsHidden ) ||
               ( !isNull && !TriggerValue && !IsHidden )) {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
            return GetVisibility( value );
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
            return DependencyProperty.UnsetValue;
        }
    }
}
