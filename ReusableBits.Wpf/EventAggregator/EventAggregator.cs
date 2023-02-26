using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if NETFX_CORE && !WinRT
#define WinRT
#endif

// from Calburn.Micro

namespace ReusableBits.Wpf.EventAggregator {
    /// <summary>
    ///   A marker interface for classes that subscribe to messages.
    /// </summary>
    public interface IHandle { }

    /// <summary>
    ///   Denotes a class which can handle a particular type of message.
    /// </summary>
    /// <typeparam name = "TMessage">The type of message to handle.</typeparam>
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IHandle<TMessage> : IHandle {  //don't use contravariance here
        /// <summary>
        ///   Handles the message.
        /// </summary>
        /// <param name = "message">The message.</param>
        void Handle( TMessage message );
    }

    /// <summary>
    ///   Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public interface IEventAggregator {
        /// <summary>
        ///   Gets or sets the default publication thread marshaller.
        /// </summary>
        /// <value>
        ///   The default publication thread marshaller.
        /// </value>
        Action<Action> PublicationThreadMarshaller { get; set; }

        /// <summary>
        /// Searches the subscribed handlers to check if we have a handler for
        /// the message type supplied.
        /// </summary>
        /// <param name="messageType">The message type to check with</param>
        /// <returns>True if any handler is found, false if not.</returns>
        bool HandlerExistsFor( Type messageType );

        /// <summary>
        ///   Subscribes an instance to all events declared through implementations of <see cref = "IHandle{T}" />
        /// </summary>
        /// <param name = "subscriber">The instance to subscribe for event publication.</param>
        void Subscribe( object subscriber );

        /// <summary>
        ///   Unsubscribes the instance from all events.
        /// </summary>
        /// <param name = "subscriber">The instance to unsubscribe.</param>
        void Unsubscribe( object subscriber );

        /// <summary>
        ///   Publishes a message.
        /// </summary>
        /// <param name = "message">The message instance.</param>
        /// <remarks>
        ///   Uses the default thread marshaller during publication.
        /// </remarks>
        void Publish( object message );

