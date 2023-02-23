using System;
using System.ComponentModel;
using System.Reflection;

// Adapted from the Prism library:
// source: https://github.com/PrismLibrary/Prism

namespace ReusableBits.Wpf.Commands {
    /// <summary>
    /// Represents each node of nested properties expression and takes care of 
    /// subscribing/unsubscribing INotifyPropertyChanged.PropertyChanged listeners on it.
    /// </summary>
    internal class PropertyObserverNode {
        private readonly Action             mAction;
        private INotifyPropertyChanged ?    mInpcObject;

        public PropertyInfo                 PropertyInfo { get; }
        public PropertyObserverNode ?       Next { get; set; }

        public PropertyObserverNode( PropertyInfo propertyInfo, Action action ) {
            PropertyInfo = propertyInfo ?? throw new ArgumentNullException( nameof( propertyInfo ) );
            mAction = () => {
                action.Invoke();
                if( Next == null ) return;
                Next.UnsubscribeListener();
                GenerateNextNode();
            };
        }

        public void SubscribeListenerFor( INotifyPropertyChanged inpcObject ) {
            mInpcObject = inpcObject;
            mInpcObject.PropertyChanged += OnPropertyChanged;

            if( Next != null ) GenerateNextNode();
        }

        private void GenerateNextNode() {
            var nextProperty = PropertyInfo.GetValue(mInpcObject);
            if( nextProperty == null ) return;
            if( !( nextProperty is INotifyPropertyChanged nextInpcObject ) )
                throw new InvalidOperationException( "Trying to subscribe PropertyChanged listener in object that " +
                                                    $"owns '{Next?.PropertyInfo.Name}' property, but the object does not implements INotifyPropertyChanged." );

            Next?.SubscribeListenerFor( nextInpcObject );
        }

        private void UnsubscribeListener() {
            if( mInpcObject != null )
                mInpcObject.PropertyChanged -= OnPropertyChanged;

            Next?.UnsubscribeListener();
        }

        private void OnPropertyChanged( object ? sender, PropertyChangedEventArgs e ) {
            if( e.PropertyName == PropertyInfo.Name || string.IsNullOrEmpty( e.PropertyName )) {
                mAction.Invoke();
            }
        }
    }
}
