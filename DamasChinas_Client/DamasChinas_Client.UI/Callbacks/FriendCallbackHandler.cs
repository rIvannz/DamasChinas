using System;
using DamasChinas_Client.UI.FriendServiceProxy;

namespace DamasChinas_Client.UI.Callbacks
{
    public class FriendCallbackHandler : IFriendServiceCallback
    {
        public static event Action<string> FriendRequestReceivedEvent;
        public static event Action<string> FriendRequestAcceptedEvent;
        public static event Action<string> FriendRemovedEvent;
        public static event Action<string> UserBlockedEvent;
        public static event Action<string> UserUnblockedEvent;

        public void FriendRequestReceived(string fromUsername)
        {
            FriendRequestReceivedEvent?.Invoke(fromUsername);
        }

        public void FriendRequestAccepted(string byUsername)
        {
            FriendRequestAcceptedEvent?.Invoke(byUsername);
        }

        public void FriendRemoved(string username)
        {
            FriendRemovedEvent?.Invoke(username);
        }

        public void UserBlockedYou(string username)
        {
            UserBlockedEvent?.Invoke(username);
        }

        public void UserUnblockedYou(string username)
        {
            UserUnblockedEvent?.Invoke(username);
        }
    }
}
