using HassMqtt.Models;

namespace HassMqtt.Sensors {
    public static class SensorExtensions {
        /// <summary>
        /// Returns TRUE if the configured sensor is single-value
        /// </summary>
        /// <param name="configuredSensor"></param>
        /// <returns></returns>
        public static bool IsSingleValue(this ConfiguredSensor configuredSensor) => true; // configuredSensor.Type.IsSingleValue();

//        public static bool IsSingleValue(this SensorType sensorType) => !SensorsManager.SensorInfoCards[sensorType].MultiValue;
    }
}
