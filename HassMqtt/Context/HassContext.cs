using HassMqtt.Discovery;
using HassMqtt.Models;
using HassMqtt.Mqtt;
using HassMqtt.Platform;
using HassMqtt.Support;

namespace HassMqtt.Context {
    public interface IHassContext {
        bool                MqttEnabled { get; }
        bool                UseMqttRetainFlag { get; }
        DeviceConfigModel   DeviceConfiguration { get; }

        string  DeviceAvailabilityTopic();
        string  DeviceMessageSubscriptionTopic();

        string  DeviceBaseTopic( string forDomain );
    }

    public class HassContext : IHassContext {
        private readonly HassParameters     mHassParameters;
        private readonly MqttParameters     mMqttParameters;

        public  bool                        MqttEnabled => mMqttParameters.MqttEnabled;
        public  bool                        UseMqttRetainFlag => mMqttParameters.UseRetainFlag;
        public  DeviceConfigModel           DeviceConfiguration { get; }

        public HassContext( IClientConfiguration clientConfiguration ) {
            mHassParameters = clientConfiguration.HassConfiguration;
            mMqttParameters = clientConfiguration.MqttConfiguration;
            DeviceConfiguration = clientConfiguration.DeviceConfiguration;
        }

        public string  DeviceBaseTopic( string forDomain ) =>
            $"{mHassParameters.DiscoveryPrefix}/{forDomain}/{DeviceConfiguration.Name}";

        public string DeviceAvailabilityTopic() =>
            $"{mHassParameters.DiscoveryPrefix}/{DeviceConfiguration.Name}/{DeviceConfiguration.Identifiers}/{Constants.Availability}";

        public string DeviceMessageSubscriptionTopic() =>
            $"{mHassParameters.DiscoveryPrefix}/+/{DeviceConfiguration.Name}/#";
    }
}
