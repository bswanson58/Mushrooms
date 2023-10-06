using System.Reactive.Linq;
using System.Reactive.Subjects;
using HassMqtt.Platform;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;
using OneOf;
using OneOf.Types;

namespace HassMqtt.Mqtt {
    public enum MqttStatus {
        Uninitialized,
        Connected,
        Connecting,
        Disconnected,
        Error
    }

    public interface IMqttManager : IDisposable {
        bool        IsEnabled { get; }
        bool        IsConnected { get; }
        MqttStatus  Status { get; }

        IObservable<MqttMessage>        OnMessageReceived { get; }
        IObservable<MqttMessage>        OnMessageProcessed {  get; }

        Task<OneOf<None, Exception>>    InitializeAsync();

        Task<OneOf<None, Exception>>    PublishAsync( string topic, string payload );
        Task<OneOf<None, Exception>>    PublishAsync( MqttApplicationMessage message );

        Task<OneOf<None, Exception>>    SubscribeAsync( string topic );
        Task<OneOf<None, Exception>>    SubscribeAsync( IList<string> topics );
        Task<OneOf<None, Exception>>    UnsubscribeAsync( string topic );
    }

    public class MqttManager : IMqttManager {
        private readonly MqttFactory            mClientFactory;
        private readonly IClientConfiguration   mClientConfiguration;
        private readonly IManagedMqttClient     mClient;
        private readonly Subject<MqttMessage>   mReceivedMessageSubject;
        private readonly Subject<MqttMessage>   mProcessedMessageSubject;

        public  bool                            IsEnabled { get; private set; }
        public  MqttStatus                      Status { get; private set; }

        public  IObservable<MqttMessage>        OnMessageReceived => mReceivedMessageSubject.AsObservable();
        public  IObservable<MqttMessage>        OnMessageProcessed => mProcessedMessageSubject.AsObservable();

        public MqttManager( MqttFactory clientFactory, IClientConfiguration clientConfiguration ) {
            mClientFactory = clientFactory;
            mClientConfiguration = clientConfiguration;

            IsEnabled = false;
            Status = MqttStatus.Uninitialized;

            mClient = mClientFactory.CreateManagedMqttClient();

            mReceivedMessageSubject = new Subject<MqttMessage>();
            mProcessedMessageSubject = new Subject<MqttMessage>();
        }

        public bool IsConnected => 
            mClient is { IsConnected: true };

        public async Task<OneOf<None, Exception>> InitializeAsync() {
            try {
                if( mClientConfiguration.MqttConfiguration.MqttEnabled ) {
                    Status = MqttStatus.Connecting;

                    var mqttClientOptions = new MqttClientOptionsBuilder()
                        .WithClientId( mClientConfiguration.DeviceConfiguration.Identifiers )
                        .WithTcpServer( mClientConfiguration.MqttConfiguration.ServerAddress )
                        .WithCleanSession()
                        .WithKeepAlivePeriod( TimeSpan.FromSeconds( 15 ));

                    if(!String.IsNullOrWhiteSpace( mClientConfiguration.MqttConfiguration.UserName )) {
                        mqttClientOptions
                            .WithCredentials( mClientConfiguration.MqttConfiguration.UserName, 
                                              mClientConfiguration.MqttConfiguration.Password );
                    }

                    if(!String.IsNullOrWhiteSpace( mClientConfiguration.LastWillTopic )) {
                        mqttClientOptions
                            .WithWillTopic( mClientConfiguration.LastWillTopic )
                            .WithWillPayload( mClientConfiguration.LastWillPayload )
                            .WithWillRetain( mClientConfiguration.MqttConfiguration.UseRetainFlag );
                    }

                    var managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
                        .WithClientOptions( mqttClientOptions.Build())
                        .Build();

                    mClient.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync; 
                    mClient.ApplicationMessageProcessedAsync += OnApplicationMessageProcessedAsync;
                    mClient.ConnectedAsync += OnConnectedAsync;
                    mClient.ConnectingFailedAsync += OnConnectingFailedAsync;
                    mClient.DisconnectedAsync += OnDisconnectedAsync;

                    await mClient.StartAsync( managedMqttClientOptions );

                    IsEnabled = true;
                }
                else {
                    IsEnabled = false;
                }

                return new None();
            }
            catch( Exception ex ) {
                Status = MqttStatus.Error;

                return ex;
            }
        }

        private Task OnConnectingFailedAsync( ConnectingFailedEventArgs arg ) {
            Status = MqttStatus.Error;

            return Task.CompletedTask;
        }

        private Task OnConnectedAsync( MqttClientConnectedEventArgs arg ) {
            Status = MqttStatus.Connected;

            return Task.CompletedTask;
        }

        private Task OnDisconnectedAsync( MqttClientDisconnectedEventArgs arg ) {
            Status = MqttStatus.Disconnected;

            return Task.CompletedTask;
        }

        private async Task OnApplicationMessageReceivedAsync( MqttApplicationMessageReceivedEventArgs arg ) {
            mReceivedMessageSubject.OnNext( new MqttMessage( arg.ApplicationMessage ));

            await arg.AcknowledgeAsync( CancellationToken.None );
        }

        private Task OnApplicationMessageProcessedAsync( ApplicationMessageProcessedEventArgs arg ) {
            mProcessedMessageSubject.OnNext( new MqttMessage( arg.ApplicationMessage ));

            return Task.CompletedTask;
        }

        public async Task<OneOf<None, Exception>> PublishAsync( MqttApplicationMessage message ) {
            await mClient.EnqueueAsync( message );

            return new None();
        }

        public async Task<OneOf<None, Exception>> PublishAsync( string topic, string payload ) {
            var messageFactory = mClientFactory.CreateApplicationMessageBuilder();
            var message = messageFactory
                .WithTopic( topic )
                .WithPayload( payload )
                .Build();

            await mClient.EnqueueAsync( message );

            return new None();
        }

        public async Task<OneOf<None, Exception>> SubscribeAsync( string topic ) =>
            await SubscribeAsync( new List<string>{ topic });

        public async Task<OneOf<None, Exception>> SubscribeAsync( IList<string> topics ) {
            var topicFilters = topics.Select( t => new MqttTopicFilter{ Topic = t }).ToList();

            await mClient.SubscribeAsync( topicFilters );

            return new None();
        }

        public async Task<OneOf<None, Exception>> UnsubscribeAsync( string topic ) {
            await mClient.UnsubscribeAsync( topic );

            return new None();
        }

        private async void Disconnect() =>
            await mClient.StopAsync();

        public void Dispose() {
            Disconnect();

            mClient.ApplicationMessageReceivedAsync -= OnApplicationMessageReceivedAsync; 
            mClient.ApplicationMessageProcessedAsync -= OnApplicationMessageProcessedAsync;
            mClient.ConnectedAsync -= OnConnectedAsync;
            mClient.ConnectingFailedAsync -= OnConnectingFailedAsync;
            mClient.DisconnectedAsync -= OnDisconnectedAsync;

            mClient.Dispose();
        }
    }
}
