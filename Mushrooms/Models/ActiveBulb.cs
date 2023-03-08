using HueLighting.Models;
using System.Windows.Media;
using System;

namespace Mushrooms.Models {
    internal class ActiveBulb {
        public  Bulb        Bulb { get; }
        public  Color       ActiveColor { get; }
        public  DateTime    NextUpdateTime { get; }

        public ActiveBulb( Bulb bulb ) {
            ActiveColor = Colors.Transparent;
            Bulb = bulb;

            NextUpdateTime = DateTime.MinValue;
        }

        public ActiveBulb( Bulb bulb, Color color, DateTime nextUpdateTime ) {
            Bulb = bulb;
            ActiveColor = color;
            NextUpdateTime = nextUpdateTime;
        }
    }
}
