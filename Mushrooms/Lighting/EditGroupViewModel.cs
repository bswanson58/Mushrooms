using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HueLighting.Models;
using Mushrooms.Entities;
using Mushrooms.Models;
using Q42.HueApi.Models.Groups;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.Platform;

namespace Mushrooms.Lighting {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class EditGroupViewModel : DialogAwareBase {
        public  const string        cGroup = "group";
        public  const string        cGroupType = "group type";
        public  const string        cLights = "lights";

        private BulbGroup ?         mBulbGroup;
        private GroupType           mGroupType;
        private string              mGroupName;
        private RoomClassItem ?     mRoomType;

        public  bool                IsRoomGroup { get; private set; }

        public  ObservableCollection<RoomClassItem>         RoomClasses { get; }
        public  ObservableCollection<LightSourceViewModel>  Lights { get; }

        public EditGroupViewModel() {
            RoomClasses = new ObservableCollection<RoomClassItem>();
            Lights = new ObservableCollection<LightSourceViewModel>();

            mBulbGroup = null;
            mGroupType = GroupType.Zone;
            mRoomType = null;
            mGroupName = String.Empty;

            Title = "Lighting Group Properties";

            LoadItems();
        }

        public override void OnDialogOpened( IDialogParameters parameters ) {
            mBulbGroup = parameters.GetValue<BulbGroup ?>( cGroup );
            var groupType = parameters.GetValue<GroupType ?>( cGroupType );
            var lightingList = parameters.GetValue<IEnumerable<Bulb>>( cLights );

            if( lightingList != null ) {
                Lights.Clear();
                Lights.AddRange( lightingList.Select( l => 
                    new LightSourceViewModel(
                        new LightSource( l.Id, l.Name, LightSourceType.Bulb ),
                        new []{ l }, OnLightSelected )));
            }

            if( mBulbGroup != null ) {
                mGroupName = mBulbGroup.Name;
                mGroupType = mBulbGroup.GroupType;
                SelectedRoomClass = RoomClasses.FirstOrDefault( c => c.RoomClass.Equals( mBulbGroup.RoomClass ));

                foreach( var light in Lights ) {
                    light.IsSelected = mBulbGroup.Bulbs.Any( b => b.Name.Equals( light.Name ));
                }
            }
            else {
                mGroupType = groupType ?? GroupType.Zone;
                mGroupName = String.Empty;
                mRoomType = null;
            }

            IsRoomGroup = mGroupType.Equals( GroupType.Room );

            RaiseAllPropertiesChanged();
            Ok.RaiseCanExecuteChanged();
        }

        private void OnLightSelected( LightSourceViewModel light ) {
            Ok.RaiseCanExecuteChanged();
        }

        private void LoadItems() {
            RoomClasses.Clear();
            RoomClasses.AddRange( RoomClassList.Values());
        }

        public string GroupName {
            get => mGroupName;
            set {
                mGroupName = value;

                Ok.RaiseCanExecuteChanged();
            }
        }

        public RoomClassItem ? SelectedRoomClass {
            get => mRoomType;
            set {
                mRoomType = value;

                Ok.RaiseCanExecuteChanged();
            }
        }

        protected override bool CanAccept() =>
            !String.IsNullOrWhiteSpace( mGroupName ) &&
            Lights.Any( l => l.IsSelected );

        protected override DialogParameters CreateClosingParameters() {
            var bulbs = Lights
                .Where( l => l.IsSelected )
                .SelectMany( l => l.Bulbs )
                .ToList();

            var bulbGroup = 
                new BulbGroup( mBulbGroup?.Id ?? String.Empty, mGroupName, mGroupType, mRoomType?.RoomClass, bulbs );

            if( mBulbGroup != null ) {
                bulbGroup = new BulbGroup( 
                    mBulbGroup.Id ?? String.Empty, mGroupName, mGroupType, mRoomType?.RoomClass, bulbs );
            }

            return new DialogParameters { { cGroup, bulbGroup } };
        }
    }
}
