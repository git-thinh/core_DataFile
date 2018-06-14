using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp.Server;

namespace WebSocketSharp
{
    public static class Ext2
    {
        internal static string CheckIfAvailable(
          this ServerState state, bool ready, bool start, bool shutting)
        {
            return (!ready && (state == ServerState.Ready || state == ServerState.Stop)) ||
                   (!start && state == ServerState.Start) ||
                   (!shutting && state == ServerState.ShuttingDown)
                   ? "This operation isn't available in: " + state.ToString().ToLower()
                   : null;
        }
    }
}
