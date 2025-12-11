using System;
using System.ServiceModel;
using DamasChinas_Client.UI.LogInServiceProxy;
using DamasChinas_Client.UI.SessionServiceProxy;
using static DamasChinas_Client.UI.Utilities.MessageKeys;
using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI.Utilities
{
    public static class ClientSession
    {
        private static PublicProfile _currentProfile;

        public static LoginServiceClient LoginClient { get; private set; }

        public static ILoginServiceCallback CallbackHandler { get; private set; }

        public static SessionServiceClient SessionClient { get; set; }

        public static PublicProfile CurrentProfile
        {
            get
            {
                if (_currentProfile == null)
                {
                    string message = MessageTranslator.GetLocalizedMessage(SessionNotInitialized);
                    throw new InvalidOperationException(message);
                }

                return _currentProfile;
            }
        }

        public static bool IsLoggedIn => _currentProfile != null;

        public static string SafeUsername => _currentProfile?.Username;

        public static string SafeUsernameNormalized =>
            _currentProfile?.Username?.Trim()?.ToLower();

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
            CloseClientSafely(LoginClient);
            CloseClientSafely(SessionClient);

            LoginClient = null;
            SessionClient = null;
            CallbackHandler = null;
            _currentProfile = null;
        }

        private static void CloseClientSafely(ICommunicationObject client)
        {
            if (client == null)
                return;

            try
            {
                if (client.State != CommunicationState.Faulted)
                    client.Close();
                else
                    client.Abort();
            }
            catch
            {
                try { client.Abort(); } catch { }
            }
        }
    }
}
