using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using DamasChinas_Client.UI.Pages;

namespace DamasChinas_Client.UI.Utilities
{
    public static class GuestDisconnectNotifier
    {
        private static int _notified;

        public static void Reset()
        {
            Interlocked.Exchange(ref _notified, 0);
        }

        public static void TryNotifyAndGoHome(string messageKeyOrCode)
        {
            if (ClientSession.IsIntentionalDisconnect)
                return;

            if (Interlocked.Exchange(ref _notified, 1) == 1)
                return;

            ClientSession.MarkIntentionalDisconnect();

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
                return;

            dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    string finalKey = NormalizeServerMessageKey(messageKeyOrCode);
                    MessageHelper.ShowPopup(finalKey, PopupType.Error);
                }
                catch
                {
                    try
                    {
                        MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
                    }
                    catch { }
                }

                try { ClientSession.ClearForced(); } catch { }

                AppNavigator.NavigateToRoot(new MainWindow());
            }), DispatcherPriority.Normal);
        }

        private static string NormalizeServerMessageKey(string codeOrKey)
        {
            if (string.IsNullOrWhiteSpace(codeOrKey))
            {
                return MessageKeys.ServerUnavailable; 
            }

            string trimmed = codeOrKey.Trim();

      
            if (trimmed.StartsWith("msg_", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }

            return "msg_" + trimmed;
        }
    }
}
