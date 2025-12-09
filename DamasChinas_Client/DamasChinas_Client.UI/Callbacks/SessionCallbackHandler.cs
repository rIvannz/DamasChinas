using System;
using DamasChinas_Client.UI.SessionServiceProxy;

namespace DamasChinas_Client.UI.Callbacks
{
    public class SessionCallbackHandler : ISessionServiceCallback
    {


        public static event Action<string> PlayerConnectedEvent;
        public static event Action<string> PlayerDisconnectedEvent;
        public static event Action SessionExpiredEvent;
        public static event Action<string> PlayerInGameEvent;
        public static event Action<string> PlayerLeftGameEvent;


 

        public void PlayerConnected(string username)
        {
            PlayerConnectedEvent?.Invoke(username);
        }

        public void PlayerDisconnected(string username)
        {
            PlayerDisconnectedEvent?.Invoke(username);
        }

        public void SessionExpired()
        {
            SessionExpiredEvent?.Invoke();
        }

        public void PlayerInGame(string username)
        {
            PlayerInGameEvent?.Invoke(username);
        }

        public void PlayerLeftGame(string username)
        {
            PlayerLeftGameEvent?.Invoke(username);
        }

    }
}
