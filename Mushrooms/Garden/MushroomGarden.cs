using System.Threading.Tasks;
using HueLighting.Hub;

namespace Mushrooms.Garden {
    public interface IMushroomGarden {
        Task<bool>  Initialize();
    }

    internal class MushroomGarden : IMushroomGarden {
        private readonly IHubManager    mHubManager;

        public MushroomGarden( IHubManager hubManager ) {
            mHubManager = hubManager;
        }

        public async Task<bool> Initialize() =>
            await mHubManager.InitializeConfiguredHub();
    }
}
