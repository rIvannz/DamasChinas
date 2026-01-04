using DamasChinas_Client.UI.GuestSessionServiceProxy;
using System;

namespace DamasChinas_Client.UI.Callbacks
{
    public sealed class GuestSessionCallbackHandler : IGuestSessionServiceCallback
    {
        public static event Action<string> ServerMessageReceived;

        public void OnServerMessage(string code)
        {
            ServerMessageReceived?.Invoke(code);
        }
    }
}
