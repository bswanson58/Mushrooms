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
        Task<bool>                      InitializeConfiguredHub();
        bool                            IsInitialized { get; }

        Task<IEnumerable<Bulb>>         GetBulbs();
        Task<IEnumerable<BulbGroup>>    GetBulbGroups();

        Task<BulbGroup ?>               CreateGroup( string zoneName, IList<Bulb> bulbs, GroupType groupType, RoomClass ? roomClass = null );
        Task                            DeleteGroup( BulbGroup group );
        Task                            UpdateGroup( BulbGroup group );
    }

    public class LightingGroupManager : ILightingGroupManager {
        private readonly IPreferences           mPreferences;
        private readonly IBasicLog              mLog;
        private ILocalHueClient ?               mClient;

        public  bool                            IsInitialized => mClient != null;

        public LightingGroupManager( IPreferences preferences, IBasicLog log ) {
            mPreferences = preferences;
            mLog = log;
        }

        public async Task<bool> InitializeConfiguredHub() {
            var retValue = false;
            var installationInfo = mPreferences.Load<HueConfiguration>();

            if(!String.IsNullOrWhiteSpace( installationInfo.BridgeIp )) {
                try {
                    mClient = new LocalHueClient( installationInfo.BridgeIp, installationInfo.BridgeAppKey );

                    retValue = await mClient.CheckConnection();

                    if(!retValue ) {
                        mClient = null;
                    }
                }
                catch( Exception ) {
                    mClient = null;

                    retValue = false;
                }
            }

            return retValue;
        }

        public async Task<IEnumerable<Bulb>> GetBulbs() {
            var retValue = Enumerable.Empty<Bulb>();

            try {
                if( mClient == null ) {
                    await InitializeConfiguredHub();
                }

                if( mClient != null ) {
                    var lights = await mClient.GetLightsAsync();

                    retValue = from light in lights select new Bulb( light.Id, light.Name, light.State.IsReachable == true );
                }
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( GetBulbs ), ex );
            }

            return retValue;
        }

        public async Task<IEnumerable<BulbGroup>> GetBulbGroups() {
            var retValue = Enumerable.Empty<BulbGroup>();

            try {
                if( mClient == null ) {
                    await InitializeConfiguredHub();
                }

                if( mClient != null ) {
                    retValue = await ToBulbGroup( await mClient.GetGroupsAsync());
                }
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( GetBulbGroups ), ex );
            }

            return retValue;
        }

        private async Task<IEnumerable<BulbGroup>> ToBulbGroup( IEnumerable<Group> fromList ) {
            var retValue = new List<BulbGroup>();
            var bulbList = await GetBulbs();

            foreach( var g in fromList ) {
                retValue.Add( new BulbGroup( g.Id, g.Name, g.Type, g.Class,
                    from bulb in g.Lights select bulbList.FirstOrDefault( b => b.Id.Equals( bulb ))));
            }

            return retValue;
        }

        public async Task<BulbGroup ?> CreateGroup( string groupName, IList<Bulb> bulbs, GroupType groupType, RoomClass ? roomClass ) {
            try {
                if( mClient == null ) {
                    await InitializeConfiguredHub();
                }

                if( mClient != null ) {
                    var groupId = await mClient.CreateGroupAsync( bulbs.Select( b => b.Name ), groupName, roomClass, groupType );

                    if(!String.IsNullOrWhiteSpace( groupId )) {
                        return new BulbGroup( groupId, groupName, groupType, roomClass, bulbs );
                    }
                }
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( CreateGroup ), ex );
            }

            return default;
        }

        public async Task DeleteGroup( BulbGroup group ) =>
            await DeleteGroup( group.Id );

        private async Task DeleteGroup( string groupId ) {
            try {
                if( mClient == null ) {
                    await InitializeConfiguredHub();
                }

                if( mClient != null ) {
                    var results = await mClient.DeleteGroupAsync( groupId );

//                    if( results.Any( r => !r.Error )) { }
                }
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( DeleteGroup ), ex );
            }
        }

        public async Task UpdateGroup( BulbGroup group ) =>
            await UpdateGroup( group.Id, group.Name, group.Bulbs, group.RoomClass );

        private async Task UpdateGroup( string groupId, string groupName, IList<Bulb> bulbs, RoomClass ? roomClass ) {
            try {
                if( mClient == null ) {
                    await InitializeConfiguredHub();
                }

                if( mClient != null ) {
                    await mClient.UpdateGroupAsync( groupId, bulbs.Select( b => b.Name ), groupName, roomClass );
                }
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( UpdateGroup ), ex );
            }
        }
    }
}
