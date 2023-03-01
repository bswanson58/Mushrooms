using System;

namespace HueLighting.Models {
    public class HueConfiguration {
        public  String  BridgeIp { get; set; }
        public  String  BridgeId { get; set; }
        public  String  BridgeAppKey { get; set; }
        public  String  BridgeStreamingKey { get; set; }

        public HueConfiguration() {
            BridgeIp = String.Empty;
            BridgeId = String.Empty;
            BridgeAppKey = String.Empty;
            BridgeStreamingKey = String.Empty;
        }
    }
}
