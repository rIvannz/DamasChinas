using System;
using System.ServiceModel;
using DamasChinas_Client.UI.FriendServiceProxy;

namespace DamasChinas_Client.UI.Callbacks
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public sealed class FriendCallbackHandler : IFriendServiceCallback
    {
        // ================================================================
        // EVENTOS PARA UI
        // ================================================================
        public static event Action<string> FriendRequestReceivedEvent;
        public static event Action<string> FriendRequestAcceptedEvent;
        public static event Action<string> FriendRemovedEvent;
        public static event Action<string> UserBlockedYouEvent;
        public static event Action<string> UserUnblockedYouEvent;

        // Refrescar lista al instante
        public static event Action FriendListUpdatedEvent;

        // ================================================================
        // MÉTODOS INVOCADOS POR EL SERVIDOR
        // ================================================================
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
            UserBlockedYouEvent?.Invoke(username);
        }

        public void UserUnblockedYou(string username)
        {
            UserUnblockedYouEvent?.Invoke(username);
        }

        public void FriendListUpdated()
        {
            FriendListUpdatedEvent?.Invoke();
        }
    }
}
