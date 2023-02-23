using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mushrooms.Support {
    internal static class Repeat {
        public static Task Interval( TimeSpan pollInterval, Action action, CancellationToken token ) =>
            // We don't use Observable.Interval:
            // If we block, the values start bunching up behind each other.
            Task.Run( () => {
                while( true ) {
                    if( token.WaitCancellationRequested( pollInterval )) {
                        break;
                    }

                    action();
                }
            }, token );
    }

    static class CancellationTokenExtensions {
        public static bool WaitCancellationRequested( this CancellationToken token, TimeSpan timeout ) =>
            token.WaitHandle.WaitOne( timeout );
    }
}
