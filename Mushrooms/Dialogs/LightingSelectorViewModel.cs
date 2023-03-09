using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mushrooms.Entities;
using Mushrooms.Models;
using ReusableBits.Wpf.DialogService;

namespace Mushrooms.Dialogs {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class LightingSelectorViewModel : DialogAwareBase {
        public  const string                cLightingList = "lighting list";
        public  const string                cSelectedLights = "selected light group";

        public  ObservableCollection<LightSourceViewModel>  Rooms { get; }
        public  ObservableCollection<LightSourceViewModel>  Zones { get; }
        public  ObservableCollection<LightSourceViewModel>  Bulbs { get; }

        public LightingSelectorViewModel() {
            Rooms = new ObservableCollection<LightSourceViewModel>();
            Zones = new ObservableCollection<LightSourceViewModel>();
            Bulbs = new ObservableCollection<LightSourceViewModel>();

            Title = "Scene Lighting";
        }

        public override void OnDialogOpened( IDialogParameters parameters ) {
            var lightingList = parameters.GetValue<IList<LightSourceViewModel>>( cLightingList );
            var selectedLights = parameters.GetValue<IList<LightSource>>( cSelectedLights );

            if( lightingList != null ) {
                Rooms.Clear();
                Zones.Clear();
                Bulbs.Clear();

                foreach( var light in lightingList.OrderBy( l => l.Name )) {
                    switch( light.LightSource.SourceType ) {
                        case LightSourceType.Room:
                            Rooms.Add( new LightSourceViewModel( light.LightSource, light.Bulbs, OnLightingSelected ));
                            break;

                        case LightSourceType.Entertainment:
                        case LightSourceType.Zone:
                            Zones.Add( new LightSourceViewModel( light.LightSource, light.Bulbs, OnLightingSelected ));
                            break;

                        case LightSourceType.Bulb:
                            Bulbs.Add( new LightSourceViewModel( light.LightSource, light.Bulbs, OnLightingSelected ));
                            break;
                    }
                }
            }

            if( selectedLights != null ) {
                foreach( var light in Rooms ) {
                    light.IsSelected = 
                        selectedLights.Any( l => l.SourceType.Equals( light.LightSource.SourceType ) && 
                                                 l.SourceName.Equals( light.LightSource.SourceName ));
                }
                foreach( var light in Zones ) {
                    light.IsSelected = 
                        selectedLights.Any( l => l.SourceType.Equals( light.LightSource.SourceType ) && 
                                                 l.SourceName.Equals( light.LightSource.SourceName ));
                }
                foreach( var light in Bulbs ) {
                    light.IsSelected = 
                        selectedLights.Any( l => l.SourceType.Equals( light.LightSource.SourceType ) && 
                                                 l.SourceName.Equals( light.LightSource.SourceName ));
                }
            }
        }

        private void OnLightingSelected( LightSourceViewModel _ ) {
            Ok.RaiseCanExecuteChanged();
        }

        private IList<LightSource> CollectSelectedLighting() {
            var selectedRooms = Rooms.Where( r => r.IsSelected ).Select( r => r.LightSource );
            var selectedZones = Zones.Where( z => z.IsSelected ).Select( z => z.LightSource );
            var selectedBulbs = Bulbs.Where( b => b.IsSelected ).Select( b => b.LightSource );

            return selectedRooms.Concat( selectedZones ).Concat( selectedBulbs ).ToList();
        }

        protected override DialogParameters CreateClosingParameters() =>
            new() { { cSelectedLights, CollectSelectedLighting() } };

        protected override bool CanAccept() =>
            CollectSelectedLighting().Any();
    }
}
