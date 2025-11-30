using DamasChinas_Server.Interfaces;
using System;
using System.ServiceModel;

namespace DamasChinas_Server.Services
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class SessionService : ISessionService
    {
        public void Subscribe(string username)
        {
            var callback = OperationContext.Current.GetCallbackChannel<ISessionCallback>();

            // Guardar sesión
            SessionManager.AddSession(username, callback);

            // Notificar a todos los demás
            SessionManager.ForEachSession((otherUsername, cb) =>
            {
                if (!otherUsername.Equals(username, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        cb.PlayerConnected(username);
                    }
                    catch
                    {
                        // Si algún callback falla, ignoramos
                    }
                }
            });
        }

        public void Unsubscribe(string username)
        {
            // Remover sesión
            SessionManager.RemoveSession(username);

            // Notificar a todos los demás
            SessionManager.ForEachSession((otherUsername, cb) =>
            {
                if (!otherUsername.Equals(username, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        cb.PlayerDisconnected(username);
                    }
                    catch
                    {
                        // Ignorar fallos de callbacks
                    }
                }
            });
        }
    }
}
