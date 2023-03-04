using HueLighting.Models;
using System.Collections.Generic;
using Mushrooms.Entities;

namespace Mushrooms.Models {
    internal class LightSourceViewModel {
        public  LightSource     LightSource { get; }
        public  IList<Bulb>     Bulbs { get; }

        public  string          Name => LightSource.SourceName;

        public  bool            IsSelected { get; set; }

        public  string          DisplayName => $"{Name} ({LightSource.SourceType})";

        public LightSourceViewModel( LightSource lightSource, IEnumerable<Bulb> bulbs ) {
            LightSource = lightSource;
            Bulbs = new List<Bulb>( bulbs );

            IsSelected = false;
        }
    }
}
