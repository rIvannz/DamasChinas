using DamasChinas_Client.UI.AccountManagerServiceProxy;
using DamasChinas_Client.UI.LobbyServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

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
            _userId = profile.IdUser;

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
                    AvatarImage.Source = PathProvider.LoadAvatar(avatar);
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
                NavigationService?.Navigate(new ProfilePlayer());
            }
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
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
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ConfiSound());
            }
            catch
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
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }


        // =========================================================
        //  CREATE GAME CORREGIDO
        // =========================================================
        private void OnCreateGameClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var lobbyManager = LobbySession.Manager;

                var request = new CreateLobbyRequest
                {
                    MaxPlayers = 6,
                    Visibility = LobbyVisibility.Public
                };

                // 1. Crear (Retorna OperationResult)
                var result = lobbyManager.CreateLobby(_profile.Username, request);

                if (result.Success)
                {
                    // 2. Obtener Snapshot pasando el username
                    var snapshot = lobbyManager.GetCurrentLobby(_profile.Username);

                    if (snapshot == null)
                    {
                        MessageHelper.ShowPopup(MessageKeys.MatchCreationFailed, PopupType.Error);
                        return;
                    }

                    // 3. Navegar
                    NavigationService?.Navigate(
                        new PreLobby(snapshot, _profile.Username, _userId)
                    );
                }
                else
                {
                    // 4. Mostrar error del server
                    MessageHelper.ShowFromResult(result);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OnCreateGameClick] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.MatchCreationFailed, PopupType.Error);
            }
        }

        private void OnJoinPartyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new JoinParty(_userId, _profile.Username));
            }
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.JoinPartyOpenError, PopupType.Error);
            }
        }
    }
}