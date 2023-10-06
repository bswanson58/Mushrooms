using HassMqtt.Discovery;
using HassMqtt.Lights;
using HassMqtt.Models;
using HassMqtt.Support;

namespace HassMqtt.Sensors {
    public abstract class SensorBase : AbstractSingleValueSensor {
        private const string   cSensorName = "unknown sensor";

        protected SensorBase( string ? name = cSensorName, int ? updateIntervalSeconds = null,  string ? id = default ) : 
            base( name ?? cSensorName, updateIntervalSeconds ?? 5, id ) {
        }

        protected override BaseDiscoveryModel ? CreateDiscoveryModel() {
            if( HassContext == null ) {
                return null;
            }

            return new SensorDiscoveryModel {
                    AvailabilityTopic = HassContext.DeviceAvailabilityTopic(),
                    Name = Name,
                    UniqueId = Id,
                    Device = HassContext.DeviceConfiguration,
                    StateTopic = $"{HassContext.DeviceBaseTopic( Domain )}/{ObjectId}/{Constants.State}/{Constants.Status}"
            };
        }

        public override IList<DeviceTopicState> GetStatesToPublish() {
            var retValue = new List<DeviceTopicState>();

            if( GetDiscoveryModel() is LightDiscoveryModel discoveryModel ) {
                if(!String.IsNullOrWhiteSpace( discoveryModel.StateTopic )) {
                    retValue.Add( new DeviceTopicState( discoveryModel.StateTopic, GetState()));
                }
            }

            return retValue;
        }

        protected virtual BaseDiscoveryModel OnDiscoveryModelCreated( SensorDiscoveryModel discoveryModel ) =>
            discoveryModel;

        protected virtual string GetState() =>
            String.Empty;

        public override string GetCombinedState() =>
            GetState();
    }
}
