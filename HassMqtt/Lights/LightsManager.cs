using HassMqtt.Context;
using HassMqtt.Mqtt;
using ReusableBits.Platform.Interfaces;

// ReSharper disable IdentifierTypo

namespace HassMqtt.Lights {
    public interface ILightsManager : IDisposable {
        Task    AddLight( LightBase light );
        Task    RemoveLight( string lightId );

        Task    RevokeAllLights();
        Task    RevokeLight( LightBase light );
    }

    public class LightsManager : ILightsManager, IMqttMessageHandler {
        private readonly List<LightBase>        mLights;
        private readonly IMqttManager           mMqttManager;
        private readonly IHassContextProvider   mContextProvider;
        private readonly IHassMqttManager       mHassManager;
        private readonly IBasicLog              mLog;
        private IDisposable ?                   mStatusSubscription;
        private DateTime                        mLastAutoDiscoPublish;
        private CancellationTokenSource ?       mTokenSource;
        private Task ?                          mProcessTask;
        private bool                            mPause;

        public LightsManager( IMqttManager mqttManager, IBasicLog log, IHassContextProvider contextProvider, 
                              IHassMqttManager hassManager ) {
            mMqttManager = mqttManager;
            mContextProvider = contextProvider;
            mLog = log;
            mHassManager = hassManager;

            mPause = false;
            mLastAutoDiscoPublish = DateTime.MinValue;

            mLights = new List<LightBase>();

            mHassManager.RegisterMessageHandler( this );
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

        private void Pause() =>
            mPause = true;

        private void Resume() =>
            mPause = false;

        public async Task RevokeAllLights() {
            foreach( var light in GetLights()) {
                await RevokeLight( light );
            }
        }

        public async Task RevokeLight( LightBase light ) =>
            await mHassManager.RevokeAutoDiscoveryConfigAsync( light );

        public async Task AddLight( LightBase light ) {
            light.InitializeParameters( mContextProvider );
        
            // publish the initial discovery configuration
            await mHassManager.PublishAutoDiscoveryConfigAsync( light );

            lock( mLights ) {
                mLights.Add( light );
            }
        }

        public async Task RemoveLight( string lightId ) {
            var light = GetLights().FirstOrDefault( l => l.Id.Equals( lightId ));

            if( light != null ) {
                await RevokeLight( light );

                lock( mLights ) {
                    mLights.Remove( light );
                }
            }
        }

        /// <summary>
        /// Continuously processes sensors (auto discovery, states)
        /// </summary>
        private async void Process( CancellationToken cancellationToken ) {
            var initialPublish = true;

            while(!cancellationToken.IsCancellationRequested ) {
                try {
                    await Task.Delay( TimeSpan.FromMilliseconds( 750 ), cancellationToken );

                    if(( mPause ) ||
                       ( cancellationToken.IsCancellationRequested ) ||
                       ( mMqttManager.Status != MqttStatus.Connected )) {
                        continue;
                    }

                    var lights = GetLights();

                    // publish sensor discovery every 30 sec
                    if(( DateTime.Now - mLastAutoDiscoPublish ).TotalSeconds > 30 ) {
                        foreach( var light in lights ) {
                            await mHassManager.PublishAutoDiscoveryConfigAsync( light );
                        }

                        mLastAutoDiscoPublish = DateTime.Now;
                    }

                    // publish sensor states (they have their own time-based scheduling)
                    foreach( var device in lights ) {
                        await mHassManager.PublishStateAsync( device, !initialPublish );
                    }

                    initialPublish = false;
                }
                catch( Exception ex ) {
                    mLog.LogException( "Error while publishing sensor.", ex );
                }
            }
        }

        public bool ProcessMessage( MqttMessage message ) {
            var retValue = false;

            foreach( var device in GetLights()) {
                if( device.ProcessMessage( message.Topic, message.Payload )) {
                    retValue = true;

                    break;
                }
            }

            return retValue;
        }

        private IList<LightBase> GetLights() {
            var retValue = new List<LightBase>();

            lock( mLights ) {
                retValue.AddRange( mLights );
            }

            return retValue;
        }

        /// <summary>
        /// Resets all sensor checks (last sent and previous value), so they'll all be published again
        /// </summary>
        internal void ResetAllSensorChecks() {
            try {
                // pause processing
                Pause();

                foreach( var light in GetLights()) {
                    light.ResetChecks();
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "Error while resetting all sensor checks", ex );
            }
            finally {
                Resume();
            }
        }

        public async void Dispose() {
            mStatusSubscription?.Dispose();
            mStatusSubscription = null;

            await StopProcessing();
        }
    }
}
