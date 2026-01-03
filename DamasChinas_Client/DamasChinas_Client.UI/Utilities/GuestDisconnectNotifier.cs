using System;
using System.Threading;
using System.Windows;
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

        public static void TryNotifyAndGoHome(string messageKeyOrText)
        {
            // ✅ Si tú mismo estás limpiando/cerrando, NO notifiques.
            if (ClientSession.IsIntentionalDisconnect)
            {
                return;
            }

            // Solo 1 vez
            if (Interlocked.Exchange(ref _notified, 1) == 1)
            {
                return;
            }

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                return;
            }

            dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    string msg;

                    if (!string.IsNullOrWhiteSpace(messageKeyOrText) &&
                        (messageKeyOrText.StartsWith("msg_", StringComparison.OrdinalIgnoreCase) ||
                         messageKeyOrText.StartsWith("confirm", StringComparison.OrdinalIgnoreCase) ||
                         messageKeyOrText.StartsWith("Server", StringComparison.OrdinalIgnoreCase)))
                    {
                        msg = MessageTranslator.GetLocalizedMessage(messageKeyOrText);
                    }
                    else
                    {
                        msg = messageKeyOrText;
                    }

                    MessageHelper.ShowPopup(msg, PopupType.Error);
                }
                catch
                {
                    try { MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error); } catch { }
                }

                try
                {
                    ClientSession.ClearForced();
                }
                catch
                {
                }

                AppNavigator.NavigateToRoot(new MainWindow());
            }));
        }
    }
}
