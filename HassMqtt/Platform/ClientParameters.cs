namespace HassMqtt.Platform {
    public class ClientParameters {
        public  string  DeviceName { get; set; }
        public  string  ClientIdentifier { get; set; }
        public  string  Model { get; set; }
        public  string  Version { get; set; }

        public ClientParameters() {
            DeviceName = String.Empty;
            ClientIdentifier = String.Empty;
            Model = String.Empty;
            Version = String.Empty;
        }
    }
}
