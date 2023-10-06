using System.Text.RegularExpressions;
using HassMqtt.Discovery;
using HassMqtt.Models;
using HassMqtt.Mqtt;
using HassMqtt.Support;
using ReusableBits.Platform.Interfaces;
using ReusableBits.Platform.Preferences;

namespace HassMqtt.Platform {
    public interface IClientConfiguration {
        DeviceConfigModel   DeviceConfiguration { get; }
        MqttParameters      MqttConfiguration { get; }
        HassParameters      HassConfiguration { get; }

        string              LastWillTopic { get; }
        string              LastWillPayload { get; }
    }

    public class ClientConfiguration : IClientConfiguration {
        private readonly IPreferences   mPreferences;

        public  DeviceConfigModel   DeviceConfiguration { get; }
        public  MqttParameters      MqttConfiguration { get; }
        public  HassParameters      HassConfiguration { get; }

        public  string              LastWillTopic =>
            $"{HassConfiguration.DiscoveryPrefix}/{DeviceConfiguration.Name}/{DeviceConfiguration.Identifiers}/{Constants.Availability}";
        public  string              LastWillPayload => Constants.Offline;

        public ClientConfiguration( IPreferences preferences, IApplicationConstants appConstants ) {
            mPreferences = preferences;

            DeviceConfiguration = new DeviceConfigModel {
                Manufacturer = appConstants.CompanyName
            };

            InitDeviceConfig();

            MqttConfiguration = mPreferences.Load<MqttParameters>();
            HassConfiguration = mPreferences.Load<HassParameters>();
        }

        private void InitDeviceConfig() {
            var clientParameters = mPreferences.Load<ClientParameters>();

            DeviceConfiguration.Name = GetConfiguredDeviceName( clientParameters.DeviceName );
            DeviceConfiguration.Identifiers = GetConfiguredDeviceName( clientParameters.ClientIdentifier );
            DeviceConfiguration.Model = clientParameters.Model;
            DeviceConfiguration.SoftwareVersion = clientParameters.Version;
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
