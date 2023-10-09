using System.Text.Json;
using HassMqtt.Lights;
using HassMqtt.Mqtt;
using HassMqtt.Support;
using ReusableBits.Platform.Interfaces;
using MQTTnet;
using HassMqtt.Discovery;
using HassMqtt.Context;

// ReSharper disable IdentifierTypo

namespace HassMqtt {
    public interface IMqttMessageHandler {
        bool    ProcessMessage( MqttMessage message );
    }

    public interface IHassMqttManager : IDisposable {
        void    RegisterMessageHandler( IMqttMessageHandler handler );
        void    RevokeMessageHandler( IMqttMessageHandler handler );

        Task    PublishAutoDiscoveryConfigAsync( AbstractDiscoverable discoverable );
        Task    RevokeAutoDiscoveryConfigAsync( AbstractDiscoverable discoverable );

        Task    PublishStateAsync( AbstractDiscoverable device, bool respectChecks = true );
    }

    public class HassMqttManager : IHassMqttManager {
        private readonly IMqttManager               mMqttManager;
        private readonly IHassContextProvider       mContextProvider;
        private readonly IBasicLog                  mLog;
        private readonly List<IMqttMessageHandler>  mMessageHandlers;
        private DateTime                            mLastAvailableAnnouncementFailedLogged;
        private IDisposable ?                       mMessageSubscription;
        private IDisposable ?                       mStatusSubscription;
        private CancellationTokenSource ?           mTokenSource;
        private Task ?                              mProcessTask;

        public HassMqttManager( IMqttManager mqttManager, IHassContextProvider contextProvider, IBasicLog log ) {
            mMqttManager = mqttManager;
            mContextProvider = contextProvider;
            mLog = log;

            mMessageHandlers = new List<IMqttMessageHandler>();
            mLastAvailableAnnouncementFailedLogged = DateTime.MinValue;

            mMessageSubscription = mMqttManager.OnMessageReceived.Subscribe( OnMessageReceived );
            mStatusSubscription = mMqttManager.OnStatusChanged.Subscribe( OnMqttStatusChanged );
        }

        private async void OnMqttStatusChanged( MqttStatus status ) {
            if( status.Equals( MqttStatus.Connected )) {
                await StartProcessing();
            }
            else {
                await StopProcessing();
            }
        }

        private async Task StartProcessing() {
            await StopProcessing();

            mTokenSource = new CancellationTokenSource();
            mProcessTask = Task.Run(() => Process( mTokenSource.Token ), mTokenSource.Token );
        }

        private async Task StopProcessing() {
            mTokenSource?.Cancel();

            if( mProcessTask != null ) {
                await mProcessTask;

                mProcessTask?.Dispose();
                mProcessTask = null;
            }

            mTokenSource?.Dispose();
            mTokenSource = null;
        }

        public void RegisterMessageHandler( IMqttMessageHandler handler ) {
            RevokeMessageHandler( handler );

            mMessageHandlers.Add( handler );
        }

        public void RevokeMessageHandler( IMqttMessageHandler handler ) {
            if( mMessageHandlers.Contains( handler )) {
                mMessageHandlers.Remove( handler );
            }
        }

        private void OnMessageReceived( MqttMessage message ) {
            if( message.Topic.EndsWith( Constants.Subscribe )) {
                foreach( var handler in mMessageHandlers ) {
                    if( handler.ProcessMessage( message )) {
                        break;
                    }
                }
            }
        }

        public async Task ShutdownAsync() {
            mTokenSource?.Cancel();

            mMessageSubscription?.Dispose();
            mMessageSubscription = null;

            if( mProcessTask != null ) {
                await mProcessTask;

                mProcessTask?.Dispose();
            }

            await AnnounceAvailabilityAsync( true );
        }

        private async Task Process( CancellationToken cancelToken ) {
            var subscriptionRequested = false;

            while(!cancelToken.IsCancellationRequested ) {
                try {
                    if(( cancelToken.IsCancellationRequested ) ||
                       ( mMqttManager.Status != MqttStatus.Connected )) {
                        continue;
                    }

                    if(!subscriptionRequested ) {
                        await mMqttManager.SubscribeAsync( mContextProvider.Context.DeviceMessageSubscriptionTopic());

                        subscriptionRequested = true;
                    }

                    await AnnounceAvailabilityAsync();

                    await Task.Delay( TimeSpan.FromSeconds( 10 ), cancelToken );
                }
                catch( Exception ex ) {
                    mLog.LogException( "Error while announcing availability.", ex );
                }
            }

            if( subscriptionRequested ) {
                await mMqttManager.UnsubscribeAsync( mContextProvider.Context.DeviceMessageSubscriptionTopic());
            }
        }

