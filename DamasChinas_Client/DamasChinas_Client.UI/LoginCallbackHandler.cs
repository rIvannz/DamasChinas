using System;
using DamasChinas_Client.UI.LogInServiceProxy;

namespace DamasChinas_Client.UI
{
    public class LoginCallbackHandler : ILoginServiceCallback
    {
        // ============================================================
        // EVENTOS DE LOGIN (INSTANCIA)
        // ============================================================

        public event Action<PublicProfile> LoginSuccess;
        public event Action<MessageCode> LoginError;

        // ============================================================
        // EVENTOS DE SESIÓN (ESTÁTICOS, GLOBALES)
        // ============================================================

        public static event Action SessionExpiredEvent;
        public static event Action<string> PlayerDisconnectedEvent;
        public static event Action<string> PlayerConnectedEvent;

        // ============================================================
        // IMPLEMENTACIÓN LOGIN
        // ============================================================

        public void OnLoginSuccess(PublicProfile profile)
        {
            LoginSuccess?.Invoke(profile);
        }

        public void OnLoginError(MessageCode code)
        {
            LoginError?.Invoke(code);
        }

        // ============================================================
        // IMPLEMENTACIÓN SESIÓN
        // (estos métodos vienen del contrato ISessionCallback
        // que ahora hereda ILoginServiceCallback)
        // ============================================================

        public void SessionExpired()
        {
            SessionExpiredEvent?.Invoke();
        }

        public void PlayerDisconnected(string nickname)
        {
            PlayerDisconnectedEvent?.Invoke(nickname);
        }

        public void PlayerConnected(string nickname)
        {
            PlayerConnectedEvent?.Invoke(nickname);
        }
    }
}
