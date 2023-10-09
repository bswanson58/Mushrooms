using HassMqtt.Context;
using HassMqtt.Hass;
using HassMqtt.Lights;
using HassMqtt.Mqtt;
using HassMqtt.Sensors;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;

// ReSharper disable IdentifierTypo

namespace HassMqtt.Support {
    public static class ServicesExtensions {
        public static void AddHassIntegration( this IServiceCollection services ) {
            services.AddScoped<MqttFactory>();

            services.AddSingleton<IMqttManager, MqttManager>();
            services.AddSingleton<IHassContextProvider, HassContextProvider>();
            services.AddSingleton<IHassMqttManager, HassMqttManager>();
            services.AddSingleton<IHassManager, HassManager>();

            services.AddSingleton<ILightsManager, LightsManager>();
            services.AddSingleton<ISensorsManager, SensorsManager>();
        }
    }
}
