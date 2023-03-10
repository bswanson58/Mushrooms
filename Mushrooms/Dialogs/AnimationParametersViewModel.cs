using System;
using Mushrooms.Entities;
using ReusableBits.Wpf.DialogService;

namespace Mushrooms.Dialogs {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class AnimationParametersViewModel : DialogAwareBase {
        public  const string    cSceneParameters = "scene parameters";
        private TimeSpan        mTransitionDuration;
        private TimeSpan        mTransitionJitter;
        private TimeSpan        mDisplayDuration;
        private TimeSpan        mDisplayJitter;
        private bool            mEnableAnimation;
        private bool            mSynchronizeLights;

        public  string          DisplayDuration => mDisplayDuration.ToString();
        public  string          DisplayJitter => mDisplayJitter.ToString();
        public  string          TransitionDuration => mTransitionDuration.ToString();
        public  string          TransitionJitter => mTransitionJitter.ToString();

        public  bool            IsAnimationEnabled => mEnableAnimation;

        public AnimationParametersViewModel() {
            var defaults = SceneParameters.Default;

            mTransitionDuration = defaults.BaseTransitionTime;
            mTransitionJitter = defaults.TransitionJitter;
            mDisplayDuration = defaults.BaseDisplayTime;
            mDisplayJitter = defaults.DisplayTimeJitter;
            mEnableAnimation = defaults.AnimationEnabled;
            mSynchronizeLights = defaults.SynchronizeLights;

            Title = "Scene Animation Parameters";
        }

        public override void OnDialogOpened( IDialogParameters parameters ) {
            var sceneParameters = parameters.GetValue<SceneParameters>( cSceneParameters );

            if( sceneParameters != null ) {
                mTransitionDuration = sceneParameters.BaseTransitionTime;
                mTransitionJitter = sceneParameters.TransitionJitter;
                mDisplayDuration = sceneParameters.BaseDisplayTime;
                mDisplayJitter = sceneParameters.DisplayTimeJitter;
                mEnableAnimation = sceneParameters.AnimationEnabled;
                mSynchronizeLights = sceneParameters.SynchronizeLights;

                RaiseAllPropertiesChanged();
            }
        }

        public int TransitionDurationSeconds {
            get => (int)mTransitionDuration.TotalSeconds;
            set { 
                mTransitionDuration = TimeSpan.FromSeconds( value );

                RaisePropertyChanged( () => TransitionDuration );
            }
        }

        public int TransitionDurationJitterSeconds {
            get => (int)mTransitionJitter.TotalSeconds;
            set { 
                mTransitionJitter = TimeSpan.FromSeconds( value );

                RaisePropertyChanged( () => TransitionJitter );
            }
        }

        public int DisplayDurationSeconds {
            get => (int)mDisplayDuration.TotalSeconds;
            set { 
                mDisplayDuration = TimeSpan.FromSeconds( value );

                RaisePropertyChanged( () => DisplayDuration );
            }
        }

        public int DisplayDurationJitterSeconds {
            get => (int)mDisplayJitter.TotalSeconds;
            set { 
                mDisplayJitter = TimeSpan.FromSeconds( value );

                RaisePropertyChanged( () => DisplayJitter );
            }
        }

        public bool EnableAnimation {
            get => mEnableAnimation;
            set {
                mEnableAnimation = value;

                RaisePropertyChanged( () => EnableAnimation );
                RaisePropertyChanged( () => IsAnimationEnabled );
            }
        }

        public bool SynchronizeLights {
            get => mSynchronizeLights;
            set {
                mSynchronizeLights = value;

                RaisePropertyChanged( () => SynchronizeLights );
            }
        } 


        protected override DialogParameters CreateClosingParameters() =>
            new () {
                { cSceneParameters,
                    new SceneParameters( mTransitionDuration, mTransitionJitter, mDisplayDuration, mDisplayJitter,
                                         mEnableAnimation, mSynchronizeLights ) }
            };
    }
}
