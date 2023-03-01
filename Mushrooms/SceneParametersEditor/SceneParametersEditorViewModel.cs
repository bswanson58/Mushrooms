using System;
using Fluxor;
using Mushrooms.Database;
using Mushrooms.Models;
using Mushrooms.SceneBuilder.Store;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.SceneParametersEditor {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class SceneParametersEditorViewModel : PropertyChangeBase {
        private readonly ISceneFacade           mSceneFacade;
        private readonly IState<SceneState>     mSceneState;
        private readonly IPlanProvider          mPlanProvider;
        private TimeSpan                        mTransitionDuration;
        private TimeSpan                        mTransitionJitter;
        private TimeSpan                        mDisplayDuration;
        private TimeSpan                        mDisplayJitter;
        private string                          mPlanName;

        public  DelegateCommand                 SavePlan { get; }

        public SceneParametersEditorViewModel( IState<SceneState> sceneState, ISceneFacade sceneFacade, 
                                               IPlanProvider planProvider  ) {
            mSceneFacade = sceneFacade;
            mSceneState = sceneState;
            mPlanProvider = planProvider;

            mTransitionDuration = mSceneState.Value.Parameters.BaseDisplayTime;
            mTransitionJitter = mSceneState.Value.Parameters.DisplayTimeJitter;
            mDisplayDuration = mSceneState.Value.Parameters.BaseDisplayTime;
            mDisplayJitter = mSceneState.Value.Parameters.DisplayTimeJitter;

            SavePlan = new DelegateCommand( OnSavePlan, CanSavePlan );

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

        public string PlanName {
            get => mPlanName;
            set {
                mPlanName = value;

                SavePlan.RaiseCanExecuteChanged();
            }
        }

        private void OnSavePlan() {
            var plan = new ScenePlan {
                Parameters = mSceneState.Value.Parameters,
                Palette = mSceneState.Value.Palette,
                PlanName = mPlanName
            };

            mPlanProvider.Insert( plan );
        }

        private bool CanSavePlan() =>
            !String.IsNullOrWhiteSpace( mPlanName );

        private void UpdateSceneState() =>
            mSceneFacade.SetSceneParameters( 
                new SceneParameters( mTransitionDuration, mTransitionJitter, mDisplayDuration, mDisplayJitter ));
    }
}
