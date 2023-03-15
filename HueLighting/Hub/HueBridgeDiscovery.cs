using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Q42.HueApi;
using Q42.HueApi.Models.Bridge;

// ReSharper disable CommentTypo

namespace HueLighting.Hub {
    public interface IHueBridgeDiscovery {
        Task<List<LocatedBridge>> CompleteDiscoveryAsync( TimeSpan fastTimeout, TimeSpan maxNetworkScanTimeout );
        Task<List<LocatedBridge>> FastDiscoveryWithNetworkScanFallbackAsync( TimeSpan fastTimeout, TimeSpan maxNetworkScanTimeout );
        Task<List<LocatedBridge>> FastDiscoveryAsync( TimeSpan timeout );
    }

    /// <summary>
    /// Some Hue Bridge Discovery approaches using the Q42.HueApi
    /// See https://developers.meethue.com/develop/application-design-guidance/hue-bridge-discovery/
    /// </summary>
    public class HueBridgeDiscovery2 : IHueBridgeDiscovery {
        /// <summary>
        /// Discovery of Hue Bridges:
        /// - Run all locators (N-UPNP, MDNS, SSDP, network scan) in parallel
        /// - Check first with N-UPNP, then MDNS/SSDP
        /// - If nothing found, continue with network scan
        /// </summary>
        /// <remarks>General purpose approach for comprehensive search</remarks>
        /// <remarks>Max awaited time if nothing found is maxScanTimeout</remarks>
        /// <param name="fastTimeout">Timeout for the fast locators (at least a few seconds, usually around 5 seconds)</param>
        /// <param name="maxNetworkScanTimeout">Timeout for the slow network scan (at least a 30 seconds, to few minutes)</param>
        /// <returns>List of bridges found</returns>
        public async Task<List<LocatedBridge>> CompleteDiscoveryAsync( TimeSpan fastTimeout, TimeSpan maxNetworkScanTimeout ) {
            using var fastLocatorsCancelSrc = new CancellationTokenSource( fastTimeout );
            using var slowNetworkScanCancelSrc = new CancellationTokenSource( maxNetworkScanTimeout );

            // Start all tasks in parallel without awaiting
            // Pack all fast locators in an array so we can await them in order
            var fastLocateTask = new [] {
                // N-UPNP is the fastest (only one endpoint to check)
                (new HttpBridgeLocator()).LocateBridgesAsync( fastLocatorsCancelSrc.Token ),
                // MDNS is the most reliable for bridge V2
                (new MdnsBridgeLocator()).LocateBridgesAsync( fastLocatorsCancelSrc.Token ),
                // SSDP is older but works for bridge V1 & V2
                (new SsdpBridgeLocator()).LocateBridgesAsync( fastLocatorsCancelSrc.Token ),
            };

            // The network scan locator is clearly the slowest
            var slowNetworkScanTask = ( new LocalNetworkScanBridgeLocator()).LocateBridgesAsync( slowNetworkScanCancelSrc.Token );

            // We will loop through the fast locators and await each one in order
            foreach( var fastTask in fastLocateTask ) {
                // Await this task to get its result
                var result = ( await fastTask ).ToList();

                // Check if it contains anything
                if( result.Any()) {
                    // Cancel all remaining tasks and return
                    fastLocatorsCancelSrc.Cancel();
                    slowNetworkScanCancelSrc.Cancel();

                    return result;
                }
            }

            // All fast locators failed, we wait for the network scan to complete and return whatever we found
            return ( await slowNetworkScanTask ).ToList();
        }

