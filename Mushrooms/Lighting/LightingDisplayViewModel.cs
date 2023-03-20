using System.Collections.ObjectModel;
using System.Linq;
using HueLighting.Hub;
using Mushrooms.Entities;
using Mushrooms.Models;
using ReusableBits.Wpf.Platform;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.Lighting {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class LightingDisplayViewModel : PropertyChangeBase {
        private readonly ILightingGroupManager  mLighting;

        public  ObservableCollection<LightSourceViewModel>  Bulbs { get; }
        public  ObservableCollection<LightSourceViewModel>  Groups { get; }

        public LightingDisplayViewModel( ILightingGroupManager lighting ) {
            mLighting = lighting;

            Bulbs = new ObservableCollection<LightSourceViewModel>();
            Groups = new ObservableCollection<LightSourceViewModel>();

            LoadLighting();
        }

        private async void LoadLighting() {
            var bulbs = await mLighting.GetBulbs();

            Bulbs.Clear();
            Bulbs.AddRange( 
                    bulbs
                        .OrderBy( b => b.Name )
                        .Select( b => new LightSourceViewModel( 
                            new LightSource( b.Name, LightSourceType.Bulb ), new []{ b }, _ => { } )));

            var groups = await mLighting.GetBulbGroups();

            Groups.Clear();
            Groups.AddRange( 
                    groups
                        .OrderBy( g => g.Name )
                        .Select( g => new LightSourceViewModel( 
                            new LightSource( g.Name, g.GroupType ), g.Bulbs, _ => { })));
        }
    }
}
