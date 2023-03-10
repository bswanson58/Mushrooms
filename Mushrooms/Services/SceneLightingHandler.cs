using HueLighting.Hub;
using Mushrooms.Models;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using HueLighting.Models;
using Mushrooms.Entities;
using Mushrooms.Support;

namespace Mushrooms.Services {
    internal interface ISceneLightingHandler {
        IList<ActiveBulb>           GetBulbUpdateList( ActiveScene forScene );
        Task<IEnumerable<Bulb>>     GetSceneBulbs( Scene forScene );
        ActiveBulb                  UpdateBulb( ActiveBulb bulb, Scene inScene, SceneControl control );
        IList<ActiveBulb>           UpdateBulbs( IList<ActiveBulb> bulb, Scene inScene, SceneControl control );

        Task                        ActivateScene( ActiveScene scene );
        Task                        DeactivateScene( ActiveScene scene );
        Task                        UpdateSceneBrightness( ActiveScene scene );
    }

    internal class SceneLightingHandler : ISceneLightingHandler {
        private readonly IHubManager            mHubManager;
        private readonly LimitedRepeatingRandom mLimitedRandom;
        private readonly Random                 mRandom;

        public SceneLightingHandler( IHubManager hubManager ) {
            mHubManager = hubManager;
            mLimitedRandom = new LimitedRepeatingRandom();
            mRandom = Random.Shared;
        }

        public async Task ActivateScene( ActiveScene scene ) {
            foreach( var bulb in scene.SceneBulbs ) {
                await mHubManager.SetBulbState( bulb, true );
            }
        }

        public async Task UpdateSceneBrightness( ActiveScene scene ) {
            foreach ( var bulb in scene.SceneBulbs ) {
                await mHubManager.SetBulbState( bulb, scene.Control.Brightness );
            }
        }

        public async Task DeactivateScene( ActiveScene scene ) {
            foreach( var bulb in scene.SceneBulbs ) {
                await mHubManager.SetBulbState( bulb, false );
            }
        }

        public async Task<IEnumerable<Bulb>> GetSceneBulbs( Scene forScene ) {
            var retValue = new List<Bulb>();
            var groups = ( await mHubManager.GetBulbGroups()).ToList();
            var bulbs = ( await mHubManager.GetBulbs()).ToList();

            foreach( var lightSource in forScene.Lights ) {
                if( lightSource.SourceType.Equals( LightSourceType.Bulb )) {
                    var bulb = bulbs.FirstOrDefault( b => b.Name.Equals( lightSource.SourceName ));

                    if( bulb != null ) {
                        retValue.Add( bulb );
                    }
                }
                else {
                    var group = groups.FirstOrDefault( g => g.Name.Equals( lightSource.SourceName ));

                    if( group != null ) {
                        retValue.AddRange( group.Bulbs );
                    }
                }
            }

            return retValue                    
                .GroupBy( b => b.Id )
                .Select( g => g.First());
        }

        public IList<ActiveBulb> GetBulbUpdateList( ActiveScene forScene ) {
            var activity = new List<ActiveBulb>();

            foreach( var bulb in forScene.SceneBulbs ) {
                activity.Add( forScene.ActiveBulbs.FirstOrDefault( b => b.Bulb.Id.Equals( bulb.Id ), new ActiveBulb( bulb )));
            }

            var now = DateTime.Now;

            return activity.Where( b => b.NextUpdateTime < now ).ToList();
        }

        public ActiveBulb UpdateBulb( ActiveBulb bulb, Scene inScene, SceneControl control ) =>
            UpdateBulbs( new []{ bulb }, inScene, control ).First();

        public IList<ActiveBulb> UpdateBulbs( IList<ActiveBulb> bulbs, Scene inScene, SceneControl control ) {
            var color = inScene.Palette.Palette[ mLimitedRandom.Next( inScene.Palette.Palette.Count )];
            var transitionJitter = TimeSpan.FromSeconds( mRandom.Next((int)inScene.Parameters.TransitionJitter.TotalSeconds ));
            var transitionTime = inScene.Parameters.BaseTransitionTime + transitionJitter;
            var displayJitter = TimeSpan.FromSeconds( mRandom.Next((int)inScene.Parameters.DisplayTimeJitter.TotalSeconds ));
            var displayTime = inScene.Parameters.BaseDisplayTime + displayJitter;
            var nextUpdateTime = DateTime.Now + transitionTime + displayTime;

            mHubManager.SetBulbState( bulbs.Select( b => b.Bulb ), color, control.Brightness, transitionTime );

            return bulbs.Select( b => new ActiveBulb( b.Bulb, color, nextUpdateTime )).ToList();
        }
    }
}
