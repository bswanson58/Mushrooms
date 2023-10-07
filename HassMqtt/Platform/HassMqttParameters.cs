// ReSharper disable IdentifierTypo

namespace HassMqtt.Platform {
    public class HassMqttParameters {
        public  bool        MqttEnabled { get; set; }
        public  String      ServerAddress { get; set; }
        public  String      UserName {  get; set; }
        public  String      Password { get; set; }
        public  bool        UseRetainFlag { get; set; }
        public  String      DeviceName { get; set; }
        public  String      ClientIdentifier { get; set; }
    }
}
