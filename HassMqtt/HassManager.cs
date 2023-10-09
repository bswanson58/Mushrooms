using HassMqtt.Context;
using HassMqtt.Hass;
using HassMqtt.Lights;

// ReSharper disable IdentifierTypo

namespace HassMqtt {
    public interface IHassManager {
        HassMqttParameters      GetHassMqttParameters();
        void                    SetHassMqttParameters( HassMqttParameters parameters );

        ILightsManager          LightsManager { get; }
    }

    public class HassManager : IHassManager {
        private readonly IHassContextProvider   mContextProvider;

        public  ILightsManager                  LightsManager { get; }

        public HassManager( ILightsManager lightsManager, IHassContextProvider contextProvider ) {
            LightsManager = lightsManager;
            mContextProvider = contextProvider;
        }

        public HassMqttParameters GetHassMqttParameters() =>
            mContextProvider.GetHassMqttParameters();

        public void SetHassMqttParameters( HassMqttParameters parameters ) =>
            mContextProvider.SetHassMqttParameters( parameters );
    }
}