        /// <summary>
        ///   Publishes a message.
        /// </summary>
        /// <param name = "message">The message instance.</param>
        /// <param name = "marshal">Allows the publisher to provide a custom thread marshaller for the message publication.</param>
        void Publish( object message, Action<Action> marshal );
    }

    /// <summary>
    ///   Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public class EventAggregator : IEventAggregator {
        readonly List<Handler> mHandlers = new ();

        /// <summary>
        ///   The default thread marshaller used for publication;
        /// </summary>
        public static readonly Action<Action> DefaultPublicationThreadMarshaller = action => action();

        /// <summary>
        /// Processing of handler results on publication thread.
        /// </summary>
        // ReSharper disable UnusedParameter.Local
        public static readonly Action<object, object> HandlerResultProcessing = ( target, result ) => { };
        // ReSharper restore UnusedParameter.Local

        /// <summary>
        ///   Initializes a new instance of the <see cref = "EventAggregator" /> class.
        /// </summary>
        public EventAggregator() {
            PublicationThreadMarshaller = DefaultPublicationThreadMarshaller;
        }

        /// <summary>
        ///   Gets or sets the default publication thread marshaller.
        /// </summary>
        /// <value>
        ///   The default publication thread marshaller.
        /// </value>
        public Action<Action> PublicationThreadMarshaller { get; set; }

        /// <summary>
        /// Searches the subscribed handlers to check if we have a handler for
        /// the message type supplied.
        /// </summary>
        /// <param name="messageType">The message type to check with</param>
        /// <returns>True if any handler is found, false if not.</returns>
        public bool HandlerExistsFor( Type messageType ) {
            lock ( mHandlers ) {
                return mHandlers.Any( handler => handler.Handles( messageType ) & !handler.IsDead );
            }
        }

        /// <summary>
        ///   Subscribes an instance to all events declared through implementations of <see cref = "IHandle{T}" />
        /// </summary>
        /// <param name = "subscriber">The instance to subscribe for event publication.</param>
        public virtual void Subscribe( object subscriber ) {
            if( subscriber == null ) {
                throw new ArgumentNullException( nameof( subscriber ));
            }
            lock( mHandlers ) {
                if( mHandlers.Any( x => x.Matches( subscriber ))) {
                    return;
                }

                mHandlers.Add( new Handler( subscriber ) );
            }
        }

        /// <summary>
        ///   Unsubscribes the instance from all events.
        /// </summary>
        /// <param name = "subscriber">The instance to unsubscribe.</param>
        public virtual void Unsubscribe( object subscriber ) {
            if( subscriber == null ) {
                throw new ArgumentNullException( nameof( subscriber ));
            }
            lock( mHandlers ) {
                var found = mHandlers.FirstOrDefault(x => x.Matches(subscriber));

                if( found != null ) {
                    mHandlers.Remove( found );
                }
            }
        }

        /// <summary>
        ///   Publishes a message.
        /// </summary>
        /// <param name = "message">The message instance.</param>
        /// <remarks>
        ///   Does not marshall the the publication to any special thread by default.
        /// </remarks>
        public virtual void Publish( object message ) {
            if( message == null ) {
                throw new ArgumentNullException( nameof( message ));
            }

            Publish( message, PublicationThreadMarshaller );
        }

        /// <summary>
        ///   Publishes a message.
        /// </summary>
        /// <param name = "message">The message instance.</param>
        /// <param name = "marshal">Allows the publisher to provide a custom thread marshaller for the message publication.</param>
        public virtual void Publish( object message, Action<Action> marshal ) {
            if( message == null ) {
                throw new ArgumentNullException( nameof( message ));
            }
            if( marshal == null ) {
                throw new ArgumentNullException( nameof( marshal ));
            }

            Handler[] toNotify;
            lock( mHandlers ) {
                toNotify = mHandlers.ToArray();
            }

            marshal( () => {
                var messageType = message.GetType();

                var dead = toNotify
                    .Where(handler => !handler.Handle( messageType, message ))
                    .ToList();

                if( dead.Any() ) {
                    lock( mHandlers ) {
                        dead.Apply( x => mHandlers.Remove( x ) );
                    }
                }
            } );
        }

        class Handler {
            readonly WeakReference                  mReference;
            readonly Dictionary<Type, MethodInfo>   mSupportedHandlers = new ();

            public bool IsDead => 
                mReference.Target == null;

            public Handler( object handler ) {
                mReference = new WeakReference( handler );

#if WinRT
                var handlerInfo = typeof(IHandle).GetTypeInfo();
                var interfaces = handler.GetType().GetTypeInfo().ImplementedInterfaces
                    .Where(x => handlerInfo.IsAssignableFrom(x.GetTypeInfo()) && x.GetTypeInfo().IsGenericType);

                foreach (var @interface in interfaces) {
                    var type = @interface.GenericTypeArguments[0];
                    var method = @interface.GetTypeInfo().DeclaredMethods.First(x => x.Name == "Handle");
                    supportedHandlers[type] = method;
                }
#else
                var interfaces = handler.GetType().GetInterfaces()
                    .Where(x => typeof(IHandle).IsAssignableFrom(x) && x.IsGenericType );

                foreach( var @interface in interfaces ) {
                    var type = @interface.GetGenericArguments()[0];
                    var method = @interface.GetMethod( "Handle" );

                    if ( method != null ) {
                        mSupportedHandlers[type] = method;
                    }
                }
#endif
            }

            public bool Matches( object instance ) {
                return mReference.Target == instance;
            }

            public bool Handle( Type messageType, object message ) {
                var target = mReference.Target;

                if( target == null ) {
                    return false;
                }

                foreach( var pair in mSupportedHandlers ) {
                    if( pair.Key.IsAssignableFrom( messageType )) {
                        var result = pair.Value.Invoke(target, new[] { message });
                        if( result != null ) {
                            HandlerResultProcessing( target, result );
                        }
                    }
                }

                return true;
            }

            public bool Handles( Type messageType ) {
                return mSupportedHandlers.Any( pair => pair.Key.IsAssignableFrom( messageType ));
            }
        }
    }
}
