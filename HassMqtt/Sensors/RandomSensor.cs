using HassMqtt.Discovery;

namespace HassMqtt.Sensors {
    public class RandomSensor : SensorBase {
        private const string   cSensorName = "random";

        public RandomSensor( string ? name = cSensorName, int ? updateIntervalSeconds = null, string ? id = default ) : 
            base( name ?? cSensorName, updateIntervalSeconds ?? 5, id ) {
        }

        protected override BaseDiscoveryModel OnDiscoveryModelCreated( SensorDiscoveryModel discoveryModel ) {
            discoveryModel.Icon = "mdi:chat-question";

            return discoveryModel;
        }

        protected override string GetState() => 
            Random.Shared.Next( 0, 100 ).ToString();
    }
}
