using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamasChinas_Server.Common
{
    public static class LogFactory
    {
        public static ILogService Create<T>()
        {
            return new Log4NetService(typeof(T));
        }

        public static ILogService Create(Type type)
        {
            return new Log4NetService(type);
        }
    }
}