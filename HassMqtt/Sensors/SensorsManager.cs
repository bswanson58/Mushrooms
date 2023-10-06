using HassMqtt.Context;
using HassMqtt.Models;
using HassMqtt.Mqtt;
using ReusableBits.Platform.Interfaces;

namespace HassMqtt.Sensors {
    public interface ISensorsManager {
        Task InitializeAsync();

        void Stop();
        void Pause();
        void Resume();
        Task RevokeAllSensors();

        void AddSensor( AbstractSingleValueSensor sensor );
    }

    /// <summary>
    /// Continuously performs sensor auto discovery and state publishing
    /// </summary>
    public class SensorsManager : ISensorsManager {
        private readonly List<AbstractSingleValueSensor>        mSingleValueSensors;
        private readonly List<AbstractMultiValueSensor>         mMultiValueSensors;
        private readonly IMqttManager       mMqttManager;
        private readonly IHassContext       mHassContext;
        private readonly IHassMqttManager   mHassManager;
        private readonly IStoredSensors     mStoredSensors;
        private readonly IBasicLog          mLog;
        private DateTime                    mLastAutoDiscoPublish;
        private bool                        mActive;
        private bool                        mPause;

        public SensorsManager( IMqttManager mqttManager, IBasicLog log, IStoredSensors storedSensors, IHassContext hassContext,
                               IHassMqttManager hassManager ) {
            mMqttManager = mqttManager;
            mHassContext = hassContext;
            mLog = log;
            mStoredSensors = storedSensors;
            mHassManager = hassManager;

            mActive = true;
            mPause = false;
            mLastAutoDiscoPublish = DateTime.MinValue;

            mSingleValueSensors = new List<AbstractSingleValueSensor>();
            mMultiValueSensors = new List<AbstractMultiValueSensor>();
        }

        public async Task InitializeAsync() {
            if(!mMqttManager.IsEnabled ) {
                return;
            }

            // wait while connecting
            while( mMqttManager.Status == MqttStatus.Connecting ) {
                await Task.Delay( 250 );
            } 

            // start background processing
            _ = Task.Run( Process );
        }

        public void Stop() => 
            mActive = false;

        public void Pause() => 
            mPause = true;

        public void Resume() => 
            mPause = false;

        public async Task RevokeAllSensors() {
            foreach( var sensor in mSingleValueSensors ) {
                await mHassManager.RevokeAutoDiscoveryConfigAsync( sensor );
            }
            foreach( var sensor in mMultiValueSensors ) {
                await mHassManager.RevokeAutoDiscoveryConfigAsync( sensor );
            }
        }

        public void AddSensor( AbstractSingleValueSensor sensor ) {
            sensor.InitializeParameters( mHassContext );

            mSingleValueSensors.Add( sensor );
        }

        /// <summary>
        /// Continuously processes sensors (auto discovery, states)
        /// </summary>
        private async void Process() {
            // we use the first run flag to publish our state without respecting the time elapsed/value change check
            // otherwise, the state might stay in 'unknown' in HA until the value changes
            var firstRun = true;
            var firstRunDone = false;

            while( mActive ) {
                try {
                    await Task.Delay( TimeSpan.FromMilliseconds( 750 ));

                    if(( mPause ) ||
                       (!mActive ) ||
                       ( mMqttManager.Status != MqttStatus.Connected ) ) {
                        continue;
                    }

                    // optionally flag as the first real run
                    if(!firstRunDone ) {
                        firstRunDone = true;
                    }

                    // publish availability & sensor discovery every 30 sec
                    if(( DateTime.Now - mLastAutoDiscoPublish ).TotalSeconds > 30 ) {
                        // publish the auto discovery
                        foreach( var sensor in mSingleValueSensors ) {
                            await mHassManager.PublishAutoDiscoveryConfigAsync( sensor );
                        }

                        foreach( var sensor in mMultiValueSensors ) {
                            await mHassManager.PublishAutoDiscoveryConfigAsync( sensor );
                        }

                        mLastAutoDiscoPublish = DateTime.Now;
                    }

                    if( mPause ) {
                        continue;
                    }

                    // publish sensor states (they have their own time-based scheduling)
                    foreach( var sensor in mSingleValueSensors ) {
                        await mHassManager.PublishStateAsync( sensor, !firstRun );
                    }

                    foreach( var sensor in mMultiValueSensors ) {
                        await mHassManager.PublishStateAsync( sensor, !firstRun );
                    }
                }
                catch( Exception ex ) {
                    mLog.LogException( "Error while publishing sensor.", ex );
                }
                finally {
                    // check if we need to take down the 'first run' flag
                    if( firstRunDone && firstRun ) {
                        firstRun = false;
                    }
                }
            }
        }

        /// <summary>
        /// Resets all sensor checks (last sent and previous value), so they'll all be published again
        /// </summary>
        internal void ResetAllSensorChecks() {
            try {
                // pause processing
                Pause();

                foreach( var sensor in mSingleValueSensors ) {
                    sensor.ResetChecks();
                }

                foreach( var sensor in mMultiValueSensors ) {
                    sensor.ResetChecks();
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "Error while resetting all sensor checks", ex );
            }
            finally {
                Resume();
            }
        }

