using HassMqtt.Lights;
using Mushrooms.Models;
using Mushrooms.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using HassMqtt.Discovery;
using HassMqtt.Models;
using HassMqtt.Support;

namespace Mushrooms.HassIntegration {
    internal class HaSceneLight : LightBase, IDisposable {
        private const string cDefaultName = "Mushroom Scene";

        private readonly IMushroomGarden    mGarden;
        private readonly ActiveScene        mScene;
        private readonly IDisposable        mSceneSubscription;

        public HaSceneLight( ActiveScene scene, IMushroomGarden garden ) : 
            base( string.IsNullOrWhiteSpace( scene.Scene.SceneName ) ? cDefaultName : scene.Scene.SceneName, 3, scene.Scene.Id ) {
            mScene = scene;
            mGarden = garden;

            mState = scene.IsActive;

            mSceneSubscription = mScene.OnSceneChanged.Subscribe( OnSceneChanged );
        }

        protected override BaseDiscoveryModel ? CreateDiscoveryModel() {
            var discoveryModel = base.CreateDiscoveryModel() as LightDiscoveryModel;
            var context = ContextProvider?.Context;

            if(( discoveryModel != null ) &&
               ( context != null )) {
                discoveryModel.RgbStateTopic = $"{context.DeviceBaseTopic( Domain )}/{ObjectId}/color/{Constants.Status}";
            }

            return discoveryModel;
        }

        public override IList<DeviceTopicState> GetStatesToPublish() {
            var statesList = base.GetStatesToPublish();

            if( GetDiscoveryModel() is LightDiscoveryModel discoveryModel ) {
                if(!String.IsNullOrWhiteSpace( discoveryModel.RgbStateTopic )) {
                    statesList.Add( new DeviceTopicState( discoveryModel.RgbStateTopic, GetRgbColor()));
                }
            }

            return statesList;
        }

        private string GetRgbColor() {
            var retValue = "200,200,200";
            var bulb = mScene.GetActiveBulbs().FirstOrDefault();

            if( bulb != null ) {
                retValue = $"{bulb.ActiveColor.R},{bulb.ActiveColor.G},{bulb.ActiveColor.B}";
            }

            return retValue;
        }

        public override string GetCombinedState() =>
            $"{base.GetCombinedState()}|{GetRgbColor()}";

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
