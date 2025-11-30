using System;
using DamasChinas_Client.UI.LogInServiceProxy;

namespace DamasChinas_Client.UI.Callbacks
{
    public class LoginCallbackHandler : ILoginServiceCallback
    {
        // ============================================================
        // EVENTOS PROPIOS (LOGIN)
        // ============================================================

        public event Action<PublicProfile> LoginSuccess;
        public event Action<MessageCode> LoginError;

        // ============================================================
        // REFERENCIA AL CLIENTE (para quien la necesite)
        // ============================================================

        public LoginServiceClient Client { get; private set; }

        public void AttachClient(LoginServiceClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        // ============================================================
        // IMPLEMENTACIÓN DEL CONTRATO
        // ============================================================

        public void OnLoginSuccess(PublicProfile profile)
        {
            // Solo dispara el evento, la lógica de sesión/navegación
            // vive en Login.xaml.cs
            LoginSuccess?.Invoke(profile);
        }

        public void OnLoginError(MessageCode code)
        {
            LoginError?.Invoke(code);
        }
    }
}
