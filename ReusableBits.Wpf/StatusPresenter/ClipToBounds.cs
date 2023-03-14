using System.Windows;
using System.Windows.Media;

namespace ReusableBits.Wpf.StatusPresenter {
	public class Clip {
		public static bool GetToBounds( DependencyObject depObj ) {
			return (bool)depObj.GetValue( ToBoundsProperty );
		}

		public static void SetToBounds( DependencyObject depObj, bool clipToBounds ) {
			depObj.SetValue( ToBoundsProperty, clipToBounds );
		}

		/// Identifies the ToBounds Dependency Property.
		public static readonly DependencyProperty ToBoundsProperty =
			DependencyProperty.RegisterAttached( "ToBounds", typeof( bool ), typeof( Clip ), new PropertyMetadata( false, OnToBoundsPropertyChanged ));

		private static void OnToBoundsPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e ) {
			var fe = d as FrameworkElement;

			if( fe != null ) {
				ClipToBounds( fe );

				// whenever the element which this property is attached to is loaded
				// or re-sizes, we need to update its clipping geometry
				fe.Loaded += OnLoaded;
				fe.SizeChanged += OnSizeChanged;
			}
		}

		// Creates a rectangular clipping geometry which matches the geometry of the passed element
		private static void ClipToBounds( FrameworkElement fe ) {
			if( GetToBounds( fe )) {
				fe.Clip = new RectangleGeometry {
					Rect = new Rect( 0, 0, fe.ActualWidth, fe.ActualHeight )
				};
			}
			else {
				fe.Clip = null;
			}
		}

		private static void OnSizeChanged( object sender, SizeChangedEventArgs e ) {
			if( sender is FrameworkElement element ) {
                ClipToBounds( element );
            }
		}

		private static void OnLoaded( object sender, RoutedEventArgs e ) {
            if( sender is FrameworkElement fe ) {
				ClipToBounds( fe );

				fe.Unloaded += OnUnloaded;
				fe.Loaded -= OnLoaded;
			}
		}

		private static void OnUnloaded( object sender, RoutedEventArgs args ) {
            if( sender is FrameworkElement fe ) {
				fe.SizeChanged -= OnSizeChanged;
			}
		}
	}
}
