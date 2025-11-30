using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DamasChinas_Client.UI.FriendServiceProxy;
using DamasChinas_Client.UI.Models;
using DamasChinas_Client.UI.Utilities;
using DamasChinas_Client.UI;  

namespace DamasChinas_Client.UI.Pages
{
    public partial class Friends : Page
    {
        private const string AvatarBasePath = "Assets/Icons/";
        private const string DefaultAvatarFile = "avatarIcon.png";

        public ObservableCollection<FriendList> FriendsList { get; } =
            new ObservableCollection<FriendList>();

        public Friends()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

          
            LoginCallbackHandler.PlayerDisconnectedEvent += OnPlayerDisconnected;
            LoginCallbackHandler.PlayerConnectedEvent += OnPlayerConnected;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadFriends();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            
            LoginCallbackHandler.PlayerDisconnectedEvent -= OnPlayerDisconnected;
            LoginCallbackHandler.PlayerConnectedEvent -= OnPlayerConnected;
        }

        private static string BuildAvatarUri(string avatarFile)
        {
            string file = string.IsNullOrWhiteSpace(avatarFile)
                ? DefaultAvatarFile
                : avatarFile;

            return PathProvider
                .GetPackUri($"{AvatarBasePath}{file}")
                .ToString();
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
                            EnLinea = friend.ConnectionState,
                            Avatar = BuildAvatarUri(friend.Avatar)
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
        // HANDLERS DE EVENTOS DE SESIÓN
        // ============================================================

        private void OnPlayerDisconnected(string nickname)
        {
         
            Dispatcher.Invoke(LoadFriends);
        }

        private void OnPlayerConnected(string nickname)
        {
            Dispatcher.Invoke(LoadFriends);
        }

        // ============================================================
        // HANDLERS DE UI
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
                    using (var client = new FriendServiceClient())
                    {
                        var profile = client.GetFriendPublicProfile(friend.Username);


                        if (string.IsNullOrWhiteSpace(profile.Username))
                        {
                            profile.Username = friend.Username;
                        }

                        NavigationService?.Navigate(new ProfileFriend(profile));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Friends.OnViewProfileClick] {ex.Message}");
                    MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
                }
            }
        }


        private void OnChatClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is FriendList friend)
            {
                var chatWindow = new ChatWindow(friend.Username);
                chatWindow.Show();
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
