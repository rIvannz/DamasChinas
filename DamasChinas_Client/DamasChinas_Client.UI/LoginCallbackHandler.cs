using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DamasChinas_Client.UI.LogInServiceProxy;

namespace DamasChinas_Client.UI
{
    public class LoginCallbackHandler : ILoginServiceCallback
    {
        public event Action<PublicProfile> LoginSuccess;
        public event Action<MessageCode> LoginError;

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

