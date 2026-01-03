using System.ServiceModel;
using DamasChinas_Shared.Contracts;
using DamasChinas_Server.Utilities;

namespace DamasChinas_Server.Services
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class GuestSessionService : IGuestSessionService
    {
        public void Subscribe(string guestUsername)
        {
            var callback =
                OperationContext.Current.GetCallbackChannel<IGuestSessionCallback>();

            GuestSessionCallbackManager.Add(guestUsername, callback);
        }

        public void Unsubscribe(string guestUsername)
        {
            GuestSessionCallbackManager.Remove(guestUsername);
        }
    }
}
