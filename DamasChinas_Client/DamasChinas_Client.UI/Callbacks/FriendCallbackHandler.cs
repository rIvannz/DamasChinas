using System;
using System.ServiceModel;
using DamasChinas_Client.UI.FriendServiceProxy;

namespace DamasChinas_Client.UI.Callbacks
{
    // Igual que LobbyCallbackHandler para evitar deadlocks
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public sealed class FriendCallbackHandler : IFriendServiceCallback
    {
        // ================================================================
        // EVENTOS PARA ACTUALIZAR UI EN PÁGINAS
        // ================================================================
        public static event Action<string> FriendRequestReceivedEvent;
        public static event Action<string> FriendRequestAcceptedEvent;
        public static event Action<string> FriendRemovedEvent;
        public static event Action<string> UserBlockedYouEvent;
        public static event Action<string> UserUnblockedYouEvent;

        // Solo disparamos eventos. La UI decide si muestra popups o no.

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
    }
}
