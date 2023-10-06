using System.Text.Json.Serialization;

namespace HassMqtt.Discovery {
    /// <summary>
    /// Base configuration model for Home Assistant entities.
    /// </summary>
    public abstract class BaseDiscoveryModel {
        /// <summary>
        /// (Optional) Information about the device this sensor is a part of to tie it into the device registry.
        /// Only works through MQTT discovery and when unique_id is set.
        /// </summary>
        /// <value></value>
        [JsonPropertyName( "device" )]
        public DeviceConfigModel? Device { get; set; }

        /// <summary>
        /// (Optional) The name of the MQTT sensor. Defaults to MQTT Sensor in hass.
        /// </summary>
        /// <value></value>
        [JsonPropertyName( "name" )]
        public string? Name { get; set; }

        /// <summary>
        /// The MQTT topic subscribed to receive sensor values.
        /// </summary>
        /// <value></value>
        [JsonPropertyName( "state_topic" )]
        public string? StateTopic { get; set; }
    }
}
