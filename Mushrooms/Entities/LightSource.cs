using Q42.HueApi.Models.Groups;

namespace Mushrooms.Entities {
    internal enum LightSourceType {
        Room = 1,
        Entertainment = 2,
        Zone = 3,
        Bulb = 4
    }

    internal class LightSource {
        public  string              SourceName { get; protected set; }
        public  LightSourceType     SourceType { get; protected set; }

        public LightSource( string name, LightSourceType type ) {
            SourceName = name;
            SourceType = type;
        }

        public LightSource( string name, GroupType groupType ) {
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