        private async Task AnnounceAvailabilityAsync( bool offline = false ) {
            if(!mContextProvider.Context.MqttEnabled ) {
                return;
            }

            try {
                if( mMqttManager.IsConnected ) {
                    var topic = mContextProvider.Context.DeviceAvailabilityTopic();
                    var messageBuilder = new MqttApplicationMessageBuilder()
                        .WithTopic( topic )
                        .WithPayload( offline ? Constants.Offline : Constants.Online )
                        .WithRetainFlag( mContextProvider.Context.UseMqttRetainFlag );

                    await mMqttManager.PublishAsync( messageBuilder.Build());
                }
                else {
                    // only log failures once every 5 minutes to minimize log growth
                    if(( DateTime.Now - mLastAvailableAnnouncementFailedLogged ).TotalMinutes < 5 ) {
                        return;
                    }

                    mLastAvailableAnnouncementFailedLogged = DateTime.Now;

                    mLog.LogMessage( "MQTT is not connected, availability announcement was not published" );
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "Error while announcing availability", ex );
            }
        }

        public async Task PublishAutoDiscoveryConfigAsync( AbstractDiscoverable discoverable ) {
            await AnnounceAutoDiscoveryConfigAsync( discoverable, discoverable.Domain );
        }

        public async Task RevokeAutoDiscoveryConfigAsync( AbstractDiscoverable discoverable ) {
            await AnnounceAutoDiscoveryConfigAsync( discoverable, discoverable.Domain, true );
        }

        private async Task AnnounceAutoDiscoveryConfigAsync( AbstractDiscoverable discoverable,
                                                             string domain, bool clearConfig = false ) {
            if((!mContextProvider.Context.MqttEnabled ) ||
               ( mMqttManager is not { IsConnected: true })) {
                return;
            }

            try {
                var topic =
                    $"{mContextProvider.Context.DeviceBaseTopic( domain )}/{discoverable.ObjectId}/{Constants.Configuration}";

                var messageBuilder = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithRetainFlag( mContextProvider.Context.UseMqttRetainFlag );

                if( clearConfig ) {
                    messageBuilder.WithPayload( Array.Empty<byte>() );
                }
                else {
                    var discoveryConfig = discoverable.GetDiscoveryModel();

                    if( discoveryConfig != null ) {
                        messageBuilder.WithPayload(
                            JsonSerializer.Serialize( discoveryConfig, discoveryConfig.GetType(), JsonPolicy.JsonSerializerOptions ));
                    }
                }

                // Publish discovery configuration
                await mMqttManager.PublishAsync( messageBuilder.Build());
            }
            catch( Exception ex ) {
                mLog.LogException( "Error while announcing auto discovery", ex );
            }
        }

        public async Task PublishStateAsync( AbstractDiscoverable device, bool respectChecks = true ) {
            try {
                // are we asked to check elapsed time?
                if( respectChecks ) {
                    if( device.LastUpdated.AddSeconds( device.UpdateIntervalSeconds ) > DateTime.Now ) {
                        return;
                    }
                }

                // get the current state/attributes
                var combinedState = device.GetCombinedState();
                var attributes = device.GetAttributes();

                // are we asked to check state changes?
                if( respectChecks ) {
                    if(( device.PreviousPublishedState == combinedState ) &&
                       ( device.PreviousPublishedAttributes == attributes )) {
                        return;
                    }
                }

                // fetch the auto discovery config
                if( device.GetDiscoveryModel() is not LightDiscoveryModel autoDiscoConfig ) {
                    return;
                }

                foreach( var state in device.GetStatesToPublish()) {
                    var message = new MqttApplicationMessageBuilder()
                        .WithTopic( state.Topic )
                        .WithPayload( state.State )
                        .Build();

                    var published = await mMqttManager.PublishAsync( message );
                    if( published.IsT1 ) {
                        return;
                    }
                }

                // optionally prepare and send attributes
                if( device.UseAttributes ) {
                    var message = new MqttApplicationMessageBuilder()
                        .WithTopic( autoDiscoConfig.JsonAttributesTopic )
                        .WithPayload( attributes )
                        .Build();

                    var published = await mMqttManager.PublishAsync( message );
                    if( published.IsT1 ) {
                        // failed, don't store the state
                        return;
                    }
                }

                // only store the values if the checks are respected
                // otherwise, we might stay in 'unknown' state until the value changes
                if(!respectChecks ) {
                    return;
                }

                device.UpdatePublishedState( combinedState, attributes );
            }
            catch( Exception ex ) {
                mLog.LogException( $"Sensor '{device.Name}' - Error publishing state", ex );
            }
        }

        public async void Dispose() {
            mMessageSubscription?.Dispose();
            mMessageSubscription = null;

            mStatusSubscription?.Dispose();
            mStatusSubscription = null;

            await StopProcessing();
        }
    }
}
