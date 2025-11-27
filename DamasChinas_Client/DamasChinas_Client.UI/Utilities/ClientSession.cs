using System;
using System.ServiceModel;
using DamasChinas_Client.UI.LogInServiceProxy;

namespace DamasChinas_Client.UI.Utilities
{
    /// <summary>
    /// Maneja la sesión del cliente, incluyendo:
    ///  - Perfil del jugador
    ///  - Cliente WCF (LoginServiceClient)
    ///  - Callback
    ///  - Canal duplex para recibir mensajes
    /// </summary>
    public static class ClientSession
    {
        private static PublicProfile _currentProfile;

        public static LoginServiceClient LoginClient { get; private set; }

   
        public static ILoginServiceCallback CallbackHandler { get; private set; }

        public static PublicProfile CurrentProfile
        {
            get
            {
                if (_currentProfile == null)
                    throw new InvalidOperationException("No hay una sesión activa. Inicia sesión primero.");

                return _currentProfile;
            }
        }

        public static bool IsLoggedIn => _currentProfile != null;


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
            _currentProfile = null;

            try
            {
                if (LoginClient != null)
                {
                    if (LoginClient.State != CommunicationState.Faulted)
                        LoginClient.Close();
                    else
                        LoginClient.Abort();
                }
            }
            catch
            {
                LoginClient?.Abort();
            }

            LoginClient = null;
            CallbackHandler = null;
        }
    }
}
