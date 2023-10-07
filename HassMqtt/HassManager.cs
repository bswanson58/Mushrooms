﻿using HassMqtt.Lights;
using HassMqtt.Mqtt;
using HassMqtt.Platform;

// ReSharper disable IdentifierTypo

namespace HassMqtt {
    public interface IHassManager {
        Task                    InitializeAsync();
        Task                    ShutdownAsync();

        HassMqttParameters      GetHassMqttParameters();
        void                    SetHassMqttParameters( HassMqttParameters parameters );

        ILightsManager          LightsManager { get; }
    }

    public class HassManager : IHassManager {
        private readonly IMqttManager           mMqttManager;
        private readonly IHassMqttManager       mHassManager;
        private readonly IClientConfiguration   mHassConfiguration;

        public  ILightsManager              LightsManager { get; }

        public HassManager( ILightsManager lightsManager, IHassMqttManager hassManager, IMqttManager mqttManager,
                            IClientConfiguration clientConfiguration ) {
            mHassConfiguration = clientConfiguration;
            LightsManager = lightsManager;
            mHassManager = hassManager;
            mMqttManager = mqttManager;
        }

        public async Task InitializeAsync() {
            if( mMqttManager.Status.Equals( MqttStatus.Uninitialized )) {
                await mMqttManager.InitializeAsync();
            }

            // wait while connecting
            while( mMqttManager.Status == MqttStatus.Connecting ) {
                await Task.Delay( 100 );
            } 

            if( mMqttManager.IsConnected ) {
                await mHassManager.InitializeAsync();

                await LightsManager.InitializeAsync();
            }
        }

        public async Task ShutdownAsync() {
            await LightsManager.ShutdownAsync();

            // wait for processing loops to exit.
            await Task.Delay( 800 );

            await mHassManager.ShutdownAsync();
        }

        public HassMqttParameters GetHassMqttParameters() =>
            mHassConfiguration.GetHassMqttParameters();

        public void SetHassMqttParameters( HassMqttParameters parameters ) =>
            mHassConfiguration.SetHassMqttParameters( parameters );
    }
}
