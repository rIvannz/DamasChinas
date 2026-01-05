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
        private static readonly ILogService _log = LogFactory.Create(typeof(DbOutageCoordinator));

        public static void Trip(Exception ex)
        {
            if (Interlocked.CompareExchange(ref _tripped, 1, 0) != 0)
            {
                return;
            }

            try
            {
                _log.Error("[DbOutageCoordinator] DB down detected. Forcing disconnect all.", ex);

         
                TryNotifyTelegram(ex);

 
                SessionManager.ForceDisconnectAll(MessageCode.DatabaseUnavailable);
                GuestSessionCallbackManager.ForceDisconnectAll(MessageCode.DatabaseUnavailable);
            }
            catch (Exception inner)
            {
                _log.Error("[DbOutageCoordinator] Error forcing disconnect all.", inner);
            }
        }

        private static void TryNotifyTelegram(Exception ex)
        {
            try
            {
                TelegramNotifier.Send(
                    string.Format(
                        TelegramNotifier.DbDownTemplate,
                        DateTime.Now.ToString(TelegramNotifier.TimeFormat),
                        ex.GetType().Name));
            }
            catch (Exception notifyEx)
            {
                _log.Error("[DbOutageCoordinator] Telegram notification failed.", notifyEx);
            }
        }
    }
}
