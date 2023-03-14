namespace Mushrooms.Models {
    internal class MushroomPreferences {
        public  double  Latitude { get; set; }
        public  double  Longitude { get; set; }

        public MushroomPreferences() {
            // Chicago
            Latitude = 41.997;
            Longitude = -88.458;
        }
    }
}
