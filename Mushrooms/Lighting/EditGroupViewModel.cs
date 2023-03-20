using System;
using System.Collections.ObjectModel;
using System.Linq;
using HueLighting.Models;
using Q42.HueApi.Models.Groups;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.Platform;

namespace Mushrooms.Lighting {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class EditGroupViewModel : DialogAwareBase {
        public  const string        cGroup = "group";

        private BulbGroup ?         mBulbGroup;
        private string              mGroupName;
        private GroupTypeItem ?     mGroupType;
        private RoomClassItem ?     mRoomType;

        public  ObservableCollection<GroupTypeItem>     GroupTypes { get; }
        public  ObservableCollection<RoomClassItem>     RoomClasses { get; }

        public EditGroupViewModel() {
            GroupTypes = new ObservableCollection<GroupTypeItem>();
            RoomClasses = new ObservableCollection<RoomClassItem>();

            mBulbGroup = null;
            mGroupType = null;
            mRoomType = null;
            mGroupName = String.Empty;

            Title = "Lighting Group Properties";

            LoadItems();
        }

        public override void OnDialogOpened( IDialogParameters parameters ) {
            mBulbGroup = parameters.GetValue<BulbGroup>( cGroup );

            if( mBulbGroup != null ) {
                mGroupName = mBulbGroup.Name;
                SelectedGroupType = GroupTypes.FirstOrDefault( g => g.GroupType.Equals( mBulbGroup.GroupType ));
                SelectedRoomClass = RoomClasses.FirstOrDefault( c => c.RoomClass.Equals( mBulbGroup.RoomClass ));
            }
            else {
                mGroupName = String.Empty;
                mGroupType = null;
                mRoomType = null;
            }

            RaiseAllPropertiesChanged();
            Ok.RaiseCanExecuteChanged();
        }

        private void LoadItems() {
            GroupTypes.Clear();
            GroupTypes.AddRange( GroupTypeList.Values());

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

        public GroupTypeItem ? SelectedGroupType {
            get => mGroupType;
            set {
                mGroupType = value;

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
            mGroupType != null;

        protected override DialogParameters CreateClosingParameters() {
            var groupType = mGroupType?.GroupType ?? GroupType.LightGroup;

            var bulbGroup = 
                new BulbGroup( String.Empty, mGroupName, groupType, mRoomType?.RoomClass, Enumerable.Empty<Bulb>());

            if( mBulbGroup != null ) {
                bulbGroup = new BulbGroup( 
                    mBulbGroup.Id, mGroupName, groupType, mRoomType?.RoomClass, mBulbGroup.Bulbs );
            }

            return new DialogParameters { { cGroup, bulbGroup } };
        }
    }
}
