using System;
using Fluxor;
using Mushrooms.Models;
using Mushrooms.SceneBuilder.Store;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.SceneParametersEditor {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class SceneParametersEditorViewModel : PropertyChangeBase {
        private readonly ISceneFacade           mSceneFacade;
        private TimeSpan                        mTransitionDuration;
        private TimeSpan                        mTransitionJitter;
        private TimeSpan                        mDisplayDuration;
        private TimeSpan                        mDisplayJitter;

        public SceneParametersEditorViewModel( IState<SceneState> sceneState, ISceneFacade sceneFacade  ) {
            mSceneFacade = sceneFacade;

            mTransitionDuration = sceneState.Value.Parameters.BaseDisplayTime;
            mTransitionJitter = sceneState.Value.Parameters.DisplayTimeJitter;
            mDisplayDuration = sceneState.Value.Parameters.BaseDisplayTime;
            mDisplayJitter = sceneState.Value.Parameters.DisplayTimeJitter;

            RaiseAllPropertiesChanged();
        }

        public int TransitionDurationSeconds {
            get => (int)mTransitionDuration.TotalSeconds;
            set {
                mTransitionDuration = TimeSpan.FromSeconds( value );

                UpdateSceneState();
            }
        }

        public int TransitionDurationJitterSeconds {
            get => (int)mTransitionJitter.TotalSeconds;
            set {
                mTransitionJitter = TimeSpan.FromSeconds( value );

                UpdateSceneState();
            }
        }

        public int DisplayDurationSeconds {
            get => (int)mDisplayDuration.TotalSeconds;
            set {
                mDisplayDuration = TimeSpan.FromSeconds( value );

                UpdateSceneState();
            }
        }

        public int DisplayDurationJitterSeconds {
            get => (int)mDisplayJitter.TotalSeconds;
            set {
                mDisplayJitter = TimeSpan.FromSeconds( value );

                UpdateSceneState();
            }
        }

        private void UpdateSceneState() =>
            mSceneFacade.SetSceneParameters( 
                new SceneParameters( mTransitionDuration, mTransitionJitter, mDisplayDuration, mDisplayJitter ));
    }
}
