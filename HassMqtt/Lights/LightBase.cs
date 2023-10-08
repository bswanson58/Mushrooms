using HassMqtt.Discovery;
using HassMqtt.Models;
using HassMqtt.Support;

namespace HassMqtt.Lights {
    public abstract class LightBase : AbstractDiscoverable {
        private const string    cDeviceName = "unknown light";

        protected bool          mState;
        protected int           mBrightness;

        protected LightBase( string name = cDeviceName, int updateIntervalSeconds = 5,  string ? id = default ) : 
            base( String.IsNullOrWhiteSpace( name ) ? cDeviceName : name, Constants.LightDomain, updateIntervalSeconds, id ) {
            mState = false;
            mBrightness = 0;
        }

        protected override BaseDiscoveryModel ? CreateDiscoveryModel() {
            var context = ContextProvider?.Context;

            if( context == null ) {
                return null;
            }

            return new LightDiscoveryModel {
                AvailabilityTopic = context.DeviceAvailabilityTopic(),
                Name = Name,
                Icon = "mdi:lightbulb",
                UniqueId = Id,
                Device = context.DeviceConfiguration,
                StateTopic = $"{context.DeviceBaseTopic( Domain )}/{ObjectId}/{Constants.State}/{Constants.Status}",
                CommandTopic = $"{context.DeviceBaseTopic( Domain )}/{ObjectId}/{Constants.State}/{Constants.Subscribe}",
                BrightnessStateTopic = $"{context.DeviceBaseTopic( Domain )}/{ObjectId}/{Constants.Brightness}/{Constants.Status}",
                BrightnessCommandTopic = $"{context.DeviceBaseTopic( Domain )}/{ObjectId}/{Constants.Brightness}/{Constants.Subscribe}",
                BrightnessScale = 255
            };
        }

        public override IList<DeviceTopicState> GetStatesToPublish() {
            var retValue = new List<DeviceTopicState>();

            if( GetDiscoveryModel() is LightDiscoveryModel discoveryModel ) {
                if(!String.IsNullOrWhiteSpace( discoveryModel.StateTopic )) {
                    retValue.Add( new DeviceTopicState( discoveryModel.StateTopic, GetState()));
                }
                if(!String.IsNullOrWhiteSpace( discoveryModel.BrightnessStateTopic )) {
                    retValue.Add( new DeviceTopicState( discoveryModel.BrightnessStateTopic, GetBrightness()));
                }
            }

            return retValue;
        }

        public override bool ProcessMessage( string topic, string payload ) {
            if( GetDiscoveryModel() is LightDiscoveryModel discoveryModel ) {
                if( topic.Equals( discoveryModel.BrightnessCommandTopic )) {
                    OnBrightnessCommand( payload );

                    return true;
                }
                if( topic.Equals( discoveryModel.CommandTopic )) {
                    OnCommand( payload );

                    return true;
                }
            }

            return false;
        }

        private string GetState() =>
            mState ? Constants.OnState : Constants.OffState;

        private string GetBrightness() =>
            $"{mBrightness}";

        public override string GetCombinedState() =>
            $"{GetState()}|{GetBrightness()}";

        protected virtual void OnBrightnessCommand( string payload ) =>
            mBrightness = Int32.Parse( payload );

        protected virtual void OnCommand( string payload ) =>
            mState = payload.ToUpperInvariant().Equals( Constants.OnState.ToUpperInvariant());
    }
}
