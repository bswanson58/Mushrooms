
using HassMqtt.Discovery;
using System.Text.Json.Serialization;

namespace HassMqtt.Lights {
    public class LightDiscoveryModel : BaseDiscoveryModel {
        /// <summary>
        /// The MQTT topic subscribed to receive availability (online/offline) updates.
        /// Must not be used together with availability.
        /// </summary>
        [JsonPropertyName( "availability_topic" )]
        public string ? AvailabilityTopic { get; set; }

        /// <summary>
        /// When availability is configured, this controls the conditions needed to set the entity to available.
        /// Valid entries are all, any, and latest. If set to all, payload_available must be received on all
        /// configured availability topics before the entity is marked as online.
        /// If set to any, payload_available must be received on at least one configured availability topic
        /// before the entity is marked as online. If set to latest, the last payload_available or
        /// payload_not_available received on any configured availability topic controls the availability.
        /// </summary>
        [JsonPropertyName( "availability_mode" )]
        public string ? AvailabilityMode { get; set; }

        /// <summary>
        /// Defines a template to extract device’s availability from the availability_topic.
        /// To determine the devices’s availability result of this template will be compared to payload_available
        /// and payload_not_available.
        /// </summary>
        [JsonPropertyName( "availability_template" )]
        public string ? AvailabilityTemplate  { get; set; }

        /// <summary>
        /// The MQTT topic to publish commands to change the light’s brightness.
        /// </summary>
        [JsonPropertyName( "brightness_command_topic" )]
        public string ? BrightnessCommandTopic { get; set; }

        /// <summary>
        /// Defines a template to compose message which will be sent to brightness_command_topic. Available variables: value.
        /// </summary>
        [JsonPropertyName( "brightness_command_template" )]
        public string ? BrightnessCommandTemplate { get;set; }

        /// <summary>
        /// Defines the maximum brightness value (i.e., 100%) of the MQTT device. Defaults to 1255.
        /// </summary>
        [JsonPropertyName( "brightness_scale" )]
        public int ? BrightnessScale { get; set; }

        /// <summary>
        /// The MQTT topic subscribed to receive brightness state updates.
        /// </summary>
        [JsonPropertyName( "brightness_state_topic" )]
        public string ? BrightnessStateTopic { get; set; }

        /// <summary>
        /// Defines a template to extract the brightness value.
        /// </summary>
        [JsonPropertyName( "brightness_value_template" )]
        public string ? BrightnessValueTemplate { get; set; }

        /// <summary>
        /// The MQTT topic subscribed to receive color mode updates. If this is not configured,
        /// color_mode will be automatically set according to the last received valid color or color temperature
        /// </summary>
        [JsonPropertyName( "color_mode_state_topic" )]
        public string ? ColorModeStateTopic { get; set; }

        /// <summary>
        /// Defines a template to extract the color mode.
        /// </summary>
        [JsonPropertyName( "color_mode_value_template" )]
        public string ? ColorModeValueTemplate { get; set;}

        /// <summary>
        /// Defines a template to compose message which will be sent to color_temp_command_topic. Available variables: value.
        /// </summary>
        [JsonPropertyName( "color_temp_command_template" )]
        public string ? ColorTempCommandTemplate { get; set;}

        /// <summary>
        /// The MQTT topic to publish commands to change the light’s color temperature state.
        /// The color temperature command slider has a range of 153 to 500 mireds (micro reciprocal degrees).
        /// </summary>
        [JsonPropertyName( "color_temp_command_topic" )]
        public string ? ColorTempCommandTopic { get; set; }

        /// <summary>
        /// The MQTT topic subscribed to receive color temperature state updates.
        /// </summary>
        [JsonPropertyName( "color_temp_state_topic" )]
        public string ? ColorTempStateTopic { get; set; }

        /// <summary>
        /// Defines a template to extract the color temperature value.
        /// </summary>
        [JsonPropertyName( "color_temp_value_template" )]
        public string ?  ColorTempValueTemplate { get; set; }

        /// <summary>
        /// The MQTT topic to publish commands to change the switch state. Required!
        /// </summary>
        [JsonPropertyName( "command_topic" )]
        public string ? CommandTopic { get; set; }

        /// <summary>
        /// Flag which defines if the entity should be enabled when first added. Default: true
        /// </summary>
        [JsonPropertyName( "enabled_by_default" )]
        public bool ? EnabledByDefault { get; set; }

        /// <summary>
        /// The encoding of the payloads received and published messages. Set to "" to disable decoding of incoming payload.
        /// Default: utf8
        /// </summary>
        [JsonPropertyName( "encoding" )]
        public string ? Encoding { get; set; }

        /// <summary>
        /// The category of the entity. Default: None
        /// </summary>
        [JsonPropertyName( "entity_category" )]
        public string ? EntityCategory { get; set; }

        /// <summary>
        /// The MQTT topic to publish commands to change the light’s effect state.
        /// </summary>
        [JsonPropertyName( "effect_command_topic" )]
        public string ? EffectCommandTopic { get; set; }

