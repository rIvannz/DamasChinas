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

        /// <summary>
        /// Cliente WCF que mantiene viva la sesión duplex.
        /// Debe ser global para evitar que el GC destruya la conexión.
        /// </summary>
        public static LoginServiceClient LoginClient { get; private set; }

        /// <summary>
        /// Callback del usuario autenticado.
        /// </summary>
        public static ILoginServiceCallback CallbackHandler { get; private set; }

        /// <summary>
        /// Perfil actual del usuario.
        /// </summary>
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

        /// <summary>
        /// Inicializa sesión guardando:
        ///  - Perfil
        ///  - Cliente WCF
        ///  - Callback
        /// </summary>
        public static void Initialize(
            PublicProfile profile,
            LoginServiceClient client,
            ILoginServiceCallback callback)
        {
            _currentProfile = profile ?? throw new ArgumentNullException(nameof(profile));
            LoginClient = client ?? throw new ArgumentNullException(nameof(client));
            CallbackHandler = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <summary>
        /// Limpia la sesión actual (logout).
        /// </summary>
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
