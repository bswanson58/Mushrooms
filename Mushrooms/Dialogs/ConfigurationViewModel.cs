﻿using System.Collections.ObjectModel;
using System.Linq;
using HueLighting.Hub;
using Mushrooms.Models;
using ReusableBits.Platform.Preferences;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.Platform;

namespace Mushrooms.Dialogs {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ConfigurationViewModel : DialogAwareBase {
        private readonly IHubManager        mHubManager;
        private readonly IDialogService     mDialogService;
        private readonly IPreferences       mPreferences;

        private double                      mLatitude;
        private double                      mLongitude;
        private bool                        mShouldMinimizeToTray;

        public  ObservableCollection<HubViewModel>  Hubs { get; }
        public  bool                                ScanningForHubs { get; private set; }

        public  DelegateCommand                     RegisterHubs { get; }

        public ConfigurationViewModel( IPreferences preferences, IHubManager hubManager, IDialogService dialogService ) {
            mPreferences = preferences;
            mDialogService = dialogService;
            mHubManager = hubManager;

            Hubs = new ObservableCollection<HubViewModel>();
            RegisterHubs = new DelegateCommand( OnRegisterHubs );
            ScanningForHubs = false;

            var uiPreferences = mPreferences.Load<MushroomPreferences>();

            Latitude = uiPreferences.Latitude;
            Longitude = uiPreferences.Longitude;
            ShouldMinimizeToTray = uiPreferences.ShouldMinimizeToTray;
        }

        public override void OnDialogOpened( IDialogParameters parameters ) {
            LoadRegisteredHubs();
        }

        public double Latitude {
            get => mLatitude;
            set {
                mLatitude = value;

                RaisePropertyChanged( () => Latitude );
            }
        }

        public double Longitude {
            get => mLongitude;
            set {
                mLongitude = value;

                RaisePropertyChanged( () => Longitude );
            }
        }

        public bool ShouldMinimizeToTray {
            get => mShouldMinimizeToTray;
            set {
                mShouldMinimizeToTray = value;

                RaisePropertyChanged( () => ShouldMinimizeToTray );
            }
        }

        private void OnRegisterHubs() {
            mDialogService.ShowDialog<HubRegistrationView>( result => {
                if( result.Result.Equals( ButtonResult.Ok )) {
                    LoadRegisteredHubs();
                }
            });
        }

        private async void LoadRegisteredHubs() {
            Hubs.Clear();

            var registeredHubs = await mHubManager.GetRegisteredHubs();

            ScanningForHubs = true;
            RaisePropertyChanged( () => ScanningForHubs );

            Hubs.AddRange( registeredHubs.Select( h => new HubViewModel( h )));

            ScanningForHubs = false;
            RaisePropertyChanged( () => ScanningForHubs );
        }

        protected override void OnOk() {
            var preferences = mPreferences.Load<MushroomPreferences>();

            preferences.Latitude = mLatitude;
            preferences.Longitude = mLongitude;
            preferences.ShouldMinimizeToTray = mShouldMinimizeToTray;

            mPreferences.Save( preferences );

            base.OnOk();
        }
    }
}