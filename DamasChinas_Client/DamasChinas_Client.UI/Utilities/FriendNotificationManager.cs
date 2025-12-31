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
        private static FriendCallbackHandler _callback;
        private static string _usernameNormalized;

        public static bool IsInitialized => _client != null;

        public static void Initialize(string username)
        {
            // Initialize ahora solo asegura cliente vivo + suscripción.
            EnsureClientAlive(username);
        }

        /// <summary>
        /// ÚSALO EN TODOS LADOS en vez de GetClient().
        /// Garantiza que el proxy esté vivo y suscrito.
        /// </summary>
        public static FriendServiceClient GetOrCreateClient(string username)
        {
            return EnsureClientAlive(username);
        }

        /// <summary>
        /// Mantengo GetClient por compatibilidad, pero OJO:
        /// Solo regresa el proxy actual (puede estar muerto).
        /// Idealmente migra todo a GetOrCreateClient().
        /// </summary>
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

                // Solo intenta desuscribir si coincide y el canal está abierto.
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
                _callback = null;
                _usernameNormalized = null;
            }
        }

        /// <summary>
        /// Reset duro: úsalo cuando el server se cae, logout, ban, etc.
        /// No intenta Unsubscribe (porque normalmente el canal ya está Faulted).
        /// </summary>
        public static void Reset()
        {
            try
            {
                AbortSafely(_client);
            }
            catch { }
            finally
            {
                _client = null;
                _callback = null;
                _usernameNormalized = null;
            }
        }

        // ============================================================
        // CORE
        // ============================================================
        private static FriendServiceClient EnsureClientAlive(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                string msg = MessageTranslator.GetLocalizedMessage(InvalidUsername);
                throw new ArgumentException(msg, nameof(username));
            }

            string normalized = Normalize(username);

            // 1) Si existe y está muerto => destrúyelo
            if (_client != null && IsDead(_client.State))
            {
                AbortSafely(_client);
                _client = null;
                _callback = null;
            }

            // 2) Si no existe => crea uno nuevo
            if (_client == null)
            {
                _callback = new FriendCallbackHandler();
                var context = new InstanceContext(_callback);
                _client = new FriendServiceClient(context, BindingName);
            }

            // 3) Si cambió el usuario o nunca se suscribió => suscribe (si está abierto o se puede abrir)
            // Normalmente el constructor ya deja el canal listo; si no, WCF abre al llamar.
            if (!string.Equals(_usernameNormalized, normalized, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    _client.SubscribeFriendEvents(username);
                    _usernameNormalized = normalized;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[FriendNotificationManager.EnsureClientAlive] Subscribe failed: {ex.Message}");

                    // Este proxy ya quedó sospechoso: lo matamos para que el siguiente intento cree otro.
                    AbortSafely(_client);
                    _client = null;
                    _callback = null;
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

            try { client.Abort(); } catch { }
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
