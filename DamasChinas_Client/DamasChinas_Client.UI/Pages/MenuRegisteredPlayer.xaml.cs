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

            try
            {
                if (!FriendNotificationManager.IsInitialized)
                {
                    FriendNotificationManager.Initialize(_profile.Username);
                    Debug.WriteLine("[MenuRegisteredPlayer] FriendService inicializado OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer] Error al iniciar FriendService: {ex.Message}");
            }

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

            try
            {
                var pendingBan = PendingBanNotificationStore.Load();
                if (pendingBan != null && pendingBan.IsBanned)
                {
                    string msg = PendingBanNotificationStore.BuildBanMessage(pendingBan);
                    MessageHelper.ShowPopup(msg, PopupType.Error);
                    PendingBanNotificationStore.Clear();
                }

                var pendingReport = PendingReportNotificationStore.Load();
                if (pendingReport != null && !pendingReport.IsBanned && pendingReport.TotalReports > 0)
                {
                    string msg = PendingReportNotificationStore.BuildMessage(pendingReport);
                    MessageHelper.ShowPopup(msg, PopupType.Warning);
                    PendingReportNotificationStore.Clear();
                }
            }
            catch
            {
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
            try
            {
                NavigationService?.Navigate(new HowToPlay());
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnHowToPlayClick - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnHowToPlayClick - General] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }


        private void OnStatisticsClick(object sender, RoutedEventArgs e)
        {
            if (!ClientSession.IsLoggedIn)
            {
                MessageHelper.ShowPopup(MessageKeys.GuestStatsUnavailable, PopupType.Info);
                return;
            }

            try
            {
        
                if (!ClientSession.IsLoggedIn)
                {
                    MessageHelper.ShowPopup(MessageKeys.GuestStatsUnavailable, PopupType.Info);
                    return;
                }

                NavigationService?.Navigate(new RankingPage());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuRegisteredPlayer.OnStatisticsClick] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.StatsUnavailable, PopupType.Error);
            }
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

      
                var result = lobbyManager.CreateLobby(_profile.Username, request);

                if (result.Success)
                {
          
                    var snapshot = lobbyManager.GetCurrentLobby(_profile.Username);

                    if (snapshot == null)
                    {
                        MessageHelper.ShowPopup(MessageKeys.MatchCreationFailed, PopupType.Error);
                        return;
                    }

        
                    NavigationService?.Navigate(
                        new PreLobby(snapshot, _profile.Username, _userId)
                    );
                }
                else
                {
    
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