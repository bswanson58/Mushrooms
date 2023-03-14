using System.Collections.ObjectModel;
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
        private readonly IPreferences       mPreferences;

        private double                      mLatitude;
        private double                      mLongitude;

        public  ObservableCollection<HubViewModel>  Hubs { get; }
        public  bool                        ScanningForHubs { get; private set; }

        public  DelegateCommand             LocateHubs { get; }

        public ConfigurationViewModel( IPreferences preferences, IHubManager hubManager ) {
            mPreferences = preferences;
            mHubManager = hubManager;

            Hubs = new ObservableCollection<HubViewModel>();
            LocateHubs = new DelegateCommand( OnLocateHubs, CanLocateHubs );
            ScanningForHubs = false;

            var uiPreferences = mPreferences.Load<MushroomPreferences>();

            Latitude = uiPreferences.Latitude;
            Longitude = uiPreferences.Longitude;
        }

        public override async void OnDialogOpened( IDialogParameters parameters ) {
            Hubs.Clear();

            var registeredHubs = await mHubManager.GetRegisteredHubs();

            ScanningForHubs = true;
            RaisePropertyChanged( () => ScanningForHubs );

            Hubs.AddRange( registeredHubs.Select( h => new HubViewModel( h )));

            ScanningForHubs = false;
            RaisePropertyChanged( () => ScanningForHubs );
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

        private async void OnLocateHubs() {
            Hubs.Clear();

            ScanningForHubs = true;
            RaisePropertyChanged( () => ScanningForHubs );
            LocateHubs.RaiseCanExecuteChanged();

            var hubs = await mHubManager.LocateHubs();
            Hubs.AddRange( hubs.Select( h => new HubViewModel( h )));

            ScanningForHubs = false;
            RaisePropertyChanged( () => ScanningForHubs );
            LocateHubs.RaiseCanExecuteChanged();
        }

        private bool CanLocateHubs() => !ScanningForHubs;

        protected override void OnOk() {
            var preferences = mPreferences.Load<MushroomPreferences>();

            preferences.Latitude = mLatitude;
            preferences.Longitude = mLongitude;

            mPreferences.Save( preferences );

            base.OnOk();
        }
    }
}
