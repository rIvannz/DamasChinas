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

        // FIX Sonar: nickname -> username (match interface)
        public void PlayerInGame(string username)
        {
            PlayerInGameEvent?.Invoke(username);
        }

        // FIX Sonar: nickname -> username (match interface)
        public void PlayerLeftGame(string username)
        {
            PlayerLeftGameEvent?.Invoke(username);
        }

        public void OnForcedLogout(string code)
        {
            var app = Application.Current;
            if (app?.Dispatcher == null)
            {
                return;
            }

            app.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    string resourceKey = MapServerCodeToMessageKey(code);
                    MessageHelper.ShowPopup(resourceKey, PopupType.Error);
                }
                catch
                {
                    MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
                }

                try
                {
                    ClientSession.ClearForced();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SessionCallbackHandler.OnForcedLogout] ClearForced: {ex.Message}");
                }

                AppNavigator.NavigateToRoot(new Pages.MainWindow());
            }));
        }

        private static string MapServerCodeToMessageKey(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return MessageKeys.DatabaseUnavailable;
            }

            if (code.StartsWith("msg_", StringComparison.OrdinalIgnoreCase))
            {
                return code;
            }

            if (code.Equals("DatabaseUnavailable", StringComparison.OrdinalIgnoreCase))
            {
                return MessageKeys.DatabaseUnavailable;
            }

            if (code.Equals("ServerUnavailable", StringComparison.OrdinalIgnoreCase))
            {
                return MessageKeys.ServerUnavailable;
            }

            if (code.Equals("SessionExpired", StringComparison.OrdinalIgnoreCase))
            {
                return MessageKeys.SessionExpired;
            }

            return MessageKeys.UnknownError;
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
