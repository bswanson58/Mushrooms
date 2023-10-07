using System.Text.RegularExpressions;
using HassMqtt.Discovery;
using HassMqtt.Models;
using HassMqtt.Mqtt;
using HassMqtt.Support;
using ReusableBits.Platform.Interfaces;
using ReusableBits.Platform.Preferences;

// ReSharper disable IdentifierTypo

namespace HassMqtt.Platform {
    public interface IClientConfiguration {
        DeviceConfigModel   DeviceConfiguration { get; }
        MqttParameters      MqttConfiguration { get; }
        HassParameters      HassConfiguration { get; }

        string              LastWillTopic { get; }
        string              LastWillPayload { get; }

        HassMqttParameters  GetHassMqttParameters();
        void                SetHassMqttParameters( HassMqttParameters parameters );
    }

    public class ClientConfiguration : IClientConfiguration {
        private readonly IPreferences           mPreferences;
        private readonly IApplicationConstants  mAppConstants;

        public  MqttParameters      MqttConfiguration => mPreferences.Load<MqttParameters>();
        public  HassParameters      HassConfiguration => mPreferences.Load<HassParameters>();

        public  string              LastWillTopic =>
            $"{HassConfiguration.DiscoveryPrefix}/{DeviceConfiguration.Name}/{DeviceConfiguration.Identifiers}/{Constants.Availability}";
        public  string              LastWillPayload => Constants.Offline;

        public ClientConfiguration( IPreferences preferences, IApplicationConstants appConstants ) {
            mPreferences = preferences;
            mAppConstants = appConstants;
        }

        public DeviceConfigModel DeviceConfiguration {
            get {
                var clientParameters = mPreferences.Load<ClientParameters>();

                var deviceConfiguration = new DeviceConfigModel {
                    Manufacturer = mAppConstants.CompanyName,
                    Name = GetConfiguredDeviceName( clientParameters.DeviceName ),
                    Identifiers = GetConfiguredDeviceName( clientParameters.ClientIdentifier ),
                    Model = clientParameters.Model,
                    SoftwareVersion = clientParameters.Version
                };

                return deviceConfiguration;
            }
        }

        public HassMqttParameters GetHassMqttParameters() {
            var deviceParameters = DeviceConfiguration;
            var mqqtParameters = MqttConfiguration;

            return new HassMqttParameters {
                MqttEnabled = mqqtParameters.MqttEnabled,
                ServerAddress = mqqtParameters.ServerAddress,
                UserName = mqqtParameters.UserName,
                Password = mqqtParameters.Password,
                UseRetainFlag = mqqtParameters.UseRetainFlag,
                DeviceName = deviceParameters.Name ?? String.Empty,
                ClientIdentifier = deviceParameters.Identifiers ?? String.Empty

            };
        }

        public void SetHassMqttParameters( HassMqttParameters parameters ) {
            var mqttParameters = MqttConfiguration;

            mqttParameters.MqttEnabled = parameters.MqttEnabled;
            mqttParameters.ServerAddress = parameters.ServerAddress;
            mqttParameters.UserName = parameters.UserName;
            mqttParameters.Password = parameters.Password;
            mqttParameters.UseRetainFlag = parameters.UseRetainFlag;

            mPreferences.Save( mqttParameters );

            var clientParameters = mPreferences.Load<ClientParameters>();

            clientParameters.DeviceName = parameters.DeviceName;
            clientParameters.ClientIdentifier = parameters.ClientIdentifier;

            mPreferences.Save( clientParameters );
        }


        private static string GetConfiguredDeviceName( string clientName ) =>
            string.IsNullOrEmpty( clientName )
                ? GetSafeDeviceName() 
                : GetSafeValue( clientName );

        /// <summary>
        /// Returns a safe version of this machine's name
        /// </summary>
        /// <returns></returns>
        private static string GetSafeDeviceName() => 
            GetSafeValue( Environment.MachineName );

        /// <summary>
        /// Returns a safe version of the provided value
        /// </summary>
        /// <returns></returns>
        private static string GetSafeValue( string value ) {
            var val = Regex.Replace( value, @"[^a-zA-Z0-9_\-_\s]", "_" );

            return val.Replace( " ", String.Empty );
        }
    }
}
