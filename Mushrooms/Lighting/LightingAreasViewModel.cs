using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HueLighting.Hub;
using HueLighting.Models;
using Mushrooms.Dialogs;
using Mushrooms.Entities;
using Mushrooms.Models;
using Q42.HueApi.Models.Groups;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.Platform;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.Lighting {
    public class LightGroupViewModel : PropertyChangeBase {
        public  BulbGroup       Group { get; }

        public  string          GroupName => Group.Name;
        public  string          LightList => String.Join( ", ", Group.Bulbs.OrderBy( b => b.Name ).Select( b => b.Name ));

        public LightGroupViewModel( BulbGroup group ) {
            Group = group;
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class LightingAreasViewModel : PropertyChangeBase {
        private readonly IHubManager            mHubManager;
        private readonly ILightingGroupManager  mGroupManager;
        private readonly IDialogService         mDialogService;

        public  ObservableCollection<LightSourceViewModel>  Bulbs { get; }
        public  ObservableCollection<LightGroupViewModel>   Zones { get; }
        public  ObservableCollection<LightGroupViewModel>   Rooms { get; }

        public  ICommand                                    CreateZone { get; }
        public  ICommand                                    CreateRoom { get; }
        public  DelegateCommand<LightGroupViewModel>        EditGroup { get; }
        public  DelegateCommand<LightGroupViewModel>        DeleteGroup { get; }

        public LightingAreasViewModel( IHubManager hubManager, IDialogService dialogService,
                                       ILightingGroupManager groupManager ) {
            mHubManager = hubManager;
            mGroupManager = groupManager;
            mDialogService = dialogService;

            Bulbs = new ObservableCollection<LightSourceViewModel>();
            Zones = new ObservableCollection<LightGroupViewModel>();
            Rooms = new ObservableCollection<LightGroupViewModel>();

            CreateZone = new DelegateCommand( OnCreateZone );
            CreateRoom = new DelegateCommand( OnCreateRoom );
            EditGroup = new DelegateCommand<LightGroupViewModel>( OnEditGroup );
            DeleteGroup = new DelegateCommand<LightGroupViewModel>( OnDeleteGroup );

            LoadLighting();
        }

        private async void LoadLighting() {
            var bulbs = await mHubManager.GetBulbs();

            Bulbs.Clear();
            Bulbs.AddRange( 
                    bulbs
                        .OrderBy( b => b.Name )
                        .Select( b => new LightSourceViewModel( 
                            new LightSource( b.Id, b.Name, LightSourceType.Bulb ), new []{ b }, _ => { } )));

            var zones = await mHubManager.GetZones();

            Zones.Clear();
            Zones.AddRange(
                zones
                    .OrderBy( g => g.Name )
                    .Select( g => new LightGroupViewModel( g )));

            var rooms = await mHubManager.GetRooms();

            Rooms.Clear();
            Rooms.AddRange(
                rooms
                    .OrderBy( g => g.Name )
                    .Select( g => new LightGroupViewModel( g )));
        }

        private void OnCreateZone() {
            var parameters = new DialogParameters {
                { EditGroupViewModel.cGroupType, GroupType.Zone },
                { EditGroupViewModel.cLights, Bulbs.Select( b => new Bulb( b.LightSource.Id, b.Name, true )) }
            };

            mDialogService.ShowDialog<EditGroupView>( parameters, result => {
                if( result.Result.Equals( ButtonResult.Ok )) {
                    var group = result.Parameters.GetValue<BulbGroup>( EditGroupViewModel.cGroup );

                    if( group != null ) {
                        CreateLightingZone( group );
                    }
                }
            });
        }

        private async void CreateLightingZone( BulbGroup group ) {
            await mGroupManager.CreateZone( group.Name, group.Bulbs, group.RoomClass );
            
            LoadLighting();
        }

        private void OnCreateRoom() {
            var lightsInRooms = Rooms
                .SelectMany( r => r.Group.Bulbs )
                .ToList();
            var lights = Bulbs
                .Where( b => !lightsInRooms.Any( r => r.Id.Equals( b.LightSource.Id )))
                .SelectMany( b => b.Bulbs )
                .ToList();

            var parameters = new DialogParameters {
                { EditGroupViewModel.cGroupType, GroupType.Room },
                { EditGroupViewModel.cLights, lights }
            };

            mDialogService.ShowDialog<EditGroupView>( parameters, result => {
                if( result.Result.Equals( ButtonResult.Ok )) {
                    var group = result.Parameters.GetValue<BulbGroup>( EditGroupViewModel.cGroup );

                    if( group != null ) {
                        CreateLightingRoom( group );
                    }
                }
            });
        }

        private async void CreateLightingRoom( BulbGroup group ) {
            await mGroupManager.CreateRoom( group.Name, group.Bulbs, group.RoomClass );
            
            LoadLighting();
        }

        private void OnEditGroup( LightGroupViewModel ? lightGroup ) {
            if( lightGroup != null ) {
                if( lightGroup.Group.GroupType.Equals( GroupType.Zone )) {
                    OnEditZone( lightGroup );
                }
                else {
                    OnEditRoom( lightGroup );
                }
            }
        }

        private void OnEditZone( LightGroupViewModel lightGroup ) {
            var parameters = new DialogParameters {
                { EditGroupViewModel.cGroup, lightGroup.Group },
                { EditGroupViewModel.cLights, Bulbs.Select( b => new Bulb( b.LightSource.Id, b.Name, true )) }
            };

            OnEditGroup( parameters );
        }

        private void OnEditRoom( LightGroupViewModel lightGroup ) {
            var lightsInRooms = Rooms.SelectMany( r => r.Group.Bulbs );
            var lights = Bulbs
                .Where( b => !lightsInRooms.Any( r => r.Name.Equals( b.Name )))
                .SelectMany( b => b.Bulbs )
                .Concat( lightGroup.Group.Bulbs )
                .OrderBy( b => b.Name )
                .ToList();

            var parameters = new DialogParameters {
                { EditGroupViewModel.cGroup, lightGroup.Group },
                { EditGroupViewModel.cLights, lights }
            };

            OnEditGroup( parameters );
        }

        private void OnEditGroup( DialogParameters parameters ) {
            mDialogService.ShowDialog<EditGroupView>( parameters, result => {
                if( result.Result.Equals( ButtonResult.Ok )) {
                    var group = result.Parameters.GetValue<BulbGroup>( EditGroupViewModel.cGroup );

                    if( group != null ) {
                        UpdateLightingGroup( group );
                    }
                }
            });
        }

        private async void UpdateLightingGroup( BulbGroup group ) {
            if( group.GroupType.Equals( GroupType.Zone )) {
                await mGroupManager.UpdateZone( group );
            }
            else {
                await mGroupManager.UpdateRoom( group );
            }
            
            LoadLighting();
        }

        private void OnDeleteGroup( LightGroupViewModel ? lightGroup ) {
            if( lightGroup != null ) {
                var parameters = new DialogParameters {
                    { ConfirmationDialogViewModel.cTitle, "Delete Light Group" },
                    { ConfirmationDialogViewModel.cMessage, $"Would you like to delete light group '{lightGroup.GroupName}'?" }
                };

                mDialogService.ShowDialog<ConfirmationDialog>( parameters, result => {
                    if( result.Result.Equals( ButtonResult.Ok )) {
                        DeleteLightingGroup( lightGroup.Group );
                    }
                });
            }
        }

        private async void DeleteLightingGroup( BulbGroup group ) {
            if( group.GroupType.Equals( GroupType.Zone )) {
                await mGroupManager.DeleteZone( group );
            }
            else {
                await mGroupManager.DeleteRoom( group );
            }
            
            LoadLighting();
        }
    }
}
