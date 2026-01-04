using System;
using System.Diagnostics;
using System.ServiceModel;
using DamasChinas_Client.UI.Callbacks;
using DamasChinas_Client.UI.FriendServiceProxy;
using static DamasChinas_Client.UI.Utilities.MessageKeys;

namespace DamasChinas_Client.UI.Utilities
{
    public static class FriendNotificationManager
    {
        private const string BindingName = "NetTcpBinding_IFriendService";

        private static FriendServiceClient _client;
        private static string _usernameNormalized;

        public static bool IsInitialized => _client != null;

        public static void Initialize(string username)
        {
            EnsureClientAlive(username);
        }

        public static FriendServiceClient GetOrCreateClient(string username)
        {
            return EnsureClientAlive(username);
        }

        public static FriendServiceClient GetClient()
        {
            return _client;
        }

        public static FriendServiceClient Client => _client;

        public static void Shutdown(string username)
        {
            if (!IsInitialized)
            {
                return;
            }

            try
            {
                string normalized = Normalize(username);

                if (!string.IsNullOrWhiteSpace(normalized) &&
                    normalized.Equals(_usernameNormalized, StringComparison.OrdinalIgnoreCase) &&
                    _client.State == CommunicationState.Opened)
                {
                    _client.UnsubscribeFriendEvents(username);
                }

                CloseClientSafely(_client);
            }
            catch
            {
                AbortSafely(_client);
            }
            finally
            {
                _client = null;
                _usernameNormalized = null;
            }
        }

        public static void Reset()
        {
            try
            {
                AbortSafely(_client);
            }
            finally
            {
                _client = null;
                _usernameNormalized = null;
            }
        }


        private static FriendServiceClient EnsureClientAlive(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                string msg = MessageTranslator.GetLocalizedMessage(InvalidUsername);
                throw new ArgumentException(msg, nameof(username));
            }

            string normalized = Normalize(username);

            if (_client != null && IsDead(_client.State))
            {
                AbortSafely(_client);
                _client = null;
            }

            if (_client == null)
            {
                FriendCallbackHandler callback = new FriendCallbackHandler();
                var context = new InstanceContext(callback);
                _client = new FriendServiceClient(context, BindingName);
            }

            if (!string.Equals(_usernameNormalized, normalized, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    _client.SubscribeFriendEvents(username);
                    _usernameNormalized = normalized;
                }
                catch (CommunicationException ex)
                {
                    Debug.WriteLine($"[FriendNotificationManager.EnsureClientAlive] Subscribe failed: {ex.Message}");

                    AbortSafely(_client);
                    _client = null;
                    _usernameNormalized = null;

                    throw;
                }
            }

            return _client;
        }

        private static string Normalize(string username)
        {
            return username?.Trim()?.ToLowerInvariant();
        }

        private static bool IsDead(CommunicationState state)
        {
            return state == CommunicationState.Faulted ||
                   state == CommunicationState.Closed ||
                   state == CommunicationState.Closing;
        }

        private static void AbortSafely(ICommunicationObject client)
        {
            if (client == null)
            {
                return;
            }

            try 
            { client.Abort();
            }
             catch (CommunicationException ex)
            {
                Debug.WriteLine($"[FriendNotificationManager.AbortSafely faile] Subscribe failed: {ex.Message}");
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
                try 
                { 
                    client.Abort(); 
                }
                catch (CommunicationException ex)
                {
                    Debug.WriteLine($"[FriendNotificationManager.AbortSafely faile] Subscribe failed: {ex.Message}");
                }
            }
        }
    }
}
