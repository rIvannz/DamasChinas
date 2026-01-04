using System;
using System.Threading;
using DamasChinas_Server.Common;
using DamasChinas_Server.Services;
using DamasChinas_Server.Utilities;

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
