using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HueLighting.Hub;
using HueLighting.Models;
using Mushrooms.Dialogs;
using Mushrooms.Entities;
using Mushrooms.Models;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.Platform;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.Lighting {
    public class LightingViewModel : PropertyChangeBase {
        public  BulbGroup       BulbGroup { get; }

        public  string          GroupName => BulbGroup.Name;

        public LightingViewModel( BulbGroup group ) {
            BulbGroup = group;
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class LightingDisplayViewModel : PropertyChangeBase {
        private readonly ILightingGroupManager  mLighting;
        private readonly IDialogService         mDialogService;

        public  ObservableCollection<LightSourceViewModel>  Bulbs { get; }
        public  ObservableCollection<LightingViewModel>     Groups { get; }

        public  ICommand                                    CreateGroup { get; }
        public  DelegateCommand<LightingViewModel>          EditGroup { get; }
        public  DelegateCommand<LightingViewModel>          DeleteGroup { get; }

        public LightingDisplayViewModel( ILightingGroupManager lighting, IDialogService dialogService ) {
            mLighting = lighting;
            mDialogService = dialogService;

            Bulbs = new ObservableCollection<LightSourceViewModel>();
            Groups = new ObservableCollection<LightingViewModel>();

            CreateGroup = new DelegateCommand( OnCreateLightingGroup );
            EditGroup = new DelegateCommand<LightingViewModel>( OnEditLightingGroup );
            DeleteGroup = new DelegateCommand<LightingViewModel>( OnDeleteLightingGroup );

            LoadLighting();
        }

        private async void LoadLighting() {
            var bulbs = await mLighting.GetBulbs();

            Bulbs.Clear();
            Bulbs.AddRange( 
                    bulbs
                        .OrderBy( b => b.Name )
                        .Select( b => new LightSourceViewModel( 
                            new LightSource( b.Name, LightSourceType.Bulb ), new []{ b }, _ => { } )));

            var groups = await mLighting.GetBulbGroups();

            Groups.Clear();
            Groups.AddRange(
                groups
                    .OrderBy( g => g.Name )
                    .Select( g => new LightingViewModel( g )));
        }

        private void OnCreateLightingGroup() {
            mDialogService.ShowDialog<EditGroupView>( result => {
                if( result.Result.Equals( ButtonResult.Ok )) {
                    var group = result.Parameters.GetValue<BulbGroup>( EditGroupViewModel.cGroup );

                    if( group != null ) {
                        CreateLightingGroup( group );
                    }
                }
            });
        }

        private async void CreateLightingGroup( BulbGroup group ) {
            await mLighting.CreateGroup( group.Name, new List<Bulb>(), group.GroupType, group.RoomClass );
        }

        private void OnEditLightingGroup( LightingViewModel ? lightGroup ) {
            if( lightGroup != null ) {
                var parameters = new DialogParameters{{ EditGroupViewModel.cGroup, lightGroup.BulbGroup }};

                mDialogService.ShowDialog<EditGroupView>( parameters, result => {
                    if( result.Result.Equals( ButtonResult.Ok )) {
                        var group = result.Parameters.GetValue<BulbGroup>( EditGroupViewModel.cGroup );

                        if( group != null ) {
                            UpdateLightingGroup( group );
                        }
                    }
                });
            }
        }

        private async void UpdateLightingGroup( BulbGroup group ) {
            await mLighting.UpdateGroup( group );
        }

        private void OnDeleteLightingGroup( LightingViewModel ? lightGroup ) {
            if( lightGroup != null ) {
                var parameters = new DialogParameters {
                    { ConfirmationDialogViewModel.cTitle, "Delete Light Group" },
                    { ConfirmationDialogViewModel.cMessage, $"Would you like to delete light group '{lightGroup.GroupName}'?" }
                };

                mDialogService.ShowDialog<ConfirmationDialog>( parameters, result => {
                    if( result.Result.Equals( ButtonResult.Ok )) {
                        DeleteLightingGroup( lightGroup.BulbGroup );
                    }
                });
            }
        }

        private async void DeleteLightingGroup( BulbGroup group ) {
            await mLighting.DeleteGroup( group );
        }
    }
}
