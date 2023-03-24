using HueLighting.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Q42.HueApi.Models.Groups;
using Q42.HueApi.Interfaces;
using ReusableBits.Platform.Interfaces;
using ReusableBits.Platform.Preferences;
using Q42.HueApi;

namespace HueLighting.Hub {
    public interface ILightingGroupManager {
        Task<BulbGroup ?>               CreateZone( string zoneName, IList<Bulb> bulbs, RoomClass ? roomClass = null );
        Task                            DeleteZone( BulbGroup group );
        Task                            UpdateZone( BulbGroup group );

        Task<BulbGroup ?>               CreateRoom( string zoneName, IList<Bulb> bulbs, RoomClass ? roomClass = null );
        Task                            DeleteRoom( BulbGroup group );
        Task                            UpdateRoom( BulbGroup group );
    }

    public class LightingGroupManager : ILightingGroupManager {
        private readonly IPreferences           mPreferences;
        private readonly IBasicLog              mLog;
        private ILocalHueClient ?               mClient;

        public LightingGroupManager( IPreferences preferences, IBasicLog log ) {
            mPreferences = preferences;
            mLog = log;
        }

        private async Task InitializeConfiguredHub() {
            var installationInfo = mPreferences.Load<HueConfiguration>();

            if(!String.IsNullOrWhiteSpace( installationInfo.BridgeIp )) {
                try {
                    mClient = new LocalHueClient( installationInfo.BridgeIp, installationInfo.BridgeAppKey );

                    var retValue = await mClient.CheckConnection();

                    if(!retValue ) {
                        mClient = null;
                    }
                }
                catch( Exception ) {
                    mClient = null;
                }
            }
        }

        public async Task<BulbGroup ?> CreateZone( string groupName, IList<Bulb> bulbs, RoomClass ? roomClass ) =>
            await CreateGroup( groupName, bulbs, GroupType.Zone, roomClass );

        public async Task<BulbGroup ?> CreateRoom( string groupName, IList<Bulb> bulbs, RoomClass ? roomClass ) =>
            await CreateGroup( groupName, bulbs, GroupType.Room, roomClass );

        private async Task<BulbGroup ?> CreateGroup( string groupName, IList<Bulb> bulbs, GroupType groupType, RoomClass ? roomClass ) {
            try {
                if( mClient == null ) {
                    await InitializeConfiguredHub();
                }

                if( mClient != null ) {
                    var groupId = await mClient.CreateGroupAsync( bulbs.Select( b => b.Id ), groupName, roomClass, groupType );

                    if(!String.IsNullOrWhiteSpace( groupId )) {
                        return new BulbGroup( groupId, groupName, GroupType.Zone, roomClass, bulbs );
                    }
                }
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( CreateZone ), ex );
            }

            return default;
        }

        public async Task DeleteZone( BulbGroup group ) =>
            await DeleteGroup( group.Id );

        public async Task DeleteRoom( BulbGroup group ) =>
            await DeleteGroup( group.Id );

        private async Task DeleteGroup( string groupId ) {
            try {
                if( mClient == null ) {
                    await InitializeConfiguredHub();
                }

                if( mClient != null ) {
                    var results = await mClient.DeleteGroupAsync( groupId );

                    if( results.Any()) {
                        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                        var firstError = results.FirstOrDefault( r => r.Error?.Description != null );

                        if( firstError != null ) {
                            mLog.LogMessage( $"Deleting hue group '{groupId}': {firstError.Error.Description}" );
                        }
                    }
                }
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( DeleteGroup ), ex );
            }
        }

        public async Task UpdateZone( BulbGroup group ) =>
            await UpdateGroup( group.Id, group.Name, group.Bulbs, group.RoomClass );

        public async Task UpdateRoom( BulbGroup group ) =>
            await UpdateGroup( group.Id, group.Name, group.Bulbs, group.RoomClass );

        private async Task UpdateGroup( string groupId, string groupName, IList<Bulb> bulbs, RoomClass ? roomClass ) {
            try {
                if( mClient == null ) {
                    await InitializeConfiguredHub();
                }

                if( mClient != null ) {
                    await mClient.UpdateGroupAsync( groupId, bulbs.Select( b => b.Id ), groupName, roomClass );
                }
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( UpdateGroup ), ex );
            }
        }
    }
}
