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

        public static bool ForceAvatarRefresh = false;

        private readonly PublicProfile _profile;
        private readonly int _userId;

        public MenuRegisteredPlayer(PublicProfile profile)
        {
            InitializeComponent();

            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _userId = 1;

            txtUsername.Text = _profile.Username;

          
            Loaded += OnPageLoaded;
            LoadAvatar();
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            if (ForceAvatarRefresh)
            {
                LoadAvatar();
                ForceAvatarRefresh = false;
            }
        }

    
        private void LoadAvatar()
        {
            try
            {
                string avatar = ClientSession.CurrentProfile.AvatarFile;

                if (!string.IsNullOrWhiteSpace(avatar))
                {
                    AvatarImage.Source = PathProvider.LoadAvatar(avatar);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.LoadAvatar] {ex.Message}");
            }
        }

        private void OnAvatarClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var profilePage = new ProfilePlayer();
                NavigationService?.Navigate(profilePage);
            }
            catch (InvalidOperationException)
            {
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
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
            catch (CommunicationException)
            {
                MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
            }
            catch (TimeoutException)
            {
                MessageHelper.ShowPopup(MessageKeys.NetworkLatency, PopupType.Error);
            }
        }

        private void OnJoinPartyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var joinPartyPage = new JoinParty(_userId, _profile.Username);
                NavigationService?.Navigate(joinPartyPage);
            }
            catch (InvalidOperationException)
            {
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
            catch (Exception)
            {
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
                NavigationService?.Navigate(new Friends());
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
