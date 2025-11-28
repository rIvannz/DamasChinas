using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using DamasChinas_Client.UI.FriendServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System.Windows.Navigation;
using DamasChinas_Client.UI.Models;
using System;
using System.Diagnostics;
using System.ServiceModel;

namespace DamasChinas_Client.UI.Pages
{
    public partial class Friends : Page
    {
        public ObservableCollection<FriendList> FriendsList { get; } = new ObservableCollection<FriendList>();

        public Friends()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += Friends_Loaded;
        }

        private void Friends_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Friends_Loaded;
            LoadFriends();
        }

        private void LoadFriends()
        {
            try
            {
                using (var client = new FriendServiceClient())
                {
                    var friends = client.GetFriends(ClientSession.safeUsername);

                    FriendsList.Clear();

                    foreach (var friend in friends)
                    {
                        FriendsList.Add(new FriendList
                        {
                            Username = friend.Username,
                            EnLinea = friend.ConnectionState,
                            Avatar = PathProvider.GetPackUri("Assets/Icons/avatarIcon.png").ToString()
                        });
                    }
                }
            }
            catch (EndpointNotFoundException )
            {
                MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
            }
            catch (TimeoutException )
            {
                MessageHelper.ShowPopup(MessageKeys.NetworkLatency, PopupType.Error);
            }
            catch (FaultException )
            {
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
            catch (System.Net.WebException )
            {
                MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
            }
            catch (Exception )
            {
                MessageHelper.ShowPopup(MessageKeys.FriendsLoadError, PopupType.Error);
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
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
            catch (Exception )
            {
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

        private void OnViewProfileClick(object sender, RoutedEventArgs e)
{
    try
    {
        if (sender is FrameworkElement element &&
            element.DataContext is FriendList friend)
        {
            var profilePage = new ProfileUser(friend.Username);

            NavigationService?.Navigate(profilePage);
        }
    }
    catch (Exception )
    {
        MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
    }
}



        private void OnChatClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement element &&
                    element.DataContext is FriendList friend)
                {
                    var chatWindow = new ChatWindow(friend.Username);
                    chatWindow.Show();
                }
            }
            catch (InvalidOperationException )
            {
                MessageHelper.ShowPopup(MessageKeys.ChatOpenError, PopupType.Error);
            }

        }

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ConfiSound());
            }
            catch (InvalidOperationException )
            {
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
            catch (Exception )
            {
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

        private void OnAddFriendClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new AddFriend());
            }
            catch (Exception )
            {
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }

        private void OnViewPendingRequestsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new PendingFriendRequests());
            }
            catch (Exception )
            {
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new SelectLanguage());
            }
            catch (InvalidOperationException )
            {
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }
    }
}
