using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Logic;
using System;
using System.ServiceModel;

namespace DamasChinas_Server.Services
{
    [DbGuardBehavior]
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class SessionService : ISessionService
    {
        public void Subscribe(string username)
        {
            var callback = OperationContext.Current.GetCallbackChannel<ISessionCallback>();


            SessionManager.AddSession(username, callback);


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
                  
                    }
                }
            });
        }

        public void Unsubscribe(string username)
        {
    
            SessionManager.RemoveSession(username);

   
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
                    
                    }
                }
            });
        }
    }
}
