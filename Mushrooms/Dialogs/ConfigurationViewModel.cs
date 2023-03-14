using Mushrooms.Models;
using ReusableBits.Platform.Preferences;
using ReusableBits.Wpf.DialogService;

namespace Mushrooms.Dialogs {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ConfigurationViewModel : DialogAwareBase {
        private readonly IPreferences       mPreferences;

        private double                      mLatitude;
        private double                      mLongitude;

        public ConfigurationViewModel( IPreferences preferences ) {
            mPreferences = preferences;

            var uiPreferences = mPreferences.Load<MushroomPreferences>();

            Latitude = uiPreferences.Latitude;
            Longitude = uiPreferences.Longitude;
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

        protected override void OnOk() {
            var preferences = mPreferences.Load<MushroomPreferences>();

            preferences.Latitude = mLatitude;
            preferences.Longitude = mLongitude;

            mPreferences.Save( preferences );

            base.OnOk();
        }
    }
}
