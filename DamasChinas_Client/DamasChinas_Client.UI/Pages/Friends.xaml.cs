using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DamasChinas_Client.UI.FriendServiceProxy;
using DamasChinas_Client.UI.Utilities;
using DamasChinas_Client.UI.Callbacks;

namespace DamasChinas_Client.UI.Pages
{
    public partial class Friends : Page
    {
        private const string AvatarBasePath = "Assets/Icons/";
        private const string DefaultAvatarFile = "avatarIcon.png";

        public ObservableCollection<FriendList> FriendsList { get; }
            = new ObservableCollection<FriendList>();


        public Friends()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            SessionCallbackHandler.PlayerConnectedEvent += OnPlayerConnected;

         
            SessionCallbackHandler.PlayerDisconnectedEvent += OnPlayerDisconnected;

         
            SessionCallbackHandler.PlayerInGameEvent += OnPlayerInGame;

         
            SessionCallbackHandler.PlayerLeftGameEvent += OnPlayerLeftGame;
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
        }


        private static string BuildAvatarUri(string avatarFile)
        {
            string f = string.IsNullOrWhiteSpace(avatarFile) ? DefaultAvatarFile : avatarFile;
            return PathProvider.GetPackUri($"{AvatarBasePath}{f}").ToString();
        }

        private void LoadFriends()
        {
            try
            {
                using (var client = new FriendServiceClient())
                {
                    var friends = client.GetFriends(ClientSession.SafeUsernameNormalized);

                    FriendsList.Clear();

                    foreach (var friend in friends)
                    {
                        FriendsList.Add(new FriendList
                        {
                            Username = friend.Username,
                            Avatar = BuildAvatarUri(friend.Avatar),
                            Status = friend.ConnectionState
                                ? FriendStatus.Online
                                : FriendStatus.Offline
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Friends.LoadFriends] {ex.Message}");
            }
        }



        private void OnPlayerConnected(string username)
        {
            var f = FriendsList.FirstOrDefault(x => x.Username == username);
            if (f == null) return;

            Dispatcher.Invoke(() => f.Status = FriendStatus.Online);
        }

        private void OnPlayerDisconnected(string username)
        {
            var f = FriendsList.FirstOrDefault(x => x.Username == username);
            if (f == null) return;

            Dispatcher.Invoke(() => f.Status = FriendStatus.Offline);
        }

        private void OnPlayerInGame(string username)
        {
            var f = FriendsList.FirstOrDefault(x => x.Username == username);
            if (f == null) return;

            Dispatcher.Invoke(() => f.Status = FriendStatus.InGame);
        }

        private void OnPlayerLeftGame(string username)
        {
            var f = FriendsList.FirstOrDefault(x => x.Username == username);
            if (f == null) return;

            Dispatcher.Invoke(() => f.Status = FriendStatus.Online);
        }



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
                    using (var client = new FriendServiceClient())
                    {
                        var profile = client.GetFriendPublicProfile(friend.Username);

                        if (string.IsNullOrWhiteSpace(profile.Username))
                            profile.Username = friend.Username;

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
            if (sender is FrameworkElement element &&
                element.DataContext is FriendList friend)
            {
                var chat = new ChatWindow(friend.Username);
                chat.Show();
            }
        }

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ConfiSound());
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
    }
}
