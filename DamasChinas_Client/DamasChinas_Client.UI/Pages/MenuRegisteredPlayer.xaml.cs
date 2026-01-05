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
        private static bool ForceAvatarRefresh = false;

        private readonly PublicProfile _profile;
        private int _userId;

       
        private string CurrentUsername => ClientSession.SafeUsername;

        public MenuRegisteredPlayer(PublicProfile profile)
        {
            InitializeComponent();

            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _userId = profile.IdUser;

          
            txtUsername.Text = CurrentUsername;

            try
            {
                if (!FriendNotificationManager.IsInitialized)
                {
              
                    FriendNotificationManager.Initialize(CurrentUsername);
                }
            }
            catch (CommunicationException ex)

            {
                Debug.WriteLine($"[MRP.MenuRegisteredPlayer] {ex}");
                MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
            }

            Loaded += OnPageLoaded;
            LoadAvatar();
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
         
            txtUsername.Text = CurrentUsername;

            if (ForceAvatarRefresh)
            {
                LoadAvatar();
                SetForceAvatarRefresh(false);
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
              
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Warning);
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
            catch (CommunicationException ex)

            {
                Debug.WriteLine($"[MRP.LoadAvatar] {ex}");
               
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Warning);
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
                Debug.WriteLine($"[MRP.OnHowToPlayClick.fail] {ex}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
            catch (CommunicationException ex) 
            { 

                Debug.WriteLine($"[MRP.OnHowToPlayClick.fail] {ex}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Warning);
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
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[MRP.OnstatickClick.fail] {ex}");
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

                string username = CurrentUsername;

                var result = lobbyManager.CreateLobby(username, request);

                if (result.Success)
                {
                    var snapshot = lobbyManager.GetCurrentLobby(username);

                    if (snapshot == null)
                    {
                        MessageHelper.ShowPopup(MessageKeys.MatchCreationFailed, PopupType.Error);
                        return;
                    }

                    NavigationService?.Navigate(new PreLobby(snapshot, username, _userId));
                }
                else
                {
                    MessageHelper.ShowFromResult(result);
                }
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[MRP.OnCreateGameClick.fail] {ex}");
                MessageHelper.ShowPopup(MessageKeys.MatchCreationFailed, PopupType.Error);
            }
        }

        private void OnJoinPartyClick(object sender, RoutedEventArgs e)
        {
            try
            {
    
                NavigationService?.Navigate(new JoinParty(_userId, CurrentUsername));
            }
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.JoinPartyOpenError, PopupType.Error);
            }
        }

        public static void SetForceAvatarRefresh(bool value)
        {
            ForceAvatarRefresh = value;
        }
    }
}
