using ReusableBits.Platform.Preferences;
using ReusableBits.Wpf.EventAggregator;
using ReusableBits.Wpf.ViewModelSupport;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.Linq;
using System.Windows.Input;
using ReusableBits.Platform.Interfaces;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.Platform;
using ReusableBits.Wpf.StatusPresenter;
using ReusableBits.Wpf.VersionSpinner;

namespace Mushrooms.StatusBar {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class StatusViewModel : AutomaticPropertyBase, IHandle<Events.StatusEvent>, IDisposable {
		private readonly IEventAggregator       mEventAggregator;
		private readonly IApplicationConstants	mAppConstants;
		private readonly IEnvironment			mEnvironment;
        private readonly IVersionFormatter      mVersionFormatter;
		private readonly Queue<StatusMessage>	mHoldingQueue;
		private bool							mViewAttached;

		public	ICommand						OpenDataFolder { get; }
		public	ICommand						ViewAttached { get; }

		public	string							VersionString => $"{mAppConstants.ApplicationName} v{mVersionFormatter.VersionString}";

		public StatusViewModel( IEventAggregator eventAggregator, IEnvironment environment, 
                                IApplicationConstants applicationConstants, IVersionFormatter versionFormatter ) {
			mEventAggregator = eventAggregator;
			mAppConstants = applicationConstants;
			mEnvironment = environment;
            mVersionFormatter = versionFormatter;

			mHoldingQueue = new Queue<StatusMessage>();

			OpenDataFolder = new DelegateCommand( OnOpenDataFolder );
			ViewAttached = new DelegateCommand( OnViewAttached );

            mVersionFormatter.SetVersion( AssemblyInfo.Version );
            mVersionFormatter.DisplayLevel = VersionLevel.Build;
            mVersionFormatter.PropertyChanged += VersionFormatterOnPropertyChanged;

			mEventAggregator.Subscribe( this );
		}

        private void VersionFormatterOnPropertyChanged( object ? sender, PropertyChangedEventArgs e ) {
			if(!String.IsNullOrWhiteSpace( e.PropertyName )) {
                RaisePropertyChanged( e.PropertyName );
            }
        }

        public StatusMessage ? StatusMessage {
			get{ return Get( () => StatusMessage ); }
			set{ Set( () => StatusMessage, value ); }
		}

		private void OnViewAttached() {
			StatusMessage = new StatusMessage( string.Empty ); // delay a few seconds before initial message.

			StatusMessage = new StatusMessage( AssemblyInfo.Company );
            StatusMessage = new StatusMessage( AssemblyInfo.Description );
			StatusMessage = new StatusMessage( AssemblyInfo.Copyright );

			lock( mHoldingQueue ) {
				while( mHoldingQueue.Any()) {
					StatusMessage = mHoldingQueue.Dequeue();
				}
			}

            mVersionFormatter.StartFormatting();
			mViewAttached = true;
		}

		public void Handle( Events.StatusEvent status ) {
			var message = new StatusMessage( status.Message ) { ExtendActiveDisplay = status.ExtendDisplay };

			if( mViewAttached ) {
				StatusMessage = message;
			}
			else {
				lock( mHoldingQueue ) {
					mHoldingQueue.Enqueue( message );
				}
			}
		}

        private void OnOpenDataFolder() =>
            mEventAggregator.Publish( new Events.DisplayExplorerRequest( mEnvironment.ApplicationDirectory()));

        public void Dispose() {
            mEventAggregator.Unsubscribe( this );

            mVersionFormatter.Dispose();
        }
    }
}
