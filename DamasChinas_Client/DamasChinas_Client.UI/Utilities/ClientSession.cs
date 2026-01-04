using DamasChinas_Client.UI.LogInServiceProxy;
using DamasChinas_Client.UI.SessionServiceProxy;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.ServiceModel;

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

        public static bool IsIntentionalDisconnect { get; private set; }

        public static void MarkIntentionalDisconnect()
        {
            IsIntentionalDisconnect = true;
        }

        public static void ResetIntentionalDisconnect()
        {
            IsIntentionalDisconnect = false;
        }

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

        public static void EnsureGuestSession()
        {
            if (_currentProfile != null && IsGuest)
            {
                return;
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

            LoginClient = null;
            SessionClient = null;
            CallbackHandler = null;

            IsGuest = true;

            MarkIntentionalDisconnect();

            try
            {
                ResetAllConnections();
            }
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.UserNotFound, PopupType.Error);
            }

            ResetIntentionalDisconnect();

            try
            {
                GuestDisconnectNotifier.Reset();
                GuestSessionNotificationManager.Initialize(_currentProfile.Username);
            }
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.UserNotFound, PopupType.Error);
            }
        }

        public static bool IsGuestUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }
            if (!username.StartsWith(GuestPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            string tail = username.Substring(GuestPrefix.Length);

            if (tail.Length != 4)
            {
                return false;
            }

            return int.TryParse(tail, out _);
        }

        private static string GenerateGuestUsername()
        {
            int number = GetCryptoInt(0, 10000);
            return $"{GuestPrefix}{number:0000}";
        }

        private static int GetCryptoInt(int minInclusive, int maxExclusive)
        {
            if (minInclusive >= maxExclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            }

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

        public static void ResetAllConnections()
        {
            try
            {
                FriendNotificationManager.Reset();
            }
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.UserNotFound, PopupType.Error);
            }
            try
            {
                LobbySession.Reset();
            }
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.UserNotFound, PopupType.Error);
            }
            try
            {
                GuestSessionNotificationManager.Reset();
            }
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.UserNotFound, PopupType.Error);

            }
        }


        public static void DisconnectSafely()
        {
            MarkIntentionalDisconnect();

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
                        MessageHelper.ShowPopup(MessageKeys.UserNotFound, PopupType.Error);

                    }
                }
            }
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.UserNotFound, PopupType.Error);

            }
            finally
            {
                Clear();
            }
        }

        public static void Clear()
        {
            MarkIntentionalDisconnect();

            try { ResetAllConnections(); }
            catch 
            {
                Debug.WriteLine($"[ClientSeccion.Clear.fail]");

            }

            CloseClientSafely(LoginClient);
            CloseClientSafely(SessionClient);

            LoginClient = null;
            SessionClient = null;
            CallbackHandler = null;
            _currentProfile = null;
            IsGuest = false;

            ResetIntentionalDisconnect();
        }

        public static void ClearForced()
        {
            MarkIntentionalDisconnect();

            try { ResetAllConnections(); } catch
            {
                Debug.WriteLine($"[ClientSeccion.ClearForced.fail]");
            }

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
                MessageHelper.ShowPopup(MessageKeys.UserNotFound, PopupType.Error);
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
                catch
                {
                    MessageHelper.ShowPopup(MessageKeys.UserNotFound, PopupType.Error);
                }
            }
        }
    }
}