        /// <summary>
        /// Defines a template to compose message which will be sent to effect_command_topic. Available variables: value.
        /// </summary>
        [JsonPropertyName( "effect_command_template" )]
        public string ? EffectCommandTemplate { get; set; }

        /// <summary>
        /// The list of effects the light supports.
        /// </summary>
        [JsonPropertyName( "effect_list" )]
        public string ? EffectList { get; set; }

        /// <summary>
        /// The MQTT topic subscribed to receive effect state updates.
        /// </summary>
        [JsonPropertyName( "effect_state_topic" )]
        public string ? EffectStateTopic { get; set; }

        /// <summary>
        ///         Defines a template to extract the effect value.
        /// </summary>
        [JsonPropertyName( "effect_value_template" )]
        public string ? EffectValueTemplate { get; set; }

        /// <summary>
        /// Defines a template to compose message which will be sent to hs_command_topic. Available variables: hue and sat.
        /// </summary>
        [JsonPropertyName( "hs_command_template" )]
        public string ? HsCommandTemplate {get; set; }

        /// <summary>
        /// The MQTT topic to publish commands to change the light’s color state in HS format (Hue Saturation).
        /// Range for Hue: 0° .. 360°, Range of Saturation: 0..100. Note:
        /// Brightness is sent separately in the brightness_command_topic.
        /// </summary>
        [JsonPropertyName( "hs_command_topic" )]
        public string ? HsCommandTopic { get; set; }

        /// <summary>
        /// The MQTT topic subscribed to receive color state updates in HS format.
        /// The expected payload is the hue and saturation values separated by commas, for example, 359.5,100.0.
        /// Note: Brightness is received separately in the brightness_state_topic.
        /// </summary>
        [JsonPropertyName( "hs_state_topic" )]
        public string ? HsStateTopic { get; set; }

        /// <summary>
        /// Defines a template to extract the HS value.
        /// </summary>
        [JsonPropertyName( "hs_value_template" )]
        public string ? HsValueTemplate { get; set; }

        /// <summary>
        /// Icon for the entity.
        /// </summary>
        [JsonPropertyName( "icon" )]
        public string ? Icon { get; set; }

        /// <summary>
        /// Defines a template to extract the JSON dictionary from messages received on the json_attributes_topic.
        /// Usage example can be found in MQTT sensor documentation.
        /// </summary>
        [JsonPropertyName( "json_attributes_template" )]
        public string ? JsonAttributesTemplate { get; set; }

        /// <summary>
        /// The MQTT topic subscribed to receive a JSON dictionary payload and then set as sensor attributes.
        /// Usage example can be found in MQTT sensor documentation.
        /// </summary>
        [JsonPropertyName( "json_attributes_topic" )]
        public string ? JsonAttributesTopic { get; set; }

        /// <summary>
        /// The maximum color temperature in mireds.
        /// </summary>
        [JsonPropertyName( "max_mireds" )]
        public int ? MaxMireds { get; set; }

        /// <summary>
        /// The minimum color temperature in mireds.
        /// </summary>
        [JsonPropertyName( "min_mireds" )]
        public int ? MinMireds { get; set; }

        /// <summary>
        /// Used instead of name for automatic generation of entity_id
        /// </summary>
        [JsonPropertyName( "object_id" )]
        public string ? ObjectId { get; set; }

        /// <summary>
        /// Defines when on the payload_on is sent. Using last (the default) will send any
        /// style (brightness, color, etc) topics first and then a payload_on to the command_topic.
        /// Using first will send the payload_on and then any style topics. Using brightness will only
        /// send brightness commands instead of the payload_on to turn the light on.
        /// </summary>
        [JsonPropertyName( "on_command_type" )]
        public string ? OnCommandType { get; set; }

        /// <summary>
        /// Flag that defines if switch works in optimistic mode.
        /// Default: true if no state topic defined, else false.
        /// </summary>
        [JsonPropertyName( "optimistic" )]
        public bool ? Optimistic { get; set; }

        /// <summary>
        /// The payload that represents the available state. Default: online
        /// </summary>
        [JsonPropertyName( "payload_available" )]
        public string ? PayloadAvailable { get; set; }

        /// <summary>
        /// The payload that represents the unavailable state.
        /// </summary>
        [JsonPropertyName( "payload_not_available" )]
        public string ? PayloadNotAvailable { get; set; }

        /// <summary>
        /// The payload that represents disabled state. Default: OFF
        /// </summary>
        [JsonPropertyName( "payload_off" )]
        public string ? PayloadOff { get; set; }

        /// <summary>
        /// The payload that represents enabled state. Default: ON
        /// </summary>
        [JsonPropertyName( "payload_on" )]
        public string ? PayloadOn { get; set; }

        /// <summary>
        /// The maximum QoS level to be used when receiving and publishing messages. Default: 0
        /// </summary>
        [JsonPropertyName( "qos" )]
        public int ? QualityOfService { get; set; }

        /// <summary>
        /// If the published message should have the retain flag on or not. Default: false
        /// </summary>
        [JsonPropertyName( "retain" )]
        public bool ? Retain { get; set; }

        /// <summary>
        /// Defines a template to compose message which will be sent to rgb_command_topic.
        /// Available variables: red, green and blue.
        /// </summary>
        [JsonPropertyName( "rgb_command_template" )]
        public string ? RgbCommandTemplate { get; set; }

