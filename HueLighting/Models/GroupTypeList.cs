using System;
using System.Collections.Generic;
using System.Linq;
using Q42.HueApi.Models.Groups;

namespace HueLighting.Models {
    public class GroupTypeItem {
        public  GroupType   GroupType { get; }
        public  string      DisplayName { get; }

        public GroupTypeItem( GroupType groupType ) {
            GroupType = groupType;
            DisplayName = groupType.ToString();
        }
    }

    public static class GroupTypeList {
        public static IEnumerable<GroupTypeItem> Values() =>
            Enum.GetValues( typeof( GroupType ))
                .Cast<GroupType>()
                .Select( e => new GroupTypeItem( e ))
                .OrderBy( g => g.DisplayName );
    }
}
