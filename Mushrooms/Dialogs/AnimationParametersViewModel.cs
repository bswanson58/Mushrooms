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
        private double          mBrightnessVariation;
        private bool            mEnableAnimation;
        private bool            mSynchronizeLights;

        public  string          DisplayDuration => $"{mDisplayDuration:mm\\:ss\\.ff}";
        public  string          DisplayJitter => $"{mDisplayJitter:mm\\:ss\\.ff}";
        public  string          TransitionDuration => $"{mTransitionDuration:mm\\:ss\\.ff}";
        public  string          TransitionJitter => $"{mTransitionJitter:mm\\:ss\\.ff}";
        public  string          BrightnessVariationPercent => $"{(int)(mBrightnessVariation * 100.0D )}%";

        public  bool            IsAnimationEnabled => mEnableAnimation;

        public AnimationParametersViewModel() {
            var defaults = SceneParameters.Default;

            mTransitionDuration = defaults.BaseTransitionTime;
            mTransitionJitter = defaults.TransitionJitter;
            mDisplayDuration = defaults.BaseDisplayTime;
            mDisplayJitter = defaults.DisplayTimeJitter;
            mBrightnessVariation = defaults.BrightnessVariation;
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
                mBrightnessVariation = sceneParameters.BrightnessVariation;
                mEnableAnimation = sceneParameters.AnimationEnabled;
                mSynchronizeLights = sceneParameters.SynchronizeLights;

                RaiseAllPropertiesChanged();
            }
        }

        public int TransitionDurationMilliseconds {
            get => (int)mTransitionDuration.TotalMilliseconds;
            set { 
                mTransitionDuration = TimeSpan.FromMilliseconds( value );

                RaisePropertyChanged( () => TransitionDuration );
            }
        }

        public int TransitionDurationJitterMilliseconds {
            get => (int)mTransitionJitter.TotalMilliseconds;
            set { 
                mTransitionJitter = TimeSpan.FromMilliseconds( value );

                RaisePropertyChanged( () => TransitionJitter );
            }
        }

        public int DisplayDurationMilliseconds {
            get => (int)mDisplayDuration.TotalMilliseconds;
            set { 
                mDisplayDuration = TimeSpan.FromMilliseconds( value );

                RaisePropertyChanged( () => DisplayDuration );
            }
        }

        public int DisplayDurationJitterMilliseconds {
            get => (int)mDisplayJitter.TotalMilliseconds;
            set { 
                mDisplayJitter = TimeSpan.FromMilliseconds( value );

                RaisePropertyChanged( () => DisplayJitter );
            }
        }

        public double BrightnessVariation {
            get => mBrightnessVariation * 100.0D;
            set {
                mBrightnessVariation = Math.Max( 0.0D, Math.Min( 0.5D, value / 100.0D ));

                RaisePropertyChanged( () => BrightnessVariationPercent );
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
                                         mEnableAnimation, mSynchronizeLights, mBrightnessVariation ) }
            };
    }
}
