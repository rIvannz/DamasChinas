using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DamasChinas_Client.UI.AccountManagerServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System.Diagnostics;
using System.ServiceModel;

namespace DamasChinas_Client.UI.Pages
{
    public partial class MenuRegisteredPlayer : Page
    {
        private readonly PublicProfile _profile;
        private readonly int _userId;

        public MenuRegisteredPlayer(PublicProfile profile)
        {
            InitializeComponent();
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _userId = 1;

            txtUsername.Text = _profile.Username;
        }

        private void OnAvatarClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var profilePage = new ProfilePlayer(_profile);
                NavigationService?.Navigate(profilePage);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnAvatarClick - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnAvatarClick - General] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.ProfileOpenError, PopupType.Error);
            }
        }

        private void OnCreateGameClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var lobbyManager = new LobbyManager();
                var lobby = lobbyManager.CreateLobby(_userId, _profile.Username, false);

                var preLobbyPage = new PreLobby(lobby, _userId, _profile.Username);
                NavigationService?.Navigate(preLobbyPage);
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnCreateGameClick - Communication] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnCreateGameClick - Timeout] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NetworkLatency, PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnCreateGameClick - General] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.CreateLobbyError, PopupType.Error);
            }
        }

        private void OnJoinPartyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var joinPartyPage = new JoinParty(_userId, _profile.Username);
                NavigationService?.Navigate(joinPartyPage);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnJoinPartyClick - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnJoinPartyClick - General] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.JoinPartyOpenError, PopupType.Error);
            }
        }

        private void OnHowToPlayClick(object sender, RoutedEventArgs e)
        {
            MessageHelper.ShowPopup(MessageKeys.TutorialUnavailable, PopupType.Info);
        }

        private void OnStatisticsClick(object sender, RoutedEventArgs e)
        {
            MessageHelper.ShowPopup(MessageKeys.StatsUnavailable, PopupType.Info);
        }

        private void OnFriendsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new Friends(_profile.Username));
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnFriendsClick - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnFriendsClick - General] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.FriendsOpenError, PopupType.Error);
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
                Debug.WriteLine($"[MenuRegisteredPlayer.OnSoundClick - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnSoundClick - General] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
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
                Debug.WriteLine($"[MenuRegisteredPlayer.OnLanguageClick - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnLanguageClick - General] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }
    }
}

