using ReusableBits.Platform.Interfaces;
using ReusableBits.Platform.Preferences;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HueApi;
using HueApi.Models;
using HueLighting.Models;

// Uses Hue V2 API, not yet complete. Not used due to incomplete HueApi functionality.

// ReSharper disable InconsistentNaming

namespace HueLighting.Hub {
    public interface ILightingCommand {
        Task<IList<HueLight>>           GetLights();
        Task<IList<HueGroup>>           GetZones();
        Task<IList<HueGroup>>           GetRooms();
        Task<IList<HueGroupedLight>>    GetGroupedLights();
    }

    [DebuggerDisplay("Light: {Name}")]
    public class HueLight {
        public  Guid        Id { get; }
        public  string      Name { get; }

        public HueLight( Guid id, string name ) {
            Id = id;
            Name = name;
        }
    }

    public enum HueGroupType {
        Room,
        Zone
    }

    [DebuggerDisplay("{GroupType}: {Name}")]
    public class HueGroup {
        public  Guid                    Id { get; }
        public  Guid                    GroupId { get; }
        public  HueGroupType            GroupType { get; }
        public  string                  Name { get; }
        public  IReadOnlyList<HueLight> Lights { get; }

        public HueGroup( Guid id, string name, Guid groupId, HueGroupType groupType, IEnumerable<HueLight> lights ) {
            Id = id;
            GroupId = groupId;
            GroupType = groupType;
            Name = name;
            Lights = new List<HueLight>( lights );
        }
    }

    public class HueGroupedLight {
        public  Guid            Id { get; }
        public  Guid            Owner { get; }
        public  HueGroupType    OwnerType { get; }

        public HueGroupedLight( Guid id, Guid owner, HueGroupType ownerType ) {
            Id = id;
            Owner = owner;
            OwnerType = ownerType;
        }
    }

    internal static class HueTypes {
        public  const string    BridgeHome = "bridge_home";
        public  const string    Light = "light";
        public  const string    GroupedLight = "grouped_light";
        public  const string    Zone = "zone";
        public  const string    Room = "room";
    }

    public class LightingCommand : ILightingCommand {
        private readonly IPreferences   mPreferences;
        private readonly IBasicLog      mLog;
        private LocalHueApi ?           mClient;

        public LightingCommand( IPreferences preferences, IBasicLog log ) {
            mPreferences = preferences;
            mLog = log;
        }

        private LocalHueApi HueClient() {
            if( mClient == null ) {
                var installationInfo = mPreferences.Load<HueConfiguration>();

                if(!String.IsNullOrWhiteSpace( installationInfo.BridgeIp )) {
                    mClient = new LocalHueApi( installationInfo.BridgeIp, installationInfo.BridgeAppKey );
                }
            }

            if( mClient == null ) {
                throw new ApplicationException( "LocalHueApi client could not be created from HueConfiguration" );
            }

            return mClient!;
        }

        public async Task<IList<HueLight>> GetLights() {
            var retValue = new List<HueLight>();

            try {
                var lighting = await HueClient().GetLightsAsync();

                foreach( var light in lighting.Data ) {
                    retValue.Add( new HueLight( light.Id, light.Metadata?.Name ?? String.Empty ));
                }
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( GetLights ), ex );
            }

            return retValue;
        }

        public async Task<IList<HueGroup>> GetZones() {
            var retValue = new List<HueGroup>();

            try {
                var lights = await HueClient().GetLightsAsync();
                var zones = await HueClient().GetZonesAsync();

                foreach( var zone in zones.Data ) {
                    retValue.Add( BuildZone( lights.Data, zone ));
                }
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( GetLights ), ex );
            }

