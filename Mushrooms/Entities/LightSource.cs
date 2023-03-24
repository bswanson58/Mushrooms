using Q42.HueApi.Models.Groups;

namespace Mushrooms.Entities {
    internal enum LightSourceType {
        Room = 1,
        Entertainment = 2,
        Zone = 3,
        Bulb = 4
    }

    internal class LightSource {
        public  string              Id { get; private set; }
        public  string              SourceName { get; protected set; }
        public  LightSourceType     SourceType { get; protected set; }

        public LightSource( string id,  string name, LightSourceType type ) {
            Id = id;
            SourceName = name;
            SourceType = type;
        }

        public LightSource( string id, string name, GroupType ? groupType ) {
            Id = id;
            SourceName = name;

            switch ( groupType ) {
                case GroupType.Room:
                    SourceType = LightSourceType.Room;
                    break;

                case GroupType.Zone:
                    SourceType = LightSourceType.Zone;
                    break;

                case GroupType.Entertainment:
                    SourceType = LightSourceType.Entertainment;
                    break;

                case GroupType.Free:
                    SourceType = LightSourceType.Bulb;
                    break;
            }
        }
    }
}
