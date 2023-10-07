using System;
using HassMqtt;
using HassMqtt.Platform;
using ReusableBits.Wpf.DialogService;

// ReSharper disable IdentifierTypo

namespace Mushrooms.HassIntegration {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class HassParametersViewModel : DialogAwareBase {
        private readonly IHassManager           mHassManager;
        private bool                            mEnableIntegration;
        private bool                            mUseRetainFlag;
        private string                          mServerAddress;
        private string                          mUserName;
        private string                          mPassword;
        private string                          mDeviceName;
        private string                          mClientIdentifier;

        public  bool                            IsIntegrationEnabled => mEnableIntegration;

        public HassParametersViewModel( IHassManager hassManager ) {
            mHassManager = hassManager;

            var parameters = mHassManager.GetHassMqttParameters();

            mEnableIntegration = parameters.MqttEnabled;
            mServerAddress = parameters.ServerAddress;
            mUserName = parameters.UserName;
            mPassword = parameters.Password;
            mUseRetainFlag = parameters.UseRetainFlag;
            mDeviceName = parameters.DeviceName;
            mClientIdentifier = parameters.ClientIdentifier;

            Title = "Home Assistant Integration";
        }

        public bool EnableIntegration {
            get => mEnableIntegration;
            set {
                mEnableIntegration = value;

                RaisePropertyChanged(() => EnableIntegration );
                RaisePropertyChanged(() => IsIntegrationEnabled );
                Ok.RaiseCanExecuteChanged();
            }
        }

        public string ServerAddress {
            get => mServerAddress;
            set {
                mServerAddress = value;

                Ok.RaiseCanExecuteChanged();
            }
        }

        public string UserName {
            get => mUserName;
            set {
                mUserName = value;

                Ok.RaiseCanExecuteChanged();
            }
        }

        public string Password {
            get => mPassword;
            set {
                mPassword = value;

                Ok.RaiseCanExecuteChanged();
            }
        }

        public string DeviceName {
            get => mDeviceName;
            set {
                mDeviceName = value;

                Ok.RaiseCanExecuteChanged();
            }
        }

        public string ClientIdentifier {
            get => mClientIdentifier;
            set {
                mClientIdentifier = value;

                Ok.RaiseCanExecuteChanged();
            }
        }

        public bool UseRetainFlag {
            get => mUseRetainFlag;
            set {
                mUseRetainFlag = value;

                Ok.RaiseCanExecuteChanged();
            }
        }

        protected override bool CanAccept() =>
            !mEnableIntegration ||
            !String.IsNullOrWhiteSpace( mServerAddress ) &&
            !String.IsNullOrWhiteSpace( mDeviceName ) &&
            !String.IsNullOrWhiteSpace( mClientIdentifier );

        protected override void OnOk() {
            var parameters = new HassMqttParameters {
                MqttEnabled = mEnableIntegration,
                ServerAddress = mServerAddress,
                UserName = mUserName,
                Password = mPassword,
                UseRetainFlag = mUseRetainFlag,
                DeviceName = mDeviceName,
                ClientIdentifier = mClientIdentifier
            };

            mHassManager.SetHassMqttParameters( parameters );

            base.OnOk();
        }
    }
}