            return retValue;
        }

        private HueGroup BuildZone( IList<Light> lights, Zone forZone ) {
            var lightList = new List<HueLight>();

            foreach( var resource in forZone.Children.Where( c => c.Rtype.Equals( HueTypes.Light ))) {
                var bulb = lights.FirstOrDefault( b => b.Id.Equals( resource.Rid ));

                if( bulb != null ) {
                    lightList.Add( new HueLight( bulb.Id, bulb.Metadata?.Name ?? String.Empty ));
                }
            }

            var groupService = forZone.Services.FirstOrDefault( s => s.Rtype.Equals( HueTypes.GroupedLight ));
            var groupId = groupService != null ? groupService.Rid : Guid.Empty;

            return new HueGroup( forZone.Id, forZone.Metadata?.Name ?? String.Empty, groupId, HueGroupType.Zone, lightList );
        }

        public async Task<IList<HueGroup>> GetRooms() {
            var retValue = new List<HueGroup>();

            try {
                var devices = await HueClient().GetDevicesAsync();
                var lights = await HueClient().GetLightsAsync();
                var rooms = await HueClient().GetRoomsAsync();

                foreach( var room in rooms.Data ) {
                    retValue.Add( BuildRoom( devices.Data, lights.Data, room ));
                }
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( GetLights ), ex );
            }

            return retValue;
        }

        private HueGroup BuildRoom( IList<Device> devices, IList<Light> lights, Room forRoom ) {
            var lightList = new List<HueLight>();

            foreach( var resource in forRoom.Children ) {
                var device = devices.FirstOrDefault( b => b.Id.Equals( resource.Rid ));

                if( device != null ) {
                    var lightServices = device.Services.Where( s => s.Rtype.Equals( HueTypes.Light ));

                    foreach( var lightService in lightServices ) {
                        var light = lights.FirstOrDefault( l => l.Id.Equals( lightService.Rid ));

                        if( light != null ) {
                            lightList.Add( new HueLight( device.Id, device.Metadata?.Name ?? String.Empty ));
                        }
                    }
                }
            }

            var groupService = forRoom.Services.FirstOrDefault( s => s.Rtype.Equals( HueTypes.GroupedLight ));
            var groupId = groupService != null ? groupService.Rid : Guid.Empty;

            return new HueGroup( forRoom.Id, forRoom.Metadata?.Name ?? String.Empty, groupId, HueGroupType.Room, lightList );
        }

        public async Task<IList<HueGroupedLight>> GetGroupedLights() {
            var retValue = new List<HueGroupedLight>();

            try {
                var groups = await HueClient().GetGroupedLightsAsync();
                var bridges = await HueClient().GetBridgeHomesAsync();

                foreach( var group in groups.Data ) {
                    if( group.Owner != null ) {
                        if( group.Owner.Rtype.Equals( HueTypes.BridgeHome )) {
                            var bridge = bridges.Data.FirstOrDefault( b => b.Id.Equals( group.Owner.Rid ));

                            if(( bridge != null ) &&
                               ( bridge.ExtensionData.ContainsKey( "children" ))) {
                                var children = bridge.ExtensionData["children"];
                                var data = children.Deserialize<List<ResourceIdentifier>>();

                                if( data != null ) {
                                    foreach( var rid in data ) {
                                        if( rid.Rtype.Equals( HueTypes.Room )) {
                                            retValue.Add( new HueGroupedLight( group.Id, rid.Rid, HueGroupType.Room ));
                                        }
                                        if( rid.Rtype.Equals( HueTypes.Zone )) {
                                            retValue.Add( new HueGroupedLight( group.Id, rid.Rid, HueGroupType.Zone ));
                                        }
                                    }
                                }
                            }
                        }
                        else {
                            var groupedLight = CreateGroupedLight( group, group.Owner );

                            if( groupedLight != null ) {
                                retValue.Add( groupedLight );
                            }
                        }
                    }
                }
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( GetLights ), ex );
            }

            return retValue;
        }

        private HueGroupedLight ? CreateGroupedLight( GroupedLight group, ResourceIdentifier fromRid ) {
            if( group.Owner != null ) {
                if( fromRid.Rtype.Equals( HueTypes.Room )) {
                    return new HueGroupedLight( group.Id, group.Owner.Rid, HueGroupType.Room );
                }
                if( fromRid.Rtype.Equals( HueTypes.Zone )) { 
                    return new HueGroupedLight( group.Id, group.Owner.Rid, HueGroupType.Zone );
                }
            }

            return default;
        }
    }
}
