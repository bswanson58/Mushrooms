using System;
using System.Collections.Generic;
using System.Linq;
using Q42.HueApi.Models.Groups;

namespace HueLighting.Models {
    public class RoomClassItem {
        public  RoomClass   RoomClass { get; }
        public  string      DisplayName { get; }

        public RoomClassItem( RoomClass roomClass ) {
            RoomClass = roomClass;
            DisplayName = RoomClass.ToString();
        }
    }

    public static class RoomClassList {
        public static IEnumerable<RoomClassItem> Values() =>
            Enum.GetValues( typeof( RoomClass ))
                .Cast<RoomClass>()
                .Select( e => new RoomClassItem( e ))
                .OrderBy( g => g.DisplayName );
    }
}
