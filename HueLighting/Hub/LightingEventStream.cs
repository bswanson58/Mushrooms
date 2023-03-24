using HueLighting.Models;
using ReusableBits.Platform.Interfaces;
using ReusableBits.Platform.Preferences;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HueApi;
using HueApi.Models;
using HueApi.Models.Responses;

// Uses Hue V2 API, not yet complete. Not used due to incomplete HueApi functionality.

namespace HueLighting.Hub {
    public interface ILightingEventStream {
        Task    OpenEventStream( Action<LightingEvent> streamHandler );
        void    CloseEventStream();
    }

    public class LightingEvent {
        public  IReadOnlyList<HueLight>     Lights { get; }
        public  Light ?                     LightData { get; }
        public  IReadOnlyList<Object>       What { get; }

        public LightingEvent( IEnumerable<HueLight> lights, Light ? light, IEnumerable<Object> what ) {
            Lights = new List<HueLight>( lights );
            LightData = light;
            What = new List<object>( what );
        }
    }

    public class LightingEventStream : ILightingEventStream {
        private readonly ILightingCommand       mLighting;
        private readonly IPreferences           mPreferences;
        private readonly IBasicLog              mLog;
        private readonly List<HueLight>         mLights;
        private readonly List<HueGroup>         mGroups;
        private readonly List<HueGroupedLight>  mLightGroups;
        private Action<LightingEvent>           mStreamHandler;
        private LocalHueApi ?                   mClient;

        public LightingEventStream( ILightingCommand lighting, IPreferences preferences, IBasicLog log ) {
            mLighting = lighting;
            mPreferences = preferences;
            mLog = log;

            mLights = new List<HueLight>();
            mGroups = new List<HueGroup>();
            mLightGroups = new List<HueGroupedLight>();

            mStreamHandler = _ => { };
        }

        private LocalHueApi HueClient() {
            if( mClient == null ) {
                var installationInfo = mPreferences.Load<HueConfiguration>();

                if(!String.IsNullOrWhiteSpace( installationInfo.BridgeIp )) {
                    mClient = new LocalHueApi( installationInfo.BridgeIp, installationInfo.BridgeAppKey );
                }
            }

            if( mClient == null ) {
                throw new ApplicationException( "LocalHueApi client could not be created from HueConfiguration" );
            }

            return mClient!;
        }

        public async Task OpenEventStream( Action<LightingEvent> streamHandler ) {
            mStreamHandler = streamHandler;

            try {
                mLights.Clear();
                mLightGroups.Clear();
                mGroups.Clear();

                mLights.AddRange( await mLighting.GetLights());
                mLightGroups.AddRange( await mLighting.GetGroupedLights());
                mGroups.AddRange( await mLighting.GetZones());
                mGroups.AddRange( await mLighting.GetRooms());

                HueClient().OnEventStreamMessage += OnEventStreamMessage;
                HueClient().StartEventStream();
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( OpenEventStream ), ex );
            }
        }

        public void CloseEventStream() {
            HueClient().StopEventStream();
        }

        private async void OnEventStreamMessage( List<EventStreamResponse> events ) {
            foreach( var e in events ) {
                foreach( var eventData in e.Data ) {
                    mStreamHandler?.Invoke( await CreateLightingEvent( eventData ));
                }
            }
        }

        private async Task<LightingEvent> CreateLightingEvent( EventStreamData data ) {
            var lights = new List<HueLight>();

            if( data.Type.Equals( HueTypes.GroupedLight )) {
                var lightGroup = mLightGroups.FirstOrDefault( g => g.Id.Equals( data.Id ));

                if( lightGroup != null ) {
                    var owningGroup = mGroups.FirstOrDefault( g => g.Id.Equals( lightGroup.Owner ));

                    if( owningGroup != null ) {
                        lights.AddRange( owningGroup.Lights );
                    }
                }
            }

            if( data.Type.Equals( HueTypes.Light )) {
                var light = mLights.FirstOrDefault( l => l.Id.Equals( data.Id ));

                if( light != null ) {
                    lights.Add( light );
                }
            }

            Light ? lightData = default;

            if( lights.Any()) {
                var light = lights.FirstOrDefault();

                if( light != null ) {
                    var response = await HueClient().GetLightAsync( light.Id );

                    lightData = response.Data.FirstOrDefault( l => l.Id.Equals( light.Id ));
                }
            }

            return new LightingEvent( lights, lightData, CreateExtensionData( data.ExtensionData ));
        }

        private IEnumerable<Object> CreateExtensionData( Dictionary<string, JsonElement> elements ) {
            var retValue = new List<Object ?>();

            foreach( var element in elements ) {
                switch ( element.Key ) {
                    case "on":
                        retValue.Add( element.Value.Deserialize<On>());
                        break;

                    case "brightness":
                        retValue.Add( element.Value.Deserialize<Dimming>());
                        break;

                    case "color":
                        retValue.Add( element.Value.Deserialize<Color>());
                        break;

                    case "color_temperature":
                        retValue.Add( element.Value.Deserialize<ColorTemperature>());
                        break;
                }
            }

            return retValue.Where( o => o != null )!;
        }
    }
}
