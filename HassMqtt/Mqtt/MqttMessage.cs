using MQTTnet.Extensions.ManagedClient;
using MQTTnet;

namespace HassMqtt.Mqtt {
    public class MqttMessage {
        public  string      Topic { get; }
        public  string      Payload { get; }

        public MqttMessage() {
            Topic = String.Empty;
            Payload = String.Empty;
        }

        public MqttMessage( MqttApplicationMessage message ) {
            Topic = message.Topic;
            Payload = System.Text.Encoding.Default.GetString( message.PayloadSegment );
        }

        public MqttMessage( ManagedMqttApplicationMessage message ) {
            Topic = message.ApplicationMessage.Topic;
            Payload = String.Empty;
        }
    }
}
