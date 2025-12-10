using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

using DamasChinas_Client.UI.FriendServiceProxy;
using DamasChinas_Client.UI.Utilities;
using DamasChinas_Client.UI.Callbacks;
using DamasChinas_Client.UI.PopUps;

namespace DamasChinas_Client.UI.Pages
{
    public partial class Friends : Page
    {
        public ObservableCollection<FriendList> FriendsList { get; }
            = new ObservableCollection<FriendList>();

        public Friends()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            // ------- EVENTOS DE SESIÓN -------
            SessionCallbackHandler.PlayerConnectedEvent += OnPlayerConnected;
            SessionCallbackHandler.PlayerDisconnectedEvent += OnPlayerDisconnected;
            SessionCallbackHandler.PlayerInGameEvent += OnPlayerInGame;
            SessionCallbackHandler.PlayerLeftGameEvent += OnPlayerLeftGame;

            // ------- EVENTOS DE AMIGOS -------
            FriendCallbackHandler.FriendRemovedEvent += OnFriendRemoved;
            FriendCallbackHandler.UserBlockedYouEvent += OnUserBlocked;
            FriendCallbackHandler.UserUnblockedYouEvent += OnUserUnblocked;
            FriendCallbackHandler.FriendRequestAcceptedEvent += OnFriendAccepted;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadFriends();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            SessionCallbackHandler.PlayerConnectedEvent -= OnPlayerConnected;
            SessionCallbackHandler.PlayerDisconnectedEvent -= OnPlayerDisconnected;
            SessionCallbackHandler.PlayerInGameEvent -= OnPlayerInGame;
            SessionCallbackHandler.PlayerLeftGameEvent -= OnPlayerLeftGame;

            FriendCallbackHandler.FriendRemovedEvent -= OnFriendRemoved;
            FriendCallbackHandler.UserBlockedYouEvent -= OnUserBlocked;
            FriendCallbackHandler.UserUnblockedYouEvent -= OnUserUnblocked;
            FriendCallbackHandler.FriendRequestAcceptedEvent -= OnFriendAccepted;
        }

        // ============================================================
        // Cargar lista inicial
        // ============================================================
        private void LoadFriends()
        {
            try
            {
                var callback = new FriendCallbackHandler();
                var context = new InstanceContext(callback);

                using (var client = new FriendServiceClient(context))
                {
                    var friends = client.GetFriends(ClientSession.SafeUsernameNormalized);

                    FriendsList.Clear();

                    if (friends == null) return;

                    foreach (var f in friends)
                    {
                        string avatar = string.IsNullOrWhiteSpace(f.Avatar)
                            ? "avatarIcon.png"
                            : f.Avatar;

                        FriendsList.Add(new FriendList
                        {
                            Username = f.Username,
                            AvatarFile = avatar,
                            AvatarSource = PathProvider.LoadAvatar(avatar),
                            Status = f.ConnectionState ? FriendStatus.Online : FriendStatus.Offline
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Friends.LoadFriends] {ex.Message}");
            }
        }

        // ============================================================
        // EVENTOS DE PRESENCIA
        // ============================================================
        private void OnPlayerConnected(string username)
        {
            var f = FriendsList.FirstOrDefault(x =>
                x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (f != null)
                Dispatcher.Invoke(() => f.Status = FriendStatus.Online);
        }

        private void OnPlayerDisconnected(string username)
        {
            var f = FriendsList.FirstOrDefault(x =>
                x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (f != null)
                Dispatcher.Invoke(() => f.Status = FriendStatus.Offline);
        }

        private void OnPlayerInGame(string username)
        {
            var f = FriendsList.FirstOrDefault(x =>
                x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (f != null)
                Dispatcher.Invoke(() => f.Status = FriendStatus.InGame);
        }

        private void OnPlayerLeftGame(string username)
        {
            var f = FriendsList.FirstOrDefault(x =>
                x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (f != null)
                Dispatcher.Invoke(() => f.Status = FriendStatus.Online);
        }

        // ============================================================
        // EVENTOS DE AMIGOS (CALLBACK)
        // ============================================================
        private void OnFriendRemoved(string username)
        {
            var friend = FriendsList.FirstOrDefault(f =>
                f.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (friend != null)
            {
                Dispatcher.Invoke(() => FriendsList.Remove(friend));
            }
        }

        private void OnUserBlocked(string username)
        {
            OnFriendRemoved(username);
        }

        private void OnUserUnblocked(string username)
        {
            LoadFriends();
        }

        private void OnFriendAccepted(string username)
        {
            LoadFriends();
        }

        // ============================================================
        // BOTONES
        // ============================================================
        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void OnViewProfileClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is FriendList friend)
            {
                try
                {
                    var callback = new FriendCallbackHandler();
                    var context = new InstanceContext(callback);
                    using (var client = new FriendServiceClient(context))
                    {
                        var profile = client.GetFriendPublicProfile(friend.Username);
                        NavigationService?.Navigate(new ProfileFriend(profile));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Friends.OnViewProfileClick] {ex.Message}");
                }
            }
        }

        private void OnChatClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement el && el.DataContext is FriendList friend)
            {
                new ChatWindow(friend.Username).Show();
            }
        }

        private void OnAddFriendClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddFriend());
        }

        private void OnViewPendingRequestsClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new PendingFriendRequests());
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SelectLanguage());
        }

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ConfiSound());
        }
    }
}
