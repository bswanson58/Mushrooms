using Mushrooms.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Mushrooms.Models {
    internal class SceneViewModel {
        public  Scene               Scene { get; }

        public  string              Name => Scene.SceneName;
        public  IEnumerable<Color>  ExampleColors => Scene.Palette.Palette.Take( 7 );

        public SceneViewModel( Scene scene ) {
            Scene = scene;
        }
    }
}
