using DamasChinas_Server.Common;
using DamasChinas_Server.Services;
using DamasChinas_Server.Utilidades;
using DamasChinas_Server.Utilities;
using System;
using System.Threading;

namespace DamasChinas_Server.Logic
{
    public static class DbOutageCoordinator
    {
        private static int _tripped;
        private static readonly ILogService _log = LogFactory.Create(typeof(DbOutageCoordinator));

        public static void Trip(Exception ex)
        {
            if (Interlocked.CompareExchange(ref _tripped, 1, 0) != 0)
                return;

            try
            {
                _log.Error("[DbOutageCoordinator] DB down detected. Forcing disconnect all.", ex);
                TelegramNotifier.Send(
    string.Format(TelegramNotifier.DbDownTemplate,DateTime.Now.ToString(TelegramNotifier.TimeFormat),ex.GetType().Name
    )
);
                SessionManager.ForceDisconnectAll(MessageCode.DatabaseUnavailable);
                GuestSessionCallbackManager.ForceDisconnectAll(MessageCode.DatabaseUnavailable);
            }
            catch (Exception inner)
            {
                _log.Error("[DbOutageCoordinator] Error forcing disconnect all.", inner);
            }
        }

    }
}
