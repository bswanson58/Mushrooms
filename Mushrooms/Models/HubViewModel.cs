using HueLighting.Hub;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.Models {
    internal class HubViewModel : PropertyChangeBase {
        private readonly HubInformation mHub;

        public  string                  IpAddress => mHub.IpAddress;
        public  string                  HubName => mHub.BridgeName;
        public  bool                    IsRegistered => mHub.IsAppRegistered;

        public  DelegateCommand         RegisterHub { get; }

        public HubViewModel( HubInformation hub ) {
            mHub = hub;

            RegisterHub = new DelegateCommand( OnRegisterHub, CanRegisterHub );
        }

        private void OnRegisterHub() { }

        private bool CanRegisterHub() => !IsRegistered;
    }
}
