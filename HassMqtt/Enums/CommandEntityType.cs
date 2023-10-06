using System.ComponentModel;
using System.Runtime.Serialization;

namespace HassMqtt.Enums {
    /// <summary>
    /// Contains all possible command entity types
    /// </summary>
    public enum CommandEntityType {
        [Description( "Button" )]
        [EnumMember(Value = "button")]
        Button,

        [Description( "Light" )]
        [EnumMember(Value = "light")]
        Light,

        [Description( "Lock" )]
        [EnumMember(Value = "lock")]
        Lock,

        [Description( "Siren" )]
        [EnumMember(Value = "siren")]
        Siren,

        [Description( "Switch" )]
        [EnumMember(Value = "switch")]
        Switch,
    }
}