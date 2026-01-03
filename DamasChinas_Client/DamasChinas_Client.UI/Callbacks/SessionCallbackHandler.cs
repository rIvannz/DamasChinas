using DamasChinas_Client.UI.SessionServiceProxy;
using DamasChinas_Client.UI.Utilities;
using DamasChinas_Shared.Contracts.Dtos;
using System;
using System.Diagnostics;
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

        public void PlayerConnected(string nickname)
        {
            PlayerConnectedEvent?.Invoke(nickname);
        }

        public void PlayerDisconnected(string nickname)
        {
            PlayerDisconnectedEvent?.Invoke(nickname);
        }

        public void SessionExpired()
        {
            SessionExpiredEvent?.Invoke();
        }

        public void PlayerInGame(string nickname)
        {
            PlayerInGameEvent?.Invoke(nickname);
        }

        public void PlayerLeftGame(string nickname)
        {
            PlayerLeftGameEvent?.Invoke(nickname);
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

            app.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (banInfo.IsBanned)
                {
                    PendingBanNotificationStore.Save(banInfo);

                    string msg = PendingBanNotificationStore.BuildBanMessage(banInfo);
                    MessageHelper.ShowPopup(msg, PopupType.Error);

                    PendingBanNotificationStore.Clear();

             
                    try
                    {
                        ClientSession.ClearForced();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[SessionCallbackHandler.OnBanStatusUpdated] ClearForced: {ex.Message}");
                    }

                    AppNavigator.NavigateToRoot(new Pages.MainWindow());
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
    }
}
