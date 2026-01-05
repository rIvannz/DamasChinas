using DamasChinas_Client.UI.LobbyServiceProxy;
using DamasChinas_Shared.Contracts.Dtos;
using System;
using System.ServiceModel;
using System.Windows;
using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI.Callbacks
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public sealed class LobbyCallbackHandler : ILobbyServiceCallback
    {
        public static event Action<LobbySnapshotDto> SnapshotReceived;
        public static event Action<MessageCode> LobbyClosed;
        public static event Action GameStarting;
        public static event Action<string, string, string> ChatMessageReceived;
        public static event Action<MessageCode> Kicked;

        public static event Action<BanInfoDto> BanStatusUpdated;

        public void OnLobbySnapshot(LobbySnapshotDto snapshot)
            => SnapshotReceived?.Invoke(snapshot);

        public void OnKickedFromLobby(MessageCode reason)
            => Kicked?.Invoke(reason);

        public void OnLobbyClosed(MessageCode reason)
            => LobbyClosed?.Invoke(reason);

        public void OnInvitationReceived(LobbyInvitationDto invitation)
        {
        }

        public void OnGameStarting()
            => GameStarting?.Invoke();

        public void OnBanStatusUpdated(BanInfoDto banInfo)
        {
            if (banInfo == null)
                return;

            BanStatusUpdated?.Invoke(banInfo);

            var app = Application.Current;
            if (app?.Dispatcher == null)
            {
  
                if (banInfo.IsBanned) PendingBanNotificationStore.Save(banInfo);
                else if (banInfo.TotalReports > 0) PendingReportNotificationStore.Save(banInfo);
                return;
            }

            app.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (banInfo.IsBanned)
                {
                    PendingBanNotificationStore.Save(banInfo);

                    string msg = PendingBanNotificationStore.BuildBanMessage(banInfo);
                    MessageHelper.ShowPopup(msg, PopupType.Error);

                    PendingBanNotificationStore.Clear();
                    return;
                }

                if (banInfo.TotalReports > 0)
                {
                    PendingReportNotificationStore.Save(banInfo);

                    string msg = PendingReportNotificationStore.BuildMessage(banInfo);
                    MessageHelper.ShowPopup(msg, PopupType.Warning);

                    PendingReportNotificationStore.Clear();
                }
            }));

        }

        public void OnChatMessageReceived(string sender, string message, string timestamp)
            => ChatMessageReceived?.Invoke(sender, message, timestamp);
    }
}