        /// <summary>
        /// Stores the provided sensors, and (re)publishes them
        /// </summary>
        /// <param name="sensors"></param>
        /// <param name="toBeDeletedSensors"></param>
        /// <returns></returns>
        internal async Task<bool> StoreAsync( List<ConfiguredSensor> sensors, 
                                              List<ConfiguredSensor> ? toBeDeletedSensors = null ) { 
            toBeDeletedSensors ??= new List<ConfiguredSensor>();

            try {
                Pause();

                // process the to-be-removed
                foreach( var sensor in toBeDeletedSensors ) {
                    if( sensor.IsSingleValue()) {
                        var abstractSensor = mStoredSensors.ConvertConfiguredToAbstractSingleValue( sensor );

                        // remove and revoke
                        await mHassManager.RevokeAutoDiscoveryConfigAsync( abstractSensor );
                        mSingleValueSensors.RemoveAt( mSingleValueSensors.FindIndex( x => x.Id == abstractSensor.Id ));

                        mLog.LogMessage( $"Removed single-value sensor '{abstractSensor.Name}'." );
                    }
                    else {
                        var abstractSensor = mStoredSensors.ConvertConfiguredToAbstractMultiValue( sensor );

                        // remove and revoke
                        await mHassManager.RevokeAutoDiscoveryConfigAsync( abstractSensor );
                        mMultiValueSensors.RemoveAt( mMultiValueSensors.FindIndex( x => x.Id == abstractSensor.Id ));

                        mLog.LogMessage( $"Removed multi-value sensor: '{abstractSensor.Name}'." );
                    }
                }

                // copy our list to the main one
                foreach( var sensor in sensors ) {
                    if( sensor.IsSingleValue()) {
                        var abstractSensor = mStoredSensors.ConvertConfiguredToAbstractSingleValue( sensor );

                        if( mSingleValueSensors.All( x => x.Id != abstractSensor.Id )) {
                            // new, add and register
                            mSingleValueSensors.Add( abstractSensor );

                            await mHassManager.PublishAutoDiscoveryConfigAsync( abstractSensor );
                            await mHassManager.PublishStateAsync( abstractSensor, false );

                            mLog.LogMessage( $"Added single-value sensor '{abstractSensor.Name}'." );
                            continue;
                        }

                        // existing, update and re-register
                        var currentSensorIndex = mSingleValueSensors.FindIndex( x => x.Id == abstractSensor.Id );

                        if( mSingleValueSensors[currentSensorIndex].Name != abstractSensor.Name ) {
                            // name changed, revoke
                            mLog.LogMessage( $"Single-value sensor changed name, re-registering as new entity: {mSingleValueSensors[currentSensorIndex].Name} to {abstractSensor.Name}" );

                            await mHassManager.RevokeAutoDiscoveryConfigAsync( mSingleValueSensors[currentSensorIndex]);
                        }

                        mSingleValueSensors[currentSensorIndex] = abstractSensor;

                        await mHassManager.PublishAutoDiscoveryConfigAsync( abstractSensor );
                        await mHassManager.PublishStateAsync( abstractSensor, false );

                        mLog.LogMessage( $"Modified single-value sensor '{abstractSensor.Name}'." );
                    }
                    else {
                        var abstractSensor = mStoredSensors.ConvertConfiguredToAbstractMultiValue( sensor );

                        if( mMultiValueSensors.All( x => x.Id != abstractSensor.Id )) {
                            // new, add and register
                            mMultiValueSensors.Add( abstractSensor );

                            await mHassManager.PublishAutoDiscoveryConfigAsync( abstractSensor );
                            await mHassManager.PublishStateAsync( abstractSensor, false );

                            mLog.LogMessage( $"Added multi-value sensor: '{abstractSensor.Name}'." );
                            continue;
                        }

                        // existing, update and revoke
                        var currentSensorIndex = mMultiValueSensors.FindIndex( x => x.Id == abstractSensor.Id );

                        if( mMultiValueSensors[currentSensorIndex].Name != abstractSensor.Name ) {
                            // name changed, revoke
                            mLog.LogMessage( $"Multi-value sensor changed name, re-registering as new entity: {mMultiValueSensors[currentSensorIndex].Name} to {abstractSensor.Name}" );

                            await mHassManager.RevokeAutoDiscoveryConfigAsync( mMultiValueSensors[currentSensorIndex]);
                        }

                        mMultiValueSensors[currentSensorIndex] = abstractSensor;

                        await mHassManager.PublishAutoDiscoveryConfigAsync( abstractSensor );
                        await mHassManager.PublishStateAsync( abstractSensor, false );

                        mLog.LogMessage( $"Modified multi-value sensor: '{abstractSensor.Name}'." );
                    }
                }

                // store to file
                await mStoredSensors.Store();

                return true;
            }
            catch( Exception ex ) {
                mLog.LogException( "Error while storing sensors.", ex );

                return false;
            }
            finally {
                Resume();
            }
        }
    }
}
