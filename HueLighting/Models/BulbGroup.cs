using System.Collections.Generic;
using System.Diagnostics;
using Q42.HueApi.Models.Groups;

namespace HueLighting.Models {
    [DebuggerDisplay("Group: {" + nameof( Name ) + "}")]
    public class BulbGroup {
        public  string      Id { get; }
        public  string      Name { get; }
        public  RoomClass ? RoomClass { get; }
        public  GroupType ? GroupType { get; }
        public  List<Bulb>  Bulbs { get; }

        public BulbGroup( string id, string name, GroupType ? groupType, RoomClass ? roomClass, IEnumerable<Bulb> bulbs ) {
            Id = id;
            Name = name;
            GroupType = groupType;
            RoomClass = roomClass;
            Bulbs = new List<Bulb>( bulbs );
        }
    }
}
