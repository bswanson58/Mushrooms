using HassMqtt.Lights;
using Mushrooms.Models;
using Mushrooms.Services;
using System;

namespace Mushrooms.HassIntegration {
    internal class HaSceneLight : LightBase, IDisposable {
        private const string cDefaultName = "Mushroom Scene";

        private readonly IMushroomGarden    mGarden;
        private readonly ActiveScene        mScene;
        private readonly IDisposable        mSceneSubscription;

        public HaSceneLight( ActiveScene scene, IMushroomGarden garden ) : 
            base( string.IsNullOrWhiteSpace( scene.Scene.SceneName ) ? cDefaultName : scene.Scene.SceneName, 5, scene.Scene.Id ) {
            mScene = scene;
            mGarden = garden;

            mState = scene.IsActive;

            mSceneSubscription = mScene.OnSceneChanged.Subscribe( OnSceneChanged );
        }

        private void OnSceneChanged( ActiveScene scene ) {
            mState = scene.IsActive;
        }

        protected override void OnCommand( string payload ) {
            base.OnCommand( payload );

            if( mState ) {
                mGarden.StartScene( mScene.Scene );
            }
            else {
                mGarden.StopScene( mScene.Scene );
            }
        }

        public void Dispose() {
            mSceneSubscription.Dispose();
        }
    }
}
