using HassMqtt.Discovery;
using HassMqtt.Support;

namespace HassMqtt.Sensors {
    /// <summary>
    /// Abstract multiple value sensor from which all multiple value sensors are derived
    /// </summary>
    public abstract class AbstractMultiValueSensor : AbstractDiscoverable {
        public abstract Dictionary<string, AbstractSingleValueSensor> Sensors { get; protected set; }

        protected AbstractMultiValueSensor( string name, int updateIntervalSeconds = 10, string? id = default ) :
            base( name, Constants.SensorDomain, updateIntervalSeconds, id ) {
        }
    }
}
