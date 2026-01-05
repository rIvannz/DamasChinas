using DamasChinas_Client.UI.Callbacks;
using DamasChinas_Client.UI.FriendServiceProxy;
using DamasChinas_Client.UI.RankingServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DamasChinas_Client.UI.Pages
{
    public partial class RankingPage : Page
    {
        private const string DefaultAvatarFile = "avatar1.png";
        private readonly List<RankingItemViewModel> _items = new List<RankingItemViewModel>();

        public RankingPage()
        {
            InitializeComponent();
            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            LoadRanking();
        }

        private void LoadRanking()
        {
            try
            {
                using (var client = new RankingServiceClient())
                {
                    var data = client.GetTop10Ranking();

                    _items.Clear();

                    if (data != null)
                    {
                        int position = 1;

                        foreach (var entry in data)
                        {
                            string avatarFile = string.IsNullOrWhiteSpace(entry.AvatarFile)
                                ? DefaultAvatarFile
                                : entry.AvatarFile;

                            _items.Add(new RankingItemViewModel
                            {
                                Position = position,
                                Username = entry.Username,
                                AvatarFile = avatarFile,
                                AvatarSource = PathProvider.LoadAvatar(avatarFile),
                                MatchesPlayed = entry.MatchesPlayed,
                                Wins = entry.Wins,
                                Loses = entry.Loses
                            });

                            position++;
                        }
                    }

                    lvRanking.ItemsSource = null;
                    lvRanking.ItemsSource = _items;
                }
            }
            catch (Exception ex) when (
                ex is EndpointNotFoundException ||
                ex is CommunicationException ||
                ex is TimeoutException)
            {
                Debug.WriteLine($"[RankingPage.LoadRanking.fail] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.RankingUnavailable, PopupType.Info);
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.GoBack();
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[RankingPage.OnBackClick.fail] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }

        private void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            LoadRanking();
        }

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ConfiSound());
            }
            catch (Exception ex) when (
                ex is EndpointNotFoundException ||
                ex is CommunicationException ||
                ex is TimeoutException)
            {
                Debug.WriteLine($"[RankingPage.OnSoundClick.fail] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Info);
            }
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new SelectLanguage());
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[RankingPage.OnLanguageClick.fail] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }

        private static bool IsFriend(string username)
        {
            try
            {
                using (var client = new FriendServiceClient(
                    new InstanceContext(new FriendCallbackHandler()),
                    "NetTcpBinding_IFriendService"))
                {
                    var friends = client.GetFriends(ClientSession.CurrentProfile.Username);

                    if (friends == null)
                    {
                        return false;
                    }

                    return friends.Any(f =>
                        string.Equals(f.Username, username, StringComparison.OrdinalIgnoreCase));
                }
            }
            catch (Exception ex) when (
                ex is EndpointNotFoundException ||
                ex is CommunicationException ||
                ex is TimeoutException)
            {
                Debug.WriteLine($"[RankingPage.IsFriend.fail] {ex.Message}");
                return false;
            }
        }

        private void OnViewProfileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is Button button) || !(button.DataContext is RankingItemViewModel vm))
                {
                    return;
                }

                if (string.Equals(vm.Username, ClientSession.CurrentProfile.Username, StringComparison.OrdinalIgnoreCase))
                {
                    NavigationService?.Navigate(new ProfilePlayer());
                    return;
                }

                if (IsFriend(vm.Username))
                {
                    using (var client = new FriendServiceClient(
                        new InstanceContext(new FriendCallbackHandler()),
                        "NetTcpBinding_IFriendService"))
                    {
                        var friendProfile = client.GetFriendPublicProfile(vm.Username);

                        if (friendProfile == null)
                        {
                            MessageHelper.ShowPopup(MessageKeys.ProfileOpenError, PopupType.Error);
                            return;
                        }

                        NavigationService?.Navigate(new ProfileFriend(friendProfile));
                    }
                }
                else
                {
                    NavigationService?.Navigate(
                        new ProfilePublicPage(
                            vm.Username,
                            vm.AvatarFile,
                            vm.MatchesPlayed,
                            vm.Wins,
                            vm.Loses));
                }
            }
            catch (Exception ex) when (
                ex is EndpointNotFoundException ||
                ex is CommunicationException ||
                ex is TimeoutException)
            {
                Debug.WriteLine($"[RankingPage.OnViewProfileClick.fail] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.ProfileOpenError, PopupType.Error);
            }
        }

        private sealed class RankingItemViewModel
        {
            public int Position { get; set; }

            public string Username { get; set; }

            public string AvatarFile { get; set; }

            public ImageSource AvatarSource { get; set; }

            public int MatchesPlayed { get; set; }

            public int Wins { get; set; }

            public int Loses { get; set; }
        }
    }
}
