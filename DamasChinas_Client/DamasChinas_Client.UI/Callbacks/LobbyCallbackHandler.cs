using DamasChinas_Client.UI.LobbyServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System;
using System.ServiceModel;
using System.Windows;

namespace DamasChinas_Client.UI.Callbacks
{
    // CRÍTICO: Esto previene el Deadlock (congelamiento) en WPF
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public sealed class LobbyCallbackHandler : ILobbyServiceCallback
    {
        // Eventos para que la UI se suscriba
        public static event Action<LobbySnapshotDto> SnapshotReceived;
        public static event Action<MessageCode> LobbyClosed;
        public static event Action GameStarting;
        public static event Action<string, string, string> ChatMessageReceived;
        public static event Action<MessageCode> Kicked;

        public void OnLobbySnapshot(LobbySnapshotDto snapshot)
        {
            SnapshotReceived?.Invoke(snapshot);
        }

        public void OnKickedFromLobby(MessageCode reason)
        {
            Kicked?.Invoke(reason);
        }

        public void OnLobbyClosed(MessageCode reason)
        {
            LobbyClosed?.Invoke(reason);
        }

        public void OnInvitationReceived(LobbyInvitationDto invitation)
        {
            // Lógica de invitación (Popup global)
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Aquí podrías mostrar un popup global de invitación
            });
        }

        public void OnGameStarting()
        {
            GameStarting?.Invoke();
        }

        public void OnBanStatusUpdated(BanInfoDto ban)
        {
            // Implementación futura
        }

        public void OnChatMessageReceived(string sender, string message, string timestamp)
        {
            ChatMessageReceived?.Invoke(sender, message, timestamp);
        }
    }
}