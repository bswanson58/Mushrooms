using HassMqtt.Discovery;
using HassMqtt.Platform;
using HassMqtt.Support;

// ReSharper disable IdentifierTypo

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
        private readonly IClientConfiguration   mClientConfiguration;

        public  bool                            MqttEnabled => mClientConfiguration.MqttConfiguration.MqttEnabled;
        public  bool                            UseMqttRetainFlag => mClientConfiguration.MqttConfiguration.UseRetainFlag;
        public  DeviceConfigModel               DeviceConfiguration => mClientConfiguration.DeviceConfiguration;

        public HassContext( IClientConfiguration clientConfiguration ) {
            mClientConfiguration = clientConfiguration;
        }

        public string  DeviceBaseTopic( string forDomain ) =>
            $"{mClientConfiguration.HassConfiguration.DiscoveryPrefix}/{forDomain}/{DeviceConfiguration.Name}";

        public string DeviceAvailabilityTopic() =>
            $"{mClientConfiguration.HassConfiguration.DiscoveryPrefix}/{DeviceConfiguration.Name}/{DeviceConfiguration.Identifiers}/{Constants.Availability}";

        public string DeviceMessageSubscriptionTopic() =>
            $"{mClientConfiguration.HassConfiguration.DiscoveryPrefix}/+/{DeviceConfiguration.Name}/#";
    }
}
