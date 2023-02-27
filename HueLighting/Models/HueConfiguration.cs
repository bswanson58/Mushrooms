using System;

namespace HueLighting.Models {
    public class HueConfiguration {
        public  String  BridgeIp;
        public  String  BridgeId;
        public  String  BridgeAppKey;
        public  String  BridgeStreamingKey;

        public HueConfiguration() {
            BridgeIp = String.Empty;
            BridgeId = String.Empty;
            BridgeAppKey = String.Empty;
            BridgeStreamingKey = String.Empty;
        }
    }
}
