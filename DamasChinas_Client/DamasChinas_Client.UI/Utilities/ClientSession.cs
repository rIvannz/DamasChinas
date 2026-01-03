using System;
using System.ServiceModel;
using System.Security.Cryptography;
using DamasChinas_Client.UI.LogInServiceProxy;
using DamasChinas_Client.UI.SessionServiceProxy;

namespace DamasChinas_Client.UI.Utilities
{
    public static class ClientSession
    {
        private const string GuestPrefix = "Guest-";
        private const string DefaultGuestAvatar = "avatarIcon.png";

        private static PublicProfile _currentProfile;

        public static LoginServiceClient LoginClient { get; private set; }
        public static ILoginServiceCallback CallbackHandler { get; private set; }
        public static SessionServiceClient SessionClient { get; set; }

        public static bool IsGuest { get; private set; }

        public static PublicProfile CurrentProfile
        {
            get
            {
                if (_currentProfile == null)
                {
                    string message =
                        MessageTranslator.GetLocalizedMessage(
                            MessageKeys.SessionNotInitialized);

                    throw new InvalidOperationException(message);
                }

                return _currentProfile;
            }
        }

        public static bool IsLoggedIn => _currentProfile != null && !IsGuest;

        public static string SafeUsername => _currentProfile?.Username;

        public static string SafeUsernameNormalized =>
            _currentProfile?.Username?.Trim()?.ToLower();

        // ============================================================
        // CONTROL DE DESCONEXIÓN INTENCIONAL
        // ============================================================
        public static bool IsIntentionalDisconnect { get; private set; }

        public static void MarkIntentionalDisconnect()
        {
            IsIntentionalDisconnect = true;
        }

        public static void ResetIntentionalDisconnect()
        {
            IsIntentionalDisconnect = false;
        }

        // ============================================================
        // INICIALIZACIÓN REGISTRADO
        // ============================================================
        public static void Initialize(
            PublicProfile profile,
            LoginServiceClient client,
            ILoginServiceCallback callback)
        {
            ResetIntentionalDisconnect();

            _currentProfile = profile
                ?? throw new ArgumentNullException(nameof(profile));

            LoginClient = client
                ?? throw new ArgumentNullException(nameof(client));

            CallbackHandler = callback
                ?? throw new ArgumentNullException(nameof(callback));

            IsGuest = false;
        }

        // ============================================================
        // SESIÓN INVITADO
        // ============================================================
        public static void EnsureGuestSession()
        {
            if (_currentProfile != null && IsGuest)
            {
                return; // ya existe sesión guest
            }

            string guestName = GenerateGuestUsername();

            _currentProfile = new PublicProfile
            {
                IdUser = 0,
                Username = guestName,
                AvatarFile = DefaultGuestAvatar,
                Name = string.Empty,
                LastName = string.Empty,
                Email = string.Empty,
                SocialUrl = string.Empty,
                MatchesPlayed = 0,
                Wins = 0,
                Loses = 0
            };

            // Invitado NO tiene login/session WCF
            LoginClient = null;
            SessionClient = null;
            CallbackHandler = null;

            IsGuest = true;

            MarkIntentionalDisconnect();

            try { ResetAllConnections(); } catch { }

            // ya que terminaste de limpiar, vuelves a permitir notificaciones reales
            ResetIntentionalDisconnect();

            try
            {
                GuestDisconnectNotifier.Reset();
                GuestSessionNotificationManager.Initialize(_currentProfile.Username);
            }
            catch
            {
            }
        }

        public static bool IsGuestUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            // formato: Guest-#### (4 dígitos)
            if (!username.StartsWith(GuestPrefix, StringComparison.OrdinalIgnoreCase))
                return false;

            string tail = username.Substring(GuestPrefix.Length);
            if (tail.Length != 4)
                return false;

            return int.TryParse(tail, out _);
        }

        private static string GenerateGuestUsername()
        {
            // 0000 - 9999 (compatible con .NET Framework 4.7.2)
            int number = GetCryptoInt(0, 10000);
            return $"{GuestPrefix}{number:0000}";
        }

        // RNG criptográfico sin sesgo (rejection sampling)
        private static int GetCryptoInt(int minInclusive, int maxExclusive)
        {
            if (minInclusive >= maxExclusive)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));

            long diff = (long)maxExclusive - minInclusive;

            using (var rng = RandomNumberGenerator.Create())
            {
                while (true)
                {
                    byte[] bytes = new byte[4];
                    rng.GetBytes(bytes);

                    uint value = BitConverter.ToUInt32(bytes, 0);

                    long max = 1L + uint.MaxValue;
                    long remainder = max % diff;

                    if (value < max - remainder)
                    {
                        return (int)(minInclusive + (value % diff));
                    }
                }
            }
        }

        // ============================================================
        // CONEXIONES
        // ============================================================
        public static void ResetAllConnections()
        {
            try { FriendNotificationManager.Reset(); } catch { }
            try { LobbySession.Reset(); } catch { }

            // NUEVO: canal de invitado
            try { GuestSessionNotificationManager.Reset(); } catch { }
        }

        // ============================================================
        // LOGOUT NORMAL (solo aplica para registrados)
        // ============================================================
        public static void DisconnectSafely()
        {
            MarkIntentionalDisconnect();

            // Si es invitado, solo limpia y ya.
            if (IsGuest)
            {
                Clear();
                return;
            }

            string username = SafeUsername;

            try
            {
                if (!string.IsNullOrWhiteSpace(username) &&
                    SessionClient != null &&
                    SessionClient.State == CommunicationState.Opened)
                {
                    try
                    {
                        SessionClient.Unsubscribe(username);
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

        // ============================================================
        // LIMPIEZA SUAVE
        // ============================================================
        public static void Clear()
        {
            MarkIntentionalDisconnect();

            try { ResetAllConnections(); } catch { }

            CloseClientSafely(LoginClient);
            CloseClientSafely(SessionClient);

            LoginClient = null;
            SessionClient = null;
            CallbackHandler = null;
            _currentProfile = null;
            IsGuest = false;

            ResetIntentionalDisconnect();
        }

        // ============================================================
        // LIMPIEZA FORZADA (BAN / CAÍDA DE SERVER)
        // ============================================================
        public static void ClearForced()
        {
            MarkIntentionalDisconnect();

            try { ResetAllConnections(); } catch { }

            AbortSafely(LoginClient);
            AbortSafely(SessionClient);

            LoginClient = null;
            SessionClient = null;
            CallbackHandler = null;
            _currentProfile = null;
            IsGuest = false;

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
