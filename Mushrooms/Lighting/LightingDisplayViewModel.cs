using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HueApi.ColorConverters.Original.Extensions;
using HueApi.Models;
using HueLighting.Hub;
using ReusableBits.Wpf.ViewModelSupport;
using Color = HueApi.Models.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using ColorTemperature = HueApi.Models.ColorTemperature;

namespace Mushrooms.Lighting {
    public class LightData : PropertyChangeBase {
        public  HueLight                    Light { get; }
        public  System.Windows.Media.Color  Color { get; set; }
        public  bool                        IsOn { get; set; }
        public  double                      Brightness { get; set; }

        public LightData( HueLight light ) {
            Light = light;
        }

        public void UpdateProperties() => RaiseAllPropertiesChanged();
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class LightingDisplayViewModel : PropertyChangeBase, IDisposable {
        private readonly ILightingEventStream   mLightingStream;

        public  ObservableCollection<LightData>             LightData { get; }

        public LightingDisplayViewModel( ILightingEventStream lightingEventStream ) {
            mLightingStream = lightingEventStream;

            LightData = new ObservableCollection<LightData>();

            mLightingStream.OpenEventStream( OnLightingEvent );
        }

        private void OnLightingEvent( LightingEvent eventData ) {
            var lights = eventData.Lights;
            var lightData = new List<LightData>();

            foreach( var light in lights ) {
                var l = LightData.FirstOrDefault( l => l.Light.Id.Equals( light.Id ));

                if( l == null ) {
                    l = new LightData( light );

                    LightData.Add( l );
                }

                lightData.Add( l );
            }

            foreach( var what in eventData.What ) { 
                switch ( what ) {
                    case On on:
                        lightData.ForEach( l => l.IsOn = on.IsOn );
                        break;
                    case Color:
                        var rgbColor = eventData.LightData?.ToRGBColor();

                        if( rgbColor != null ) {
                            var hexColor = $"#{rgbColor.Value.ToHex()}";
                            var mediaColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString( hexColor );
                            lightData.ForEach( l => l.Color = mediaColor );
                        }
                        break;
                    case ColorTemperature:
                        break;
                    case Dimming:
                        break;
                }
            }

            lightData.ForEach( l => l.UpdateProperties());
        }

        public void Dispose() {
            mLightingStream.CloseEventStream();
        }
    }
}
