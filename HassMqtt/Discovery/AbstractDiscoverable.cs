using System.Text.RegularExpressions;
using HassMqtt.Context;
using HassMqtt.Models;

namespace HassMqtt.Discovery {
    /// <summary>
    /// Abstract discoverable object from which all Home Assistant entities are derived
    /// </summary>
    public abstract class AbstractDiscoverable {
        private string                      mObjectId;
        private BaseDiscoveryModel ?        mDeviceDiscoveryModel;

        protected IHassContextProvider ?    ContextProvider;

        public  string                      Name { get; }
        public  string                      Id { get; }
        public  string                      Domain { get; }

        public bool                         UseAttributes { get; }

        public int                          UpdateIntervalSeconds { get; }
        public DateTime                     LastUpdated { get; private set; }
        public string                       PreviousPublishedState { get; private set; }
        public string                       PreviousPublishedAttributes { get; private set; }

        protected AbstractDiscoverable( string name, string domain,
                                        int updateIntervalSeconds = 10, string? id = default, bool useAttributes = false ) {
            Name = name;
            Domain = domain;
            UseAttributes = useAttributes;

            Id = string.IsNullOrWhiteSpace( id ) ? NCuid.Cuid.Generate() : id;
            mObjectId = ObjectId;
            UpdateIntervalSeconds = updateIntervalSeconds;

            LastUpdated = DateTime.MinValue;
            PreviousPublishedState = string.Empty;
            PreviousPublishedAttributes = string.Empty;
        }

        public void InitializeParameters( IHassContextProvider contextProvider ) {
            ContextProvider = contextProvider;
        }

        public virtual string                   GetAttributes() => String.Empty;
        public virtual bool                     ProcessMessage( string topic, string payload ) => false;

        protected abstract BaseDiscoveryModel ? CreateDiscoveryModel();
        public abstract string                  GetCombinedState();
        public abstract IList<DeviceTopicState> GetStatesToPublish();

        public BaseDiscoveryModel ? GetDiscoveryModel() =>
            mDeviceDiscoveryModel ??= CreateDiscoveryModel();

        public void ClearDiscoveryModel() =>
            mDeviceDiscoveryModel = null;

        public void UpdatePublishedState( string state, string attributes ) {
            PreviousPublishedState = state;
            PreviousPublishedAttributes = attributes;
            LastUpdated = DateTime.Now;
        }

        public void ResetChecks() {
            LastUpdated = DateTime.MinValue;

            PreviousPublishedState = string.Empty;
            PreviousPublishedAttributes = string.Empty;
        }

        public string ObjectId {
            get {
                if( !string.IsNullOrEmpty( mObjectId ) ) {
                    return mObjectId;
                }

                mObjectId = Regex.Replace( Name, "[^a-zA-Z0-9_-]", "_" );

                return mObjectId;
            }

            set => mObjectId = Regex.Replace( value, "[^a-zA-Z0-9_-]", "_" );
        }
    }
}
