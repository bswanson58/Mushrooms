using System.Windows.Input;
using HueLighting.HubSelection;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.Garden {
    internal class GardenDisplayViewModel : PropertyChangeBase {
        private readonly IMushroomGarden    mGarden;
        private readonly IDialogService     mDialogService;

        public  ICommand                    ViewLoaded { get; }

        public GardenDisplayViewModel( IMushroomGarden garden, IDialogService dialogService ) {
            mGarden = garden;
            mDialogService = dialogService;

            ViewLoaded = new DelegateCommand( OnViewLoaded );
        }

        private async void OnViewLoaded() {
            if(! await mGarden.Initialize()) {
                mDialogService.ShowDialog<HubSelectionView>();
            }
        }
    }
}
