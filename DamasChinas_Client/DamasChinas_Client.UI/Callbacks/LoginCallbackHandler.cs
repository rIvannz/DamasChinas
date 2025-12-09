using System;
using DamasChinas_Client.UI.LogInServiceProxy;

namespace DamasChinas_Client.UI.Callbacks
{
    public class LoginCallbackHandler : ILoginServiceCallback
    {
  

        public event Action<PublicProfile> LoginSuccess;
        public event Action<MessageCode> LoginError;

    

        public LoginServiceClient Client { get; private set; }

        public void AttachClient(LoginServiceClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

 

        public void OnLoginSuccess(PublicProfile profile)
        {
   
            LoginSuccess?.Invoke(profile);
        }

        public void OnLoginError(MessageCode code)
        {
            LoginError?.Invoke(code);
        }
    }
}
