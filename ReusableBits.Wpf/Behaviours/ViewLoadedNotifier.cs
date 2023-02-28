using System.Windows;
using System.Windows.Input;

namespace ReusableBits.Wpf.Behaviours {
    public class ViewLoadedNotifier : DependencyObject {
        public static readonly DependencyProperty NotifyCommandProperty =
            DependencyProperty
                .RegisterAttached( "NotifyCommand", 
                    typeof( ICommand ), 
                    typeof( ViewLoadedNotifier ), 
                    new PropertyMetadata( null, OnNotifyCommandChanged ));

        public static void SetNotifyCommand( DependencyObject d, ICommand value ) =>
            d.SetValue( NotifyCommandProperty, value );

        public static ICommand ? GetNotifyCommand( DependencyObject d ) =>
            ((ICommand ?)d.GetValue( NotifyCommandProperty ));

        private static void OnNotifyCommandChanged( DependencyObject d, DependencyPropertyChangedEventArgs e ) {
            if( d is FrameworkElement element ) {
                element.Loaded += OnElementLoaded; 
            }
        }

        private static void OnElementLoaded( object sender, RoutedEventArgs args ) {
            if( sender is FrameworkElement parent ) {
                parent.Loaded -= OnElementLoaded;

                var notifyCommand = GetNotifyCommand( parent );

                notifyCommand?.Execute( null );
            }
        }
    }
}
