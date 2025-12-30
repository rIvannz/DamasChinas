using DamasChinas_Client.UI.SessionServiceProxy;
using DamasChinas_Client.UI.Utilities;
using DamasChinas_Shared.Contracts.Dtos;
using System;
using System.Windows;

namespace DamasChinas_Client.UI.Callbacks
{
    public class SessionCallbackHandler : ISessionServiceCallback
    {
        public static event Action<string> PlayerConnectedEvent;
        public static event Action<string> PlayerDisconnectedEvent;
        public static event Action SessionExpiredEvent;
        public static event Action<string> PlayerInGameEvent;
        public static event Action<string> PlayerLeftGameEvent;

        public static event Action<BanInfoDto> BanStatusUpdatedEvent;

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

        public void OnBanStatusUpdated(BanInfoDto banInfo)
        {
            if (banInfo == null)
            {
                return;
            }

            BanStatusUpdatedEvent?.Invoke(banInfo);

            var app = Application.Current;
            if (app?.Dispatcher == null)
            {

                if (banInfo.IsBanned)
                {
                    PendingBanNotificationStore.Save(banInfo);
                }
                else if (banInfo.TotalReports > 0)
                {
                    PendingReportNotificationStore.Save(banInfo);
                }

                return;
            }

            app.Dispatcher.Invoke(() =>
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
            });
        }

    }
}
