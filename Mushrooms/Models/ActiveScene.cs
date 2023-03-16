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
        private readonly IList<Bulb>            mSceneBulbs;
        private readonly IList<ActiveBulb>      mActiveBulbs;
        private readonly IList<LightSource>     mOriginalLights;
        private readonly Object                 mSceneLock;

        public  Scene                           Scene { get; private set; }
        public  SceneState                      SceneState { get; private set; }
        public  bool                            IsActive { get; private set; }
        public  SceneControl                    Control { get; private set; }

        public  IObservable<ActiveScene>        OnSceneChanged => mChangeSubject.AsObservable();

        public ActiveScene( Scene scene ) {
            Scene = scene;
            mOriginalLights = new List<LightSource>( scene.Lights );
            mSceneBulbs = new List<Bulb>();
            mActiveBulbs = new List<ActiveBulb>();
            Control = new SceneControl( Scene.Control.Brightness, Scene.Control.RateMultiplier );
            SceneState = SceneState.Inactive;
            IsActive = false;
            mSceneLock = new ();

            mChangeSubject = new Subject<ActiveScene>();
        }

        public IList<Bulb> GetSceneBulbs() {
            var retValue = new List<Bulb>();

            lock ( mSceneLock ) {
                retValue.AddRange( mSceneBulbs );    
            }

            return retValue;
        }

        public IList<ActiveBulb> GetActiveBulbs() {
            var retValue = new List<ActiveBulb>();

            lock ( mSceneLock ) {
                retValue.AddRange( mActiveBulbs );    
            }

            return retValue;
        }

        public IList<LightSource> GetOriginalLights() {
            var retValue = new List<LightSource>();

            lock ( mSceneLock ) {
                retValue.AddRange( mOriginalLights );    
            }

            return retValue;
        }

        public void SetActiveState( SceneState state ) {
            if( state == SceneState.Active ) {
                Activate();
            }

            if(state == SceneState.Scheduled ) {
                ActiveBySchedule();
            }
        }

        private void Activate() {
            IsActive = true;
            SceneState = SceneState.Active;

            mChangeSubject.OnNext( this );
        }

        private void ActiveBySchedule() {
            IsActive = true;
            SceneState = SceneState.Scheduled;

            mChangeSubject.OnNext( this );
        }

        public void Deactivate() {
            IsActive = false;
            SceneState = SceneState.Inactive;

            ClearActiveBulbs();

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
            lock( mSceneLock ) {
                mSceneBulbs.Clear();
                mSceneBulbs.AddRange( bulbs );
            }
        }

        public void Update( IList<LightSource> originalLights ) {
            lock( mSceneLock ) {
                mOriginalLights.Clear();
                mOriginalLights.AddRange( originalLights );
            }
        }

        public void ClearActiveBulbs() {
            lock( mSceneLock ) {
                mActiveBulbs.Clear();
            }
        }

        public void Update( ActiveBulb bulb ) {
            lock( mSceneLock ) {
                var existing = mActiveBulbs.FirstOrDefault( b => b.Bulb.Id.Equals( bulb.Bulb.Id ));

                if( existing != null ) {
                    mActiveBulbs.Remove( existing );
                }

                mActiveBulbs.Add( bulb );

                mChangeSubject.OnNext( this );
            }
        }
    }
}
