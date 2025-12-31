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

        public static PublicProfile CurrentProfile
        {
            get
            {
                if (_currentProfile == null)
                {
                    string message = MessageTranslator.GetLocalizedMessage(MessageKeys.SessionNotInitialized);
                    throw new InvalidOperationException(message);
                }

                return _currentProfile;
            }
        }

        public static bool IsLoggedIn => _currentProfile != null;

        public static string SafeUsername => _currentProfile?.Username;

        public static string SafeUsernameNormalized =>
            _currentProfile?.Username?.Trim()?.ToLower();

   
        public static bool IsIntentionalDisconnect { get; private set; }

        public static void MarkIntentionalDisconnect()
        {
            IsIntentionalDisconnect = true;
        }

        public static void ResetIntentionalDisconnect()
        {
            IsIntentionalDisconnect = false;
        }

        public static void Initialize(PublicProfile profile, LoginServiceClient client, ILoginServiceCallback callback)
        {
            ResetIntentionalDisconnect();

            _currentProfile = profile ?? throw new ArgumentNullException(nameof(profile));
            LoginClient = client ?? throw new ArgumentNullException(nameof(client));
            CallbackHandler = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        public static void DisconnectSafely()
        {
          
            MarkIntentionalDisconnect();

            string username = SafeUsername;

            try
            {
                if (!string.IsNullOrWhiteSpace(username) && SessionClient != null)
                {
                    try
                    {
                        if (SessionClient.State == CommunicationState.Opened)
                        {
                            SessionClient.Unsubscribe(username);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
            finally
            {
                Clear();
            }
        }

        public static void Clear()
        {
          
            MarkIntentionalDisconnect();

            CloseClientSafely(LoginClient);
            CloseClientSafely(SessionClient);

            LoginClient = null;
            SessionClient = null;
            CallbackHandler = null;
            _currentProfile = null;

         
            ResetIntentionalDisconnect();
        }

        public static void ClearForced()
        {
     
            MarkIntentionalDisconnect();

            AbortSafely(LoginClient);
            AbortSafely(SessionClient);

            LoginClient = null;
            SessionClient = null;
            CallbackHandler = null;
            _currentProfile = null;

       
            ResetIntentionalDisconnect();
        }

        private static void AbortSafely(ICommunicationObject client)
        {
            if (client == null)
            {
                return;
            }

            try
            {
                client.Abort();
            }
            catch
            {
            }
        }

        private static void CloseClientSafely(ICommunicationObject client)
        {
            if (client == null)
            {
                return;
            }

            try
            {
                if (client.State != CommunicationState.Faulted)
                {
                    client.Close();
                }
                else
                {
                    client.Abort();
                }
            }
            catch
            {
                try { client.Abort(); } catch { }
            }
        }
    }
}