        /// <summary>
        /// The MQTT topic to publish commands to change the light’s RGB state.
        /// </summary>
        [JsonPropertyName( "rgb_command_topic" )]
        public string ? RgbCommandTopic { get; set; }

        /// <summary>
        /// The MQTT topic subscribed to receive RGB state updates.
        /// The expected payload is the RGB values separated by commas, for example, 255,0,127.
        /// </summary>
        [JsonPropertyName( "rgb_state_topic" )]
        public string ? RgbStateTopic { get; set; }

        /// <summary>
        /// Defines a template to extract the RGB value.
        /// </summary>
        [JsonPropertyName( "rgb_value_template" )]
        public string ? RgbValueTemplate { get; set; }

        /// <summary>
        /// Defines a template to compose message which will be sent to rgbw_command_topic.
        /// Available variables: red, green, blue and white.
        /// </summary>
        [JsonPropertyName( "rgbw_command_template" )]
        public string ? RgbwCommandTemplate { get; set; }

        /// <summary>
        /// The MQTT topic to publish commands to change the light’s RGBW state.
        /// </summary>
        [JsonPropertyName( "rgbw_command_topic" )]
        public string ? RgbwCommandTopic { get; set; }

        /// <summary>
        /// The MQTT topic subscribed to receive RGBW state updates.
        /// The expected payload is the RGBW values separated by commas, for example, 255,0,127,64.
        /// </summary>
        [JsonPropertyName( "rgbw_state_topic" )]
        public string ? RgbwStateTopic { get; set; }

        /// <summary>
        /// Defines a template to extract the RGBW value.
        /// </summary>
        [JsonPropertyName( "rgbw_value_template" )]
        public string ? RgbwValueTemplate { get; set; }

        /// <summary>
        /// Defines a template to compose message which will be sent to rgbww_command_topic.
        /// Available variables: red, green, blue, cold_white and warm_white.
        /// </summary>
        [JsonPropertyName( "rgbww_command_template" )]
        public string ? RgbwwCommandTemplate { get; set; }

        /// <summary>
        /// The MQTT topic to publish commands to change the light’s RGBWW state.
        /// </summary>
        [JsonPropertyName( "rgbww_command_topic" )]
        public string ? RgbwwCommandTopic { get; set; }

        /// <summary>
        /// The MQTT topic subscribed to receive RGBWW state updates.
        /// The expected payload is the RGBWW values separated by commas, for example, 255,0,127,64,32.
        /// </summary>
        [JsonPropertyName( "rgbww_state_topic" )]
        public string ? RgbwwStateTopic { get; set; }

        /// <summary>
        /// Defines a template to extract the RGBWW value.
        /// </summary>
        [JsonPropertyName( "rgbww_value_template" )]
        public string ? RgbwwValueTemplate { get; set; }

        /// <summary>
        /// The schema to use. Must be default or omitted to select the default schema. Default: default
        /// </summary>
        [JsonPropertyName( "schema" )]
        public string ? Schema { get; set; }

        /// <summary>
        /// Defines a template to extract the state value.
        /// The template should match the payload on and off values, so if your light uses power on to turn on,
        /// your state_value_template string should return power on when the switch is on.
        /// For example if the message is just on, your state_value_template should be power .
        /// </summary>
        [JsonPropertyName( "state_value_template" )]
        public string ? StateValueTemplate { get; set; }

        /// <summary>
        /// An ID that uniquely identifies this light. If two lights have the same unique ID,
        /// Home Assistant will raise an exception.
        /// </summary>
        [JsonPropertyName( "unique_id" )]
        public string ? UniqueId { get; set; }

        /// <summary>
        /// The MQTT topic to publish commands to change the light to white mode with a given brightness.
        /// </summary>
        [JsonPropertyName( "white_command_topic" )]
        public string ? WhiteCommandTopic { get; set; }

        /// <summary>
        /// Defines the maximum white level (i.e., 100%) of the MQTT device. Default: 255
        /// </summary>
        [JsonPropertyName( "white_scale" )]
        public int ? WhiteScale { get; set; }

        /// <summary>
        /// Defines a template to compose message which will be sent to xy_command_topic.
        /// Available variables: x and y.
        /// </summary>
        [JsonPropertyName( "xy_command_template" )]
        public string ? XyCommandTemplate { get; set; }

        /// <summary>
        /// The MQTT topic to publish commands to change the light’s XY state.
        /// </summary>
        [JsonPropertyName( "xy_command_topic" )]
        public string ? XyCommandTopic { get; set; }

        /// <summary>
        /// The MQTT topic subscribed to receive XY state updates.
        /// The expected payload is the X and Y color values separated by commas, for example, 0.675,0.322.
        /// </summary>
        [JsonPropertyName( "xy_state_topic" )]
        public string ? XyStateTopic { get; set; }

        /// <summary>
        /// Defines a template to extract the XY value.    }
        /// </summary>
        [JsonPropertyName( "xy_value_template" )]
        public string ? XyValueTemplate { get; set; }
    }
}
