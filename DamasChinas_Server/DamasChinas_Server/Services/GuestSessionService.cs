using DamasChinas_Server.Logic;
using DamasChinas_Server.Utilities;
using DamasChinas_Shared.Contracts;
using System.ServiceModel;

namespace DamasChinas_Server.Services
{
    [DbGuardBehavior]
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
