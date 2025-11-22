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

  
        public Friends(string username)
        {
            InitializeComponent();
            DataContext = this;

            Loaded += Friends_Loaded;
        }

        private void Friends_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Friends_Loaded;

            LoadFriends(ClientSession.CurrentProfile.Username);
        }

        private void LoadFriends(string username)
        {
            try
            {
                using (var client = new FriendServiceClient())
                {
                    var friends = client.GetFriends(username);

                    FriendsList.Clear();

                    foreach (var friend in friends)
                    {
                        FriendsList.Add(new FriendList
                        {
                            Username = friend.Username,
                            EnLinea = friend.ConnectionState,
                            Avatar = "pack://application:,,,/DamasChinas_Client.UI;component/Assets/Icons/avatarIcon.png"
                        });
                    }
                }
            }
            catch (EndpointNotFoundException ex)
            {
                Debug.WriteLine($"[Friends.LoadFriends - EndpointNotFound] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_ServerUnavailable"),
                    PopupType.Error
                );
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"[Friends.LoadFriends - Timeout] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NetworkLatency"),
                    PopupType.Error
                );
            }
            catch (FaultException ex)
            {
                Debug.WriteLine($"[Friends.LoadFriends - FaultException] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    PopupType.Error
                );
            }
            catch (System.Net.WebException ex)
            {
                Debug.WriteLine($"[Friends.LoadFriends - WebException] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_ServerUnavailable"),
                    PopupType.Error
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Friends.LoadFriends - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_FriendsLoadError"),
                    PopupType.Error
                );
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
                Debug.WriteLine($"[Friends.OnBackClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    PopupType.Error
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Friends.OnBackClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    PopupType.Error
                );
            }
        }

        private void OnViewProfileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ProfileUser());
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[Friends.OnViewProfileClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    PopupType.Error
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Friends.OnViewProfileClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    PopupType.Error
                );
            }
        }

     

        private void OnChatClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement element &&
                    element.DataContext is FriendList friend)
                {
                    var chatWindow = new ChatWindow(ClientSession.CurrentProfile, friend.Username);
                    chatWindow.Show();
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[Friends.OnChatClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_ChatOpenError"),
                    PopupType.Error
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Friends.OnChatClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_ChatOpenError"),
                    PopupType.Error
                );
            }
        }

 

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ConfiSound());
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[Friends.OnSoundClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    PopupType.Error
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Friends.OnSoundClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    PopupType.Error
                );
            }
        }

        private void OnAddFriendClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new AddFriend());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Friends.OnAddFriendClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    PopupType.Error);
            }
        }

        private void OnViewPendingRequestsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new PendingFriendRequests());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Friends.OnViewPendingRequestsClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    PopupType.Error);
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
                Debug.WriteLine($"[Friends.OnLanguageClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    PopupType.Error
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Friends.OnLanguageClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    PopupType.Error
                );
            }
        }
    }
}
