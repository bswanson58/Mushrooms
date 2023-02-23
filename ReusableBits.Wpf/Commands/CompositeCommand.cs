using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using ReusableBits.Wpf.Properties;

// Adapted from the Prism library:
// source: https://github.com/PrismLibrary/Prism

namespace ReusableBits.Wpf.Commands {
    /// <summary>
    /// The CompositeCommand composes one or more ICommands.
    /// </summary>
    public class CompositeCommand : ICommand {
        private readonly List<ICommand>             mRegisteredCommands = new ();
        private readonly bool                       mMonitorCommandActivity;
        private readonly EventHandler               mOnRegisteredCommandCanExecuteChangedHandler;
        private readonly SynchronizationContext ?   mSynchronizationContext;

        /// <summary>
        /// Initializes a new instance of <see cref="CompositeCommand"/>.
        /// </summary>
        public CompositeCommand() {
            mOnRegisteredCommandCanExecuteChangedHandler = OnRegisteredCommandCanExecuteChanged;
            mSynchronizationContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CompositeCommand"/>.
        /// </summary>
        /// <param name="monitorCommandActivity">Indicates when the command activity is going to be monitored.</param>
        public CompositeCommand( bool monitorCommandActivity )
            : this() {
            mMonitorCommandActivity = monitorCommandActivity;
        }

        /// <summary>
        /// Adds a command to the collection and signs up for the <see cref="ICommand.CanExecuteChanged"/> event of it.
        /// </summary>
        ///  <remarks>
        /// If this command is set to monitor command activity, and <paramref name="command"/> 
        /// implements the <see cref="IActiveAware"/> interface, this method will subscribe to its
        /// <see cref="IActiveAware.IsActiveChanged"/> event.
        /// </remarks>
        /// <param name="command">The command to register.</param>
        public virtual void RegisterCommand( ICommand command ) {
            if( command == null ) throw new ArgumentNullException( nameof( command ));
            if( command == this ) {
                throw new ArgumentException( Resources.CannotRegisterCompositeCommandInItself );
            }

            lock( mRegisteredCommands ) {
                if( mRegisteredCommands.Contains( command ) ) {
                    throw new InvalidOperationException( Resources.CannotRegisterSameCommandTwice );
                }
                mRegisteredCommands.Add( command );
            }

            command.CanExecuteChanged += mOnRegisteredCommandCanExecuteChangedHandler;
            OnCanExecuteChanged();

            if( mMonitorCommandActivity ) {
                if( command is IActiveAware activeAwareCommand ) {
                    activeAwareCommand.IsActiveChanged += Command_IsActiveChanged;
                }
            }
        }

        /// <summary>
        /// Removes a command from the collection and removes itself from the <see cref="ICommand.CanExecuteChanged"/> event of it.
        /// </summary>
        /// <param name="command">The command to unregister.</param>
        public virtual void UnregisterCommand( ICommand command ) {
            if( command == null ) throw new ArgumentNullException( nameof( command ) );
            bool removed;
            lock( mRegisteredCommands ) {
                removed = mRegisteredCommands.Remove( command );
            }

            if( removed ) {
                command.CanExecuteChanged -= mOnRegisteredCommandCanExecuteChangedHandler;
                OnCanExecuteChanged();

                if( mMonitorCommandActivity ) {
                    if( command is IActiveAware activeAwareCommand ) {
                        activeAwareCommand.IsActiveChanged -= Command_IsActiveChanged;
                    }
                }
            }
        }

        private void OnRegisteredCommandCanExecuteChanged( object ? sender, EventArgs e ) {
            this.OnCanExecuteChanged();
        }


        /// <summary>
        /// Forwards <see cref="ICommand.CanExecute"/> to the registered commands and returns
        /// <see langword="true" /> if all of the commands return <see langword="true" />.
        /// </summary>
        /// <param name="parameter">Data used by the command.
        /// If the command does not require data to be passed, this object can be set to <see langword="null" />.
        /// </param>
        /// <returns><see langword="true" /> if all of the commands return <see langword="true" />; otherwise, <see langword="false" />.</returns>
        public virtual bool CanExecute( object ? parameter ) {
            bool hasEnabledCommandsThatShouldBeExecuted = false;

            ICommand[] commandList;
            lock( mRegisteredCommands ) {
                commandList = mRegisteredCommands.ToArray();
            }
            foreach( ICommand command in commandList ) {
                if( ShouldExecute( command )) {
                    if( !command.CanExecute( parameter )) {
                        return false;
                    }

                    hasEnabledCommandsThatShouldBeExecuted = true;
                }
            }

            return hasEnabledCommandsThatShouldBeExecuted;
        }

        /// <summary>
        /// Occurs when any of the registered commands raise <see cref="ICommand.CanExecuteChanged"/>.
        /// </summary>
        public virtual event EventHandler ? CanExecuteChanged;

        /// <summary>
        /// Forwards <see cref="ICommand.Execute"/> to the registered commands.
        /// </summary>
        /// <param name="parameter">Data used by the command.
        /// If the command does not require data to be passed, this object can be set to <see langword="null" />.
        /// </param>
        public virtual void Execute( object ? parameter ) {
            Queue<ICommand> commands;
            lock( mRegisteredCommands ) {
                commands = new Queue<ICommand>( mRegisteredCommands.Where( ShouldExecute ).ToList());
            }

            while( commands.Count > 0 ) {
                ICommand command = commands.Dequeue();
                command.Execute( parameter );
            }
        }

        /// <summary>
        /// Evaluates if a command should execute.
        /// </summary>
        /// <param name="command">The command to evaluate.</param>
        /// <returns>A <see cref="bool"/> value indicating whether the command should be used 
        /// when evaluating <see cref="CompositeCommand.CanExecute"/> and <see cref="CompositeCommand.Execute"/>.</returns>
        /// <remarks>
        /// If this command is set to monitor command activity, and <paramref name="command"/>
        /// implements the <see cref="IActiveAware"/> interface, 
        /// this method will return <see langword="false" /> if the command's <see cref="IActiveAware.IsActive"/> 
        /// property is <see langword="false" />; otherwise it always returns <see langword="true" />.</remarks>
        protected virtual bool ShouldExecute( ICommand command ) {
            if( mMonitorCommandActivity && command is IActiveAware activeAwareCommand ) {
                return activeAwareCommand.IsActive;
            }

            return true;
        }

        /// <summary>
        /// Gets the list of all the registered commands.
        /// </summary>
        /// <value>A list of registered commands.</value>
        /// <remarks>This returns a copy of the commands subscribed to the CompositeCommand.</remarks>
        public IList<ICommand> RegisteredCommands {
            get {
                IList<ICommand> commandList;
                lock( mRegisteredCommands ) {
                    commandList = this.mRegisteredCommands.ToList();
                }

                return commandList;
            }
        }

        /// <summary>
        /// Raises <see cref="ICommand.CanExecuteChanged"/> on the UI thread so every 
        /// command invoker can re-query <see cref="ICommand.CanExecute"/> to check if the
        /// <see cref="CompositeCommand"/> can execute.
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
        /// Handler for IsActiveChanged events of registered commands.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">EventArgs to pass to the event.</param>
        private void Command_IsActiveChanged( object ? sender, EventArgs e ) {
            OnCanExecuteChanged();
        }
    }
}
