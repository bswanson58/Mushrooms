using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.Garden {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class GardenDisplayViewModel : PropertyChangeBase {
        private readonly IMushroomGarden    mGarden;

        public GardenDisplayViewModel( IMushroomGarden garden ) {
            mGarden = garden;
        }
    }
}
