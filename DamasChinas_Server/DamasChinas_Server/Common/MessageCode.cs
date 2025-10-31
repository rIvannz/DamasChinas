using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamasChinas_Server.Common
{
    /// <summary>
    /// Enumeración de códigos de mensaje estándar
    /// intercambiados entre el servidor y el cliente.
    /// Permite la internacionalización en el cliente
    /// sin enviar textos directos desde el servidor.
    /// </summary>
        public enum MessageCode
        {
            Success = 0,
            LoginInvalidCredentials = 1001,
            UserDuplicateEmail = 1002,
            UserNotFound = 1003,
            MatchCreationFailed = 1100,
            ServerUnavailable = 2001,
            NetworkLatency = 2100,
            UnknownError = 9999
        }
    }