        /// <summary>
        /// Discovery of Hue Bridges:
        /// - Run all fast locators (N-UPNP, MDNS, SSDP) in parallel
        /// - Check first with N-UPNP, then MDNS/SSDP after 5 seconds
        /// - If nothing found, run network scan up to 1 minute
        /// </summary>
        /// <remarks>Better approach for comprehensive search for smartphone environment</remarks>
        /// <remarks>Max awaited time if nothing found is fastTimeout + maxScanTimeout</remarks>
        /// <param name="fastTimeout">Timeout for the fast locators (at least a few seconds, usually around 5 seconds)</param>
        /// <param name="maxNetworkScanTimeout">Timeout for the slow network scan (at least a 30 seconds, to few minutes)</param>
        /// <returns>List of bridges found</returns>
        public async Task<List<LocatedBridge>> FastDiscoveryWithNetworkScanFallbackAsync( TimeSpan fastTimeout, TimeSpan maxNetworkScanTimeout ) {
            using var fastLocatorsCancelSrc = new CancellationTokenSource( fastTimeout );

            // Start all tasks in parallel without awaiting
            // Pack all fast locators in an array so we can await them in order
            var fastLocateTask = new [] {
                // N-UPNP is the fastest (only one endpoint to check)
                (new HttpBridgeLocator()).LocateBridgesAsync( fastLocatorsCancelSrc.Token ),
                // MDNS is the most reliable for bridge V2
                (new MdnsBridgeLocator()).LocateBridgesAsync( fastLocatorsCancelSrc.Token ),
                // SSDP is older but works for bridge V1 & V2
                (new SsdpBridgeLocator()).LocateBridgesAsync( fastLocatorsCancelSrc.Token ),
            };

            // We will loop through the fast locators and await each one in order
            foreach( var fastTask in fastLocateTask ) {
                // Await this task to get its result
                var result = ( await fastTask ).ToList();

                // Check if it contains anything
                if( result.Any() ) {
                    // Cancel all remaining tasks and return
                    fastLocatorsCancelSrc.Cancel();

                    return result;
                }
            }

            // All fast locators failed, let's try the network scan and return whatever we found
            var scanResults = await ( new LocalNetworkScanBridgeLocator()).LocateBridgesAsync( maxNetworkScanTimeout );
            return scanResults.ToList();
        }

        /// <summary>
        /// Discovery of Hue Bridges:
        /// - Run only the fast locators (N-UPNP, MDNS, SSDP) in parallel
        /// - Check first with N-UPNP, then MDNS/SSDP
        /// </summary>
        /// <remarks>Best approach if network scan is not desirable</remarks>
        /// <param name="timeout">Timeout for the search (at least a few seconds, usually around 5 seconds)</param>
        /// <returns>List of bridges found</returns>
        public async Task<List<LocatedBridge>> FastDiscoveryAsync( TimeSpan timeout ) {
            using var fastLocatorsCancelSrc = new CancellationTokenSource( timeout );

            // Start all tasks in parallel without awaiting
            // Pack all fast locators in an array so we can await them in order
            var fastLocateTask = new [] {
                // N-UPNP is the fastest (only one endpoint to check)
                (new HttpBridgeLocator()).LocateBridgesAsync( fastLocatorsCancelSrc.Token ),
                // MDNS is the most reliable for bridge V2
                (new MdnsBridgeLocator()).LocateBridgesAsync( fastLocatorsCancelSrc.Token ),
                // SSDP is older but works for bridge V1 & V2
                (new SsdpBridgeLocator()).LocateBridgesAsync( fastLocatorsCancelSrc.Token ),
            };

            var result = new List<LocatedBridge>();

            // We will loop through the fast locators and await each one in order
            foreach( var fastTask in fastLocateTask ) {
                // Await this task to get its result
                result = ( await fastTask ).ToList();

                // Check if it contains anything
                if( result.Any()) {
                    // Cancel all remaining tasks and break
                    fastLocatorsCancelSrc.Cancel();

                    break;
                }
            }

            return result.ToList();
        }

        public async Task<List<LocatedBridge>> ScanEverythingAsync( TimeSpan maxScanTimeout ) {
            var retValue = new List<LocatedBridge>();

            // Start all tasks in parallel without awaiting
            // Pack all locators in an array so we can await them in order
            var fastLocateTask = new [] {
                // N-UPNP is the fastest (only one endpoint to check)
                (new HttpBridgeLocator()).LocateBridgesAsync( maxScanTimeout ),
                // MDNS is the most reliable for bridge V2
                (new MdnsBridgeLocator()).LocateBridgesAsync( maxScanTimeout ),
                // SSDP is older but works for bridge V1 & V2
                (new SsdpBridgeLocator()).LocateBridgesAsync( maxScanTimeout ),
                // network scan
                (new LocalNetworkScanBridgeLocator()).LocateBridgesAsync( maxScanTimeout )
            };

            // We will loop through the fast locators and await each one in order
            foreach( var fastTask in fastLocateTask ) {
                // Await this task to get its result
                retValue.AddRange( await fastTask );
            }

            return retValue
                .GroupBy( b => b.BridgeId )
                .Select( g => g.First() )
                .ToList();
        }
    }
}