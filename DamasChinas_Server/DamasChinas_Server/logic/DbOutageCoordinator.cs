using System;
using System.Threading;
using DamasChinas_Server.Common;
using DamasChinas_Server.Services;
using DamasChinas_Server.Utilities;
using DamasChinas_Server.Utilidades;

namespace DamasChinas_Server.Logic
{
    public static class DbOutageCoordinator
    {
        private static int _tripped;

        private static readonly ILogService _log =
            LogFactory.Create(typeof(DbOutageCoordinator));

        internal static Action<string, Exception> LogError =
            (msg, ex) => _log.Error(msg, ex);

        internal static Action<string> TelegramSender =
            msg => TelegramNotifier.Send(msg);

        internal static Action<MessageCode> DisconnectSessions =
            code => SessionManager.ForceDisconnectAll(code);

        internal static Action<MessageCode> DisconnectGuests =
            code => GuestSessionCallbackManager.ForceDisconnectAll(code);

        public static void Trip(Exception ex)
        {
            if (Interlocked.CompareExchange(ref _tripped, 1, 0) != 0)
            {
                return;
            }

            try
            {
                LogError(
                    "[DbOutageCoordinator] DB down detected. Forcing disconnect all.",
                    ex
                );

                TryNotifyTelegram(ex);

                DisconnectSessions(MessageCode.DatabaseUnavailable);
                DisconnectGuests(MessageCode.DatabaseUnavailable);
            }
            catch (Exception inner)
            {
                LogError(
                    "[DbOutageCoordinator] Error forcing disconnect all.",
                    inner
                );
            }
        }

        private static void TryNotifyTelegram(Exception ex)
        {
            try
            {
                TelegramSender(
                    string.Format(
                        TelegramNotifier.DbDownTemplate,
                        DateTime.Now.ToString(TelegramNotifier.TimeFormat),
                        ex.GetType().Name
                    )
                );
            }
            catch (Exception notifyEx)
            {
                LogError(
                    "[DbOutageCoordinator] Telegram notification failed.",
                    notifyEx
                );
            }
        }

        internal static void ResetForTests()
        {
            _tripped = 0;
        }
    }
}
