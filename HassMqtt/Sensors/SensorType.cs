using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace HassMqtt.Sensors {
    /// <summary>
    /// Contains all possible sensor types
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum SensorType {
        [Description("Battery Sensor")]
        [EnumMember( Value = "BatterySensor" )]
        BatterySensor,

        [Description("Random Sensor")]
        [EnumMember( Value = "RandomSensor" )]
        RandomSensor
    }
}