using System;
using System.ServiceModel;
using DamasChinas_Client.UI.Callbacks;
using DamasChinas_Client.UI.FriendServiceProxy;

namespace DamasChinas_Client.UI.Utilities
{
    public static class FriendNotificationManager
    {
        private static FriendServiceClient _client;

        public static void Initialize(string username)
        {
            if (_client != null)
            {
                return; // ya está inicializado
            }

            var callback = new FriendCallbackHandler();
            var context = new InstanceContext(callback);

            // Usa el endpoint del config: NetTcpBinding_IFriendService
            _client = new FriendServiceClient(context, "NetTcpBinding_IFriendService");

            try
            {
                _client.SubscribeFriendEvents(username);
            }
            catch
            {
                // Si algo falla, dejamos el canal nulo
                _client = null;
            }
        }

        public static void Shutdown(string username)
        {
            if (_client == null)
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
                _client.Abort();
            }
            finally
            {
                _client = null;
            }
        }
    }
}
