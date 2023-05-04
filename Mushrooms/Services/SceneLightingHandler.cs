using HueLighting.Hub;
using Mushrooms.Models;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using HueLighting.Models;
using Mushrooms.Entities;
using Mushrooms.Support;

namespace Mushrooms.Services {
    internal interface ISceneLightingHandler {
        IList<ActiveBulb>           GetBulbUpdateList( ActiveScene forScene );
        Task<IEnumerable<Bulb>>     GetSceneBulbs( Scene forScene );
        ActiveBulb                  UpdateBulb( ActiveBulb bulb, Scene inScene, SceneControl control );
        IList<ActiveBulb>           UpdateBulbs( IList<ActiveBulb> bulb, Scene inScene, SceneControl control );
        IList<ActiveBulb>           UpdateBulbs( IList<ActiveBulb> bulbs, Color color, SceneControl control );

        Task                        ActivateScene( ActiveScene scene );
        Task                        DeactivateScene( ActiveScene scene );
        Task                        UpdateSceneBrightness( ActiveScene scene );
    }

    internal class SceneLightingHandler : ISceneLightingHandler {
        private readonly IHubManager            mHubManager;
        private readonly LimitedRepeatingRandom mLimitedRandom;
//        private readonly IBasicLog              mLog;
        private readonly Random                 mRandom;

        public SceneLightingHandler( IHubManager hubManager ) {  //}, IBasicLog log ) {
            mHubManager = hubManager;
//            mLog = log;
            mLimitedRandom = new LimitedRepeatingRandom();
            mRandom = Random.Shared;
        }

        public async Task ActivateScene( ActiveScene scene ) {
            foreach( var bulb in scene.GetSceneBulbs()) {
                await mHubManager.SetBulbState( bulb, true );
            }
        }

        public async Task UpdateSceneBrightness( ActiveScene scene ) {
            foreach ( var bulb in scene.GetSceneBulbs()) {
                await mHubManager.SetBulbState( bulb, scene.Control.Brightness );
            }
        }

        public async Task DeactivateScene( ActiveScene scene ) {
            foreach( var bulb in scene.GetSceneBulbs()) {
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
            var activeBulbs = forScene.GetActiveBulbs();

            foreach( var bulb in forScene.GetSceneBulbs()) {
                activity.Add( activeBulbs.FirstOrDefault( b => b.Bulb.Id.Equals( bulb.Id ), new ActiveBulb( bulb )));
            }

            var now = DateTime.Now;

            return activity.Where( b => b.NextUpdateTime < now ).ToList();
        }

        public ActiveBulb UpdateBulb( ActiveBulb bulb, Scene inScene, SceneControl control ) =>
            UpdateBulbs( new []{ bulb }, inScene, control ).First();

        public IList<ActiveBulb> UpdateBulbs( IList<ActiveBulb> bulbs, Scene inScene, SceneControl control ) {
            var color = inScene.Palette.Palette[ mLimitedRandom.Next( inScene.Palette.Palette.Count )];
            var transitionJitter = TimeSpan.FromMilliseconds( mRandom.Next((int)inScene.Parameters.TransitionJitter.TotalMilliseconds ));
            var transitionTime = inScene.Parameters.BaseTransitionTime + transitionJitter;
            var displayJitter = TimeSpan.FromMilliseconds( mRandom.Next((int)inScene.Parameters.DisplayTimeJitter.TotalMilliseconds ));
            var displayTime = inScene.Parameters.BaseDisplayTime + displayJitter;
            var nextUpdateTime = DateTime.Now + transitionTime + displayTime;
            var brightness = control.Brightness;

            if( inScene.Parameters.BrightnessVariation > 0.01D ) {
                var brightnessVariation = mRandom.Next((int)( inScene.Parameters.BrightnessVariation * 100 ));
                var brightnessPercent = 1.0 - ( brightnessVariation / 100.0D );

                brightness = Math.Max( 0.01D, Math.Min( 1.0, control.Brightness * brightnessPercent ));
            }

            mHubManager.SetBulbState( bulbs.Select( b => b.Bulb ), color, brightness, transitionTime );
/*
            var firstBulb = bulbs.FirstOrDefault();
            var updateTime = DateTime.MinValue;
            if( firstBulb != null ) {
                updateTime = firstBulb.NextUpdateTime;
            }
            mLog.LogMessage( 
                $"Bulb updated: {String.Join( ',', bulbs.Select( b => b.Bulb.Name ))}, update time: {updateTime}, transition time: {transitionTime}, display time: {displayTime}, next update after: {nextUpdateTime}");
*/
            return bulbs.Select( b => new ActiveBulb( b.Bulb, color, nextUpdateTime )).ToList();
        }

        public IList<ActiveBulb> UpdateBulbs( IList<ActiveBulb> bulbs, Color color, SceneControl control ) {
            var transitionTime = TimeSpan.FromMilliseconds( 300 );
            var nextUpdateTime = DateTime.Now + transitionTime;

            mHubManager.SetBulbState( bulbs.Select( b => b.Bulb ), color, control.Brightness, transitionTime );

            return bulbs.Select( b => new ActiveBulb( b.Bulb, color, nextUpdateTime )).ToList();
        }
    }
}
