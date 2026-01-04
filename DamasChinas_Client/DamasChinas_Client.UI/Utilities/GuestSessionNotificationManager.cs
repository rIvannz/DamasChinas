using DamasChinas_Client.UI.Callbacks;
using DamasChinas_Client.UI.GuestSessionServiceProxy;
using System;
using System.Diagnostics;
using System.ServiceModel;

namespace DamasChinas_Client.UI.Utilities
{
    public static class GuestSessionNotificationManager
    {
        private const string BindingName = "NetTcpBinding_IGuestSessionService";

        private static GuestSessionServiceClient _client;
        private static InstanceContext _context;
        private static string _guestNormalized;

        public static bool IsInitialized => _client != null;

        public static void Initialize(string guestUsername)
        {
            if (!ClientSession.IsGuest)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(guestUsername))
            {
                return;
            }
            EnsureAlive(guestUsername);
        }

        public static void Reset()
        {
            try
            {
                if (_client != null && _client.State == CommunicationState.Opened)
                {
                    try
                    {
                        string guest = ClientSession.SafeUsername;
                        if (!string.IsNullOrWhiteSpace(guest))
                        {
                            _client.Unsubscribe(guest);
                        }
                    }
                    catch
                    {
                        Debug.WriteLine($"[GuestSecionManager.Reset.fail]");
                    }
                }
            }
            catch
            {
                Debug.WriteLine($"[GuestSecionManager.Reset.fail]");
            }


            try
            {
                if (_client != null)
                {
                    if (_client.State == CommunicationState.Faulted)
                        _client.Abort();
                    else
                        _client.Close();
                }
            }
            catch
            {
                try {
                    _client?.Abort(); 
                }
                catch 
                {
                    Debug.WriteLine($"[GuestSecionManager.Reset.fail]");
                }
            }
            finally
            {
                _client = null;
                _context = null;
                _guestNormalized = null;

                try 
                {
                    GuestSessionCallbackHandler.ServerMessageReceived -= OnServerMessage;
                }
                catch 
                {
                    Debug.WriteLine($"[GuestSecionManager.Reset.fail]");
                }
            }
        }

        private static void EnsureAlive(string guestUsername)
        {
            string normalized = Normalize(guestUsername);

            if (_client != null &&
                _client.State == CommunicationState.Opened &&
                string.Equals(_guestNormalized, normalized, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (_client != null && IsDead(_client.State))
            {
                try 
                {
                    _client.Abort(); 
                }
                catch 
                {
                    Debug.WriteLine($"[GuestSecionManager.EnsureAlive.fail]");
                }
                _client = null;
                _context = null;
                _guestNormalized = null;
            }

            try
            {
                var callback = new GuestSessionCallbackHandler();

          
                GuestSessionCallbackHandler.ServerMessageReceived -= OnServerMessage;
                GuestSessionCallbackHandler.ServerMessageReceived += OnServerMessage;

                _context = new InstanceContext(callback);
                _client = new GuestSessionServiceClient(_context, BindingName);

                AttachChannelEvents(_client);

                _client.Subscribe(guestUsername);
                _guestNormalized = normalized;

                GuestDisconnectNotifier.Reset();
            }
            catch (Exception ex) when (
                ex is EndpointNotFoundException ||
                ex is CommunicationException ||
                ex is TimeoutException)
            {
                Debug.WriteLine($"[GuestSessionNotificationManager.Initialize] {ex.Message}");

                GuestDisconnectNotifier.TryNotifyAndGoHome(MessageKeys.ServerUnavailable);

                try 
                {
                    _client?.Abort();
                } 
                catch
                {
                    Debug.WriteLine($"[GuestSecionManager.EnsureAlive.fail]");
                }
                _client = null;
                _context = null;
                _guestNormalized = null;
            }
        }

        private static void AttachChannelEvents(ICommunicationObject channel)
        {
            if (channel == null)
            {
                return;
            }
            channel.Faulted += (s, e) =>
                GuestDisconnectNotifier.TryNotifyAndGoHome(MessageKeys.ServerUnavailable);

            channel.Closed += (s, e) =>
                GuestDisconnectNotifier.TryNotifyAndGoHome(MessageKeys.ServerUnavailable);
        }

        private static void OnServerMessage(string code)
        {
       
            string key = NormalizeToMessageKey(code);
            GuestDisconnectNotifier.TryNotifyAndGoHome(key);
        }

        private static string NormalizeToMessageKey(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return MessageKeys.ServerUnavailable;
            }
            if (code.StartsWith("msg_", StringComparison.OrdinalIgnoreCase))
            {
                return code;
            }
            return "msg_" + code.Trim();
        }


        private static bool IsDead(CommunicationState state)
        {
            return state == CommunicationState.Faulted ||
                   state == CommunicationState.Closed ||
                   state == CommunicationState.Closing;
        }

        private static string Normalize(string s)
        {
            return s?.Trim().ToLowerInvariant();
        }
    }
}
