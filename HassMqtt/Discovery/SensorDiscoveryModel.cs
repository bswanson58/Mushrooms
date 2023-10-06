using System.Text.Json.Serialization;

namespace HassMqtt.Discovery {
    public class SensorDiscoveryModel : BaseDiscoveryModel {
        /// <summary>
        /// (Optional) The MQTT topic subscribed to receive availability (online/offline) updates.
        /// </summary>
        /// <value></value>
        [JsonPropertyName( "availability_topic" )]
        public string? AvailabilityTopic { get; set; }

        /// <summary>
        /// (Optional) The type/class of the sensor to set the icon in the frontend.
        /// See https://www.home-assistant.io/integrations/sensor/#device-class for options.
        /// </summary>
        /// <value></value>
        [JsonPropertyName( "device_class" )]
        public string? DeviceClass { get; set; }

        /// <summary>
        /// (Optional) Defines the number of seconds after the sensor’s state expires, if it’s not updated.
        /// After expiry, the sensor’s state becomes unavailable. Defaults to 0 in hass.
        /// </summary>
        /// <value></value>
        [JsonPropertyName( "expire_after" )]
        public int? ExpireAfter { get; set; }

        /// <summary>
        /// Sends update events even if the value hasn't changed.
        /// Useful if you want to have meaningful value graphs in history.
        /// </summary>
        /// <value></value>
        [JsonPropertyName( "force_update" )]
        public bool? ForceUpdate { get; set; }

        /// <summary>
        /// (Optional) The icon for the sensor.
        /// </summary>
        /// <value></value>
        [JsonPropertyName( "icon" )]
        public string? Icon { get; set; }

        /// <summary>
        /// (Optional) Defines a template to extract the JSON dictionary from messages received on the json_attributes_topic.
        /// </summary>
        /// <value></value>
        [JsonPropertyName( "json_attributes_template" )]
        public string? JsonAttributesTemplate { get; set; }

        /// <summary>
        /// (Optional) The MQTT topic subscribed to receive a JSON dictionary payload and then set as sensor attributes.
        /// Implies force_update of the current sensor state when a message is received on this topic.
        /// </summary>
        /// <value></value>
        [JsonPropertyName( "json_attributes_topic" )]
        public string? JsonAttributesTopic { get; set; }

        /// <summary>
        /// (Optional) The payload that represents the available state.
        /// </summary>
        /// <value></value>
        [JsonPropertyName( "payload_available" )]
        public string? PayloadAvailable { get; set; }

        /// <summary>
        /// (Optional) The payload that represents the unavailable state.
        /// </summary>
        /// <value></value>
        [JsonPropertyName( "payload_not_available" )]
        public string? PayloadNotAvailable { get; set; }

        /// <summary>
        /// (Optional) The maximum QoS level of the state topic.
        /// </summary>
        /// <value></value>
        [JsonPropertyName( "qos" )]
        public int? Qos { get; set; }

        /// <summary>
        /// (Optional) An ID that uniquely identifies this sensor.
        /// Used instead of Name to generate the entity_id.
        /// If two sensors have the same unique ID, Home Assistant will raise an exception.
        /// </summary>
        /// <value></value>
        [JsonPropertyName( "unique_id" )]
        public string? UniqueId { get; set; }

        /// <summary>
        /// (Optional) Defines the units of measurement of the sensor, if any.
        /// </summary>
        /// <value></value>
        [JsonPropertyName( "unit_of_measurement" )]
        public string? UnitOfMeasurement { get; set; }

        /// <summary>
        /// (Optional) Defines a template to extract the value.
        /// </summary>
        /// <value></value>
        [JsonPropertyName( "value_template" )]
        public string? ValueTemplate { get; set; }
    }
}
