using HueLighting.Models;
using Mushrooms.Entities;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;

namespace Mushrooms.Models {
    internal enum SceneState {
        Active,
        Scheduled,
        Inactive
    }

    internal class ActiveScene {
        private readonly Subject<ActiveScene>   mChangeSubject;

        public  Scene                           Scene { get; private set; }
        public  IList<LightSource>              OriginalLights { get; private set; }
        public  IList<Bulb>                     SceneBulbs { get; }
        public  IList<ActiveBulb>               ActiveBulbs { get; }

        public  SceneState                      SceneState { get; private set; }
        public  bool                            IsActive { get; private set; }
        public  SceneControl                    Control { get; private set; }

        public  IObservable<ActiveScene>        OnSceneChanged => mChangeSubject.AsObservable();

        public ActiveScene( Scene scene ) {
            Scene = scene;
            OriginalLights = new List<LightSource>( scene.Lights );
            SceneBulbs = new List<Bulb>();
            ActiveBulbs = new List<ActiveBulb>();
            Control = new SceneControl( Scene.Control.Brightness, Scene.Control.RateMultiplier );
            SceneState = SceneState.Inactive;
            IsActive = false;

            mChangeSubject = new Subject<ActiveScene>();
        }

        public void Activate() {
            IsActive = true;
            SceneState = SceneState.Active;

            mChangeSubject.OnNext( this );
        }

        public void ActiveBySchedule() {
            IsActive = true;
            SceneState = SceneState.Scheduled;

            mChangeSubject.OnNext( this );
        }

        public void Deactivate() {
            IsActive = false;
            SceneState = SceneState.Inactive;

            mChangeSubject.OnNext( this );
        }

        public void Update( SceneControl control ) {
            Control = control;
            Scene.Update( control );

            mChangeSubject.OnNext( this );
        }

        public void Update( Scene scene ) {
            Scene = scene;

            mChangeSubject.OnNext( this );
        }

        public void Update( IEnumerable<Bulb> bulbs ) {
            SceneBulbs.Clear();
            SceneBulbs.AddRange( bulbs );
        }

        public void Update( IList<LightSource> originalLights ) {
            OriginalLights = originalLights;
        }

        public void ClearActiveBulbs() {
            ActiveBulbs.Clear();
        }

        public void Update( ActiveBulb bulb ) {
            var existing = ActiveBulbs.FirstOrDefault( b => b.Bulb.Id.Equals( bulb.Bulb.Id ));

            if( existing != null ) {
                ActiveBulbs.Remove( existing );
            }

            ActiveBulbs.Add( bulb );

            mChangeSubject.OnNext( this );
        }
    }
}
