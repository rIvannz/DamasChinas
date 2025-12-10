using System;
using System.ServiceModel;
using DamasChinas_Client.UI.Callbacks;
using DamasChinas_Client.UI.FriendServiceProxy;

namespace DamasChinas_Client.UI.Utilities
{
    public static class FriendNotificationManager
    {
        private static FriendServiceClient _client;
        private static FriendCallbackHandler _callback;

        public static bool IsInitialized => _client != null;

        public static void Initialize(string username)
        {
            if (IsInitialized)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("username no puede ser nulo o vacío", nameof(username));
            }

            _callback = new FriendCallbackHandler();
            var context = new InstanceContext(_callback);

            _client = new FriendServiceClient(context, "NetTcpBinding_IFriendService");

            try
            {
                _client.SubscribeFriendEvents(username);
            }
            catch
            {
                // Si falla la suscripción, limpiamos para no dejar el cliente medio vivo
                try
                {
                    if (_client.State != CommunicationState.Faulted)
                    {
                        _client.Close();
                    }
                    else
                    {
                        _client.Abort();
                    }
                }
                catch
                {
                    _client?.Abort();
                }

                _client = null;
                _callback = null;
                throw;
            }
        }

        // Para mantener compatibilidad con tu código actual
        public static FriendServiceClient GetClient()
        {
            return _client;
        }

        // Si quieres usarlo como propiedad
        public static FriendServiceClient Client => _client;

        public static void Shutdown(string username)
        {
            if (!IsInitialized)
            {
                return;
            }

            try
            {
                _client.UnsubscribeFriendEvents(username);

                if (_client.State != CommunicationState.Faulted)
                {
                    _client.Close();
                }
                else
                {
                    _client.Abort();
                }
            }
            catch
            {
                _client?.Abort();
            }
            finally
            {
                _client = null;
                _callback = null;
            }
        }
    }
}
