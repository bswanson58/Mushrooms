using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading;
using System.Windows.Input;

// Adapted from the Prism library:
// source: https://github.com/PrismLibrary/Prism

namespace ReusableBits.Wpf.Commands {
    /// <summary>
    /// An <see cref="ICommand"/> whose delegates can be attached for <see cref="Execute"/> and <see cref="CanExecute"/>.
    /// </summary>
    public abstract class DelegateCommandBase : ICommand, IActiveAware {
        private bool mIsActive;

        private readonly SynchronizationContext ?   mSynchronizationContext;
        private readonly HashSet<string>            mObservedPropertiesExpressions = new ();

        /// <summary>
        /// Creates a new instance of a <see cref="DelegateCommandBase"/>, specifying both the execute action and the can execute function.
        /// </summary>
        protected DelegateCommandBase() {
            mSynchronizationContext = SynchronizationContext.Current;
            mIsActive = false;
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public virtual event EventHandler ? CanExecuteChanged;

        /// <summary>
        /// Raises <see cref="ICommand.CanExecuteChanged"/> so every 
        /// command invoker can re-query <see cref="ICommand.CanExecute"/>.
        /// </summary>
        protected virtual void OnCanExecuteChanged() {
            var handler = CanExecuteChanged;
            if( handler != null ) {
                if( mSynchronizationContext != null && mSynchronizationContext != SynchronizationContext.Current )
                    mSynchronizationContext.Post( _ => handler.Invoke( this, EventArgs.Empty ), null );
                else
                    handler.Invoke( this, EventArgs.Empty );
            }
        }

        /// <summary>
        /// Raises <see cref="CanExecuteChanged"/> so every command invoker
        /// can requery to check if the command can execute.
        /// </summary>
        /// <remarks>Note that this will trigger the execution of <see cref="CanExecuteChanged"/> once for each invoker.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1030:UseEventsWhereAppropriate" )]
        public void RaiseCanExecuteChanged() {
            OnCanExecuteChanged();
        }

        void ICommand.Execute( object ? parameter ) {
            Execute( parameter );
        }

        bool ICommand.CanExecute( object ? parameter ) {
            return CanExecute( parameter );
        }

        /// <summary>
        /// Handle the internal invocation of <see cref="ICommand.Execute(object)"/>
        /// </summary>
        /// <param name="parameter">Command Parameter</param>
        protected abstract void Execute( object ? parameter );

        /// <summary>
        /// Handle the internal invocation of <see cref="ICommand.CanExecute(object)"/>
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns><see langword="true"/> if the Command Can Execute, otherwise <see langword="false" /></returns>
        protected abstract bool CanExecute( object ? parameter );

        /// <summary>
        /// Observes a property that implements INotifyPropertyChanged, and automatically calls DelegateCommandBase.RaiseCanExecuteChanged on property changed notifications.
        /// </summary>
        /// <typeparam name="T">The object type containing the property specified in the expression.</typeparam>
        /// <param name="propertyExpression">The property expression. Example: ObservesProperty(() => PropertyName).</param>
        protected void ObservesPropertyInternal<T>( Expression<Func<T>> propertyExpression ) {
            if( mObservedPropertiesExpressions.Contains( propertyExpression.ToString() ) ) {
                throw new ArgumentException( $"{propertyExpression} is already being observed.",
                    nameof( propertyExpression ));
            }
            else {
                mObservedPropertiesExpressions.Add( propertyExpression.ToString() );
                PropertyObserver.Observes( propertyExpression, RaiseCanExecuteChanged );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the object is active.
        /// </summary>
        /// <value><see langword="true" /> if the object is active; otherwise <see langword="false" />.</value>
        public bool IsActive {
            get => mIsActive;
            set {
                if( mIsActive != value ) {
                    mIsActive = value;
                    OnIsActiveChanged();
                }
            }
        }

        /// <summary>
        /// Fired if the <see cref="IsActive"/> property changes.
        /// </summary>
        public virtual event EventHandler ? IsActiveChanged;

        /// <summary>
        /// This raises the <see cref="DelegateCommandBase.IsActiveChanged"/> event.
        /// </summary>
        protected virtual void OnIsActiveChanged() {
            IsActiveChanged?.Invoke( this, EventArgs.Empty );
        }
    }
}
