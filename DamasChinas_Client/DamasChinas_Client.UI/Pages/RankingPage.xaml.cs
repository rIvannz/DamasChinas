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
                            _items.Add(new RankingItemViewModel
                            {
                                Position = position,
                                Username = entry.Username,
                                AvatarFile = string.IsNullOrWhiteSpace(entry.AvatarFile)
                                    ? DefaultAvatarFile
                                    : entry.AvatarFile,
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
                Debug.WriteLine($"[RankingPage.LoadRanking - Comm] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.RankingUnavailable, PopupType.Info);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RankingPage.LoadRanking - General] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

  
        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RankingPage.OnBackClick] {ex.Message}");
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
            catch (Exception ex)
            {
                Debug.WriteLine($"[RankingPage.OnSoundClick] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new SelectLanguage());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RankingPage.OnLanguageClick] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }

        private bool IsFriend(string username)
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
            catch (EndpointNotFoundException ex)
            {
                Debug.WriteLine($"[RankingPage.IsFriend - Endpoint] {ex.Message}");
                return false;
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"[RankingPage.IsFriend - Timeout] {ex.Message}");
                return false;
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[RankingPage.IsFriend - Comm] {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RankingPage.IsFriend - General] {ex.Message}");
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
            catch (EndpointNotFoundException ex)
            {
                Debug.WriteLine($"[RankingPage.OnViewProfileClick - Endpoint] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.ProfileOpenError, PopupType.Error);
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"[RankingPage.OnViewProfileClick - Timeout] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.ProfileOpenError, PopupType.Error);
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[RankingPage.OnViewProfileClick - Comm] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.ProfileOpenError, PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RankingPage.OnViewProfileClick - General] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }




        private sealed class RankingItemViewModel
        {
            public int Position { get; set; }

            public string PositionText => $"#{Position}";

            public string Username { get; set; }

            public string AvatarFile { get; set; }

            public int MatchesPlayed { get; set; }

            public int Wins { get; set; }

            public int Loses { get; set; }

            public ImageSource AvatarSource
            {
                get
                {
                    string file = string.IsNullOrWhiteSpace(AvatarFile)
                        ? "avatarIcon.png"
                        : AvatarFile;

                    return PathProvider.LoadAvatar(file);
                }
            }
        }


    }
}
