using System.Threading;
using System.Threading.Tasks;
using HueLighting.Hub;
using HueLighting.HubSelection;
using Microsoft.Extensions.Hosting;
using ReusableBits.Wpf.DialogService;

namespace Mushrooms {
    internal interface IMushroomGarden { }

    internal class MushroomGarden : IHostedService, IMushroomGarden {
        private readonly IHubManager        mHubManager;
        private readonly IDialogService     mDialogService;

        public MushroomGarden( IHubManager hubManager, IDialogService dialogService ) {
            mHubManager = hubManager;
            mDialogService = dialogService;
        }

        public async Task StartAsync( CancellationToken cancellationToken ) {
            if(! await mHubManager.InitializeConfiguredHub()) {
                mDialogService.ShowDialog<HubSelectionView>();
            }
        }

        public Task StopAsync( CancellationToken cancellationToken ) {

            return Task.CompletedTask;
        }
    }
}
