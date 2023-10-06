using ReusableBits.Platform.Preferences;
using ReusableBits.Wpf.EventAggregator;
using ReusableBits.Wpf.ViewModelSupport;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Mushrooms.Models;
using ReusableBits.Platform.Interfaces;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.Platform;
using ReusableBits.Wpf.StatusPresenter;
using ReusableBits.Wpf.VersionSpinner;
using Mushrooms.Support;

namespace Mushrooms.StatusBar {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class StatusViewModel : AutomaticPropertyBase, IHandle<Events.StatusEvent>, IDisposable {
		private readonly IEventAggregator       mEventAggregator;
		private readonly IPreferences			mPreferences;
		private readonly IApplicationConstants	mAppConstants;
		private readonly IEnvironment			mEnvironment;
        private readonly IVersionFormatter      mVersionFormatter;
        private readonly ICelestialCalculator   mCelestialCalculator;
		private readonly Queue<StatusMessage>	mHoldingQueue;
        private CelestialData ?                 mCelestialData;
		private bool							mViewAttached;
		private CancellationTokenSource			mTokenSource;

        public  bool                            IsDay {get; private set; }
        public  string                          CelestialInfo { get; private set; }

        public	ICommand						OpenDataFolder { get; }
		public	ICommand						ViewAttached { get; }

		public	string							VersionString => $"{mAppConstants.ApplicationName} v{mVersionFormatter.VersionString}";

		public StatusViewModel( IEventAggregator eventAggregator, IEnvironment environment, IPreferences preferences,
                                ICelestialCalculator celestialCalculator, IApplicationConstants applicationConstants, 
                                IVersionFormatter versionFormatter ) {
			mEventAggregator = eventAggregator;
			mAppConstants = applicationConstants;
			mPreferences = preferences;
			mCelestialCalculator = celestialCalculator;
			mEnvironment = environment;
            mVersionFormatter = versionFormatter;

			mHoldingQueue = new Queue<StatusMessage>();
			mTokenSource = new CancellationTokenSource();

			OpenDataFolder = new DelegateCommand( OnOpenDataFolder );
			ViewAttached = new DelegateCommand( OnViewAttached );

            mVersionFormatter.SetVersion( AssemblyInfo.Version );
            mVersionFormatter.DisplayLevel = VersionLevel.Build;
            mVersionFormatter.PropertyChanged += VersionFormatterOnPropertyChanged;

            mCelestialData = new CelestialData();
			IsDay = true;
			CelestialInfo = String.Empty;
			UpdateCelestialData();

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

			Repeat.Interval( TimeSpan.FromMinutes( 5 ), UpdateCelestialData, mTokenSource.Token );
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

        private void UpdateCelestialData() {
            if(( mCelestialData == null ) ||
               ( mCelestialData.SunRise.Date != DateTime.Now.Date )) {
                var preferences = mPreferences.Load<MushroomPreferences>();

                mCelestialData = mCelestialCalculator.CalculateData( preferences.Latitude, preferences.Longitude );
                var daylight = mCelestialData.SunSet - mCelestialData.SunRise;

                CelestialInfo = $"  Sunrise: {mCelestialData.SunRise:h:mm tt}\n   Sunset: {mCelestialData.SunSet:h:mm tt}\nDaylight: {daylight:h\\:mm} hours";
                RaisePropertyChanged( () => CelestialInfo );
            }

            IsDay = DateTime.Now > mCelestialData?.SunRise && DateTime.Now < mCelestialData?.SunSet;
            RaisePropertyChanged( () => IsDay );
        }


        public void Dispose() {
            mEventAggregator.Unsubscribe( this );

			mTokenSource.Cancel();
			mTokenSource.Dispose();

            mVersionFormatter.Dispose();
        }
    }
}
