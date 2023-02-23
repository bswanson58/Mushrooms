using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

// Adapted from the Prism library:
// source: https://github.com/PrismLibrary/Prism

namespace ReusableBits.Wpf.Commands {
    /// <summary>
    /// Provide a way to observe property changes of INotifyPropertyChanged objects and invokes a 
    /// custom action when the PropertyChanged event is fired.
    /// </summary>
    internal class PropertyObserver {
        private readonly Action mAction;

        private PropertyObserver( Expression propertyExpression, Action action ) {
            mAction = action;
            SubscribeListeners( propertyExpression );
        }

        private void SubscribeListeners( Expression ? propertyExpression ) {
            var propNameStack = new Stack<PropertyInfo>();
            while( propertyExpression is MemberExpression temp ) // Gets the root of the property chain.
            {
                propertyExpression = temp.Expression;

                if( temp.Member is PropertyInfo propInfo ) {
                    propNameStack.Push( propInfo ); // Records the member info as property info
                }
            }

            if( !( propertyExpression is ConstantExpression constantExpression ) )
                throw new NotSupportedException( "Operation not supported for the given expression type. " +
                                                "Only MemberExpression and ConstantExpression are currently supported." );

            var propObserverNodeRoot = new PropertyObserverNode(propNameStack.Pop(), mAction);
            PropertyObserverNode previousNode = propObserverNodeRoot;
            foreach( var propName in propNameStack ) // Create a node chain that corresponds to the property chain.
            {
                var currentNode = new PropertyObserverNode(propName, mAction);
                previousNode.Next = currentNode;
                previousNode = currentNode;
            }

            object ? propOwnerObject = constantExpression.Value;

            if( !( propOwnerObject is INotifyPropertyChanged inpcObject ) )
                throw new InvalidOperationException( "Trying to subscribe PropertyChanged listener in object that " +
                                                    $"owns '{propObserverNodeRoot.PropertyInfo.Name}' property, but the object does not implements INotifyPropertyChanged." );

            propObserverNodeRoot.SubscribeListenerFor( inpcObject );
        }

        /// <summary>
        /// Observes a property that implements INotifyPropertyChanged, and automatically calls a custom action on 
        /// property changed notifications. The given expression must be in this form: "() => Prop.NestedProp.PropToObserve".
        /// </summary>
        /// <param name="propertyExpression">Expression representing property to be observed. Ex.: "() => Prop.NestedProp.PropToObserve".</param>
        /// <param name="action">Action to be invoked when PropertyChanged event occurs.</param>
        internal static PropertyObserver Observes<T>( Expression<Func<T>> propertyExpression, Action action ) {
            return new PropertyObserver( propertyExpression.Body, action );
        }
    }
}
