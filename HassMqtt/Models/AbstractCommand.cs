using HassMqtt.Enums;
using HassMqtt.Support;
using HassMqtt.Extensions;
using HassMqtt.Discovery;

namespace HassMqtt.Models {
    /// <summary>
    /// Abstract command from which all commands are derived
    /// </summary>
    public abstract class AbstractCommand : AbstractDiscoverable {
        public  CommandEntityType   EntityType { get; }

        protected AbstractCommand( string name, CommandEntityType entityType = CommandEntityType.Switch, string ? id = default ) :
            base( name, entityType.GetEnumMemberValue() ?? Constants.SensorDomain, 1, id ) {
            EntityType = entityType;
        }

        public abstract string GetState();
        public abstract void TurnOn();
        public abstract void TurnOnWithAction( string action );
        public abstract void TurnOff();
    }
}
