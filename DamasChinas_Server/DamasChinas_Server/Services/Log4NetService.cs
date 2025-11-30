using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace DamasChinas_Server.Common
{
    public class Log4NetService : ILogService
    {
        private readonly ILog _log;

        public Log4NetService(Type forType)
        {
            _log = LogManager.GetLogger(forType);
        }

        public void Info(string message)
        {
            _log.Info(message);
        }

        public void Warn(string message)
        {
            _log.Warn(message);
        }

        public void Error(string message, Exception ex = null)
        {
            if (ex == null)
                _log.Error(message);
            else
                _log.Error(message, ex);
        }
    }
}