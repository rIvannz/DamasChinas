using DamasChinas_Client.UI.LobbyServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System.Windows;

namespace DamasChinas_Client.UI.Callbacks
{
    public sealed class LobbyCallbackHandler : ILobbyServiceCallback
    {
        public void OnLobbySnapshot(LobbySnapshotDto snapshot)
        {
            // Actualmente no usamos este handler;
            // el PreLobby se actualiza a través de LobbyManager.
        }

        public void OnKickedFromLobby(MessageCode reason)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageHelper.ShowPopup(MessageKeys.LobbyClosed, PopupType.Warning);
            });
        }

        public void OnLobbyClosed(MessageCode reason)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageHelper.ShowPopup(MessageKeys.LobbyClosed, PopupType.Warning);
            });
        }

        public void OnInvitationReceived(LobbyInvitationDto invitation)
        {
            // Pendiente de implementar UI de invitaciones.
        }

        public void OnGameStarting()
        {
            // Pendiente de implementar transición a la partida.
        }

        public void OnBanStatusUpdated(BanInfoDto ban)
        {
            // Pendiente de mostrar información de baneo.
        }
    }
}
