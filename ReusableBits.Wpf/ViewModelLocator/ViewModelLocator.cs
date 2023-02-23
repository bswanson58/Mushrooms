// from: https://github.com/PrismLibrary/Prism/blob/master/src/Wpf/Prism.Wpf/Mvvm/ViewModelLocator.cs
// with a few cleanups

// usage: https://prismlibrary.com/docs/viewmodel-locator.html

using System.ComponentModel;
using System.Windows;

namespace ReusableBits.Wpf.ViewModelLocator {
    /// <summary>
    /// This class defines the attached property and related change handler that calls the ViewModelLocator.
    /// </summary>
    public static class ViewModelLocator {
        /// <summary>
        /// The AutoWireViewModel attached property.
        /// </summary>
        public static readonly DependencyProperty AutoWireViewModelProperty = DependencyProperty.RegisterAttached(
            "AutoWireViewModel", typeof( bool? ), typeof( ViewModelLocator ), new PropertyMetadata( null, AutoWireViewModelChanged ));

        /// <summary>
        /// Gets the value for the <see cref="AutoWireViewModelProperty"/> attached property.
        /// </summary>
        /// <param name="obj">The target element.</param>
        /// <returns>The <see cref="AutoWireViewModelProperty"/> attached to the <paramref name="obj"/> element.</returns>
        public static bool? GetAutoWireViewModel( DependencyObject obj ) {
            return (bool?)obj.GetValue( AutoWireViewModelProperty );
        }

        /// <summary>
        /// Sets the <see cref="AutoWireViewModelProperty"/> attached property.
        /// </summary>
        /// <param name="obj">The target element.</param>
        /// <param name="value">The value to attach.</param>
        public static void SetAutoWireViewModel( DependencyObject obj, bool? value ) {
            obj.SetValue( AutoWireViewModelProperty, value );
        }

        private static void AutoWireViewModelChanged( DependencyObject d, DependencyPropertyChangedEventArgs e ) {
#if !HAS_WINUI
            if( !DesignerProperties.GetIsInDesignMode( d ))
#endif
            {
                var value = (bool?)e.NewValue;
                if( value.Value ) {
                    ViewModelLocationProvider.AutoWireViewModelChanged( d, Bind );
                }
            }
        }

        /// <summary>
        /// Sets the DataContext of a View.
        /// </summary>
        /// <param name="view">The View to set the DataContext on.</param>
        /// <param name="viewModel">The object to use as the DataContext for the View.</param>
        static void Bind( object view, object viewModel ) {
            if( view is FrameworkElement element )
                element.DataContext = viewModel;
        }
    }
}
