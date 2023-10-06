using HassMqtt.Support;

namespace HassMqtt.Models {
    public class HassParameters {
        public  string      DiscoveryPrefix { get; set; }
        public  string      DeviceSerialNumber { get; set; }
        public HassParameters() {
            DiscoveryPrefix = Constants.DiscoveryPrefix;
            DeviceSerialNumber = String.Empty;
        }
    }
}
