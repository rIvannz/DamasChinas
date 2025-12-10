using System;
using System.ServiceModel;
using DamasChinas_Client.UI.LogInServiceProxy;
using DamasChinas_Client.UI.SessionServiceProxy;

namespace DamasChinas_Client.UI.Utilities
{
    public static class ClientSession
    {
        private static PublicProfile _currentProfile;

        public static LoginServiceClient LoginClient { get; private set; }

        public static ILoginServiceCallback CallbackHandler { get; private set; }

        public static SessionServiceClient SessionClient { get; set; }

        // ============================================================
        // PERFIL / SESIÓN
        // ============================================================
        public static PublicProfile CurrentProfile
        {
            get
            {
                if (_currentProfile == null)
                {
                    throw new InvalidOperationException(
                        "No hay una sesión activa. Inicia sesión primero.");
                }

                return _currentProfile;
            }
        }

        public static bool IsLoggedIn => _currentProfile != null;

        public static string safeUsername =>
            _currentProfile == null
                ? null
                : _currentProfile.Username;

        public static string SafeUsernameNormalized =>
            _currentProfile == null
                ? null
                : _currentProfile.Username?.Trim()?.ToLower();

        public static void Initialize(
            PublicProfile profile,
            LoginServiceClient client,
            ILoginServiceCallback callback)
        {
            _currentProfile = profile ?? throw new ArgumentNullException(nameof(profile));
            LoginClient = client ?? throw new ArgumentNullException(nameof(client));
            CallbackHandler = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        public static void Clear()
        {
            // Cerrar LoginClient
            try
            {
                if (LoginClient != null)
                {
                    if (LoginClient.State != CommunicationState.Faulted)
                    {
                        LoginClient.Close();
                    }
                    else
                    {
                        LoginClient.Abort();
                    }
                }
            }
            catch
            {
                LoginClient?.Abort();
            }

            // Cerrar SessionClient
            try
            {
                if (SessionClient != null)
                {
                    if (SessionClient.State != CommunicationState.Faulted)
                    {
                        SessionClient.Close();
                    }
                    else
                    {
                        SessionClient.Abort();
                    }
                }
            }
            catch
            {
                SessionClient?.Abort();
            }

            LoginClient = null;
            SessionClient = null;
            CallbackHandler = null;
            _currentProfile = null;
        }
    }
}
