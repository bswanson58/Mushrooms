namespace Mushrooms.Entities {
    internal class SceneControl {
        public  double  Brightness { get; protected set; }
        public  double  RateMultiplier { get; protected set; }

        private SceneControl() {
            Brightness = 0.7D;
            RateMultiplier = 1.0D;
        }

        public SceneControl( double brightness, double rateMultiplier ) {
            Brightness = brightness;
            RateMultiplier = rateMultiplier;
        }

        public static SceneControl Default => new ();
    }
}
