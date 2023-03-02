using System.Collections.Generic;
using HueLighting.Models;
using Mushrooms.Database;

namespace Mushrooms.Models {
    internal class Scene : EntityBase {
        public  string          SceneName { get; }
        public  ScenePalette    Palette { get; }
        public  SceneParameters Parameters { get; }
        public  SceneControl    Control { get; }
        public  IList<Bulb>     Bulbs { get; }

        public Scene( string sceneName, ScenePalette palette, SceneParameters parameters, SceneControl control,     
                      IEnumerable<Bulb> bulbs ) {
            SceneName = sceneName;
            Palette = palette;
            Parameters = parameters;
            Control = control;
            Bulbs = new List<Bulb>( bulbs );
        }
    }
}
