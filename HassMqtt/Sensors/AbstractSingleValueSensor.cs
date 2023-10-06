using HassMqtt.Discovery;
using HassMqtt.Support;

namespace HassMqtt.Sensors {
    /// <summary>
    /// Abstract single value sensor from which all single value sensors are derived
    /// </summary>
    public abstract class AbstractSingleValueSensor : AbstractDiscoverable {
        protected AbstractSingleValueSensor( string name, int updateIntervalSeconds = 10, string? id = default,
                                             bool useAttributes = false ) :
            base( name, Constants.SensorDomain, updateIntervalSeconds, id, useAttributes ) {
        }
    }
}
