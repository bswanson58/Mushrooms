using HueLighting.Hub;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.Models {
    internal class HubViewModel : PropertyChangeBase {
        public  HubInformation  Hub { get; }

        public  string          IpAddress => Hub.IpAddress;
        public  string          HubName => Hub.BridgeName;
        public  bool            IsRegistered => Hub.IsAppRegistered;

        public HubViewModel( HubInformation hub ) {
            Hub = hub;
        }
    }
}
