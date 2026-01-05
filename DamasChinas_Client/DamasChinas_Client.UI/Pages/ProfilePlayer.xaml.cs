using DamasChinas_Client.UI.LogInServiceProxy;
using DamasChinas_Client.UI.RankingServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using static DamasChinas_Client.UI.Utilities.MessageKeys;

namespace DamasChinas_Client.UI.Pages
{
    public partial class ProfilePlayer : Page
    {
        private PublicProfile _profile;
        private const string DefaultAvatarFile = "avatarIcon.png";

        public ProfilePlayer()
        {
            InitializeComponent();
            Loaded += OnPageLoaded;

            try
            {
                if (ClientSession.IsLoggedIn)
                {
                    _profile = ClientSession.CurrentProfile;
                    UpdateProfileDisplay();
                }
                else
                {
                    MessageHelper.ShowPopup(UserProfileNotFound, PopupType.Warning);
                }
            }
            catch
            {
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);
            }
        }

        public ProfilePlayer(PublicProfile profile)
        {
            InitializeComponent();
            Loaded += OnPageLoaded;

            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            UpdateProfileDisplay();
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _profile = ClientSession.CurrentProfile;
                UpdateProfileDisplay();
            }
            catch { 
           
            }
        }

        private void RefreshStatsFromRanking()
        {
            try
            {
                using (var client = new RankingServiceClient(
                    "NetTcpBinding_IRankingService"))
                {
                    var ranking = client.GetTop10Ranking();

                    if (ranking == null || _profile == null)
                        return;

                    var me = ranking
                        .FirstOrDefault(r =>
                            r.Username.Equals(
                                _profile.Username,
                                StringComparison.OrdinalIgnoreCase));

                    if (me == null)
                        return;

                  
                    _profile.MatchesPlayed = me.MatchesPlayed;
                    _profile.Wins = me.Wins;
                    _profile.Loses = me.Loses;
                }
            }
            catch
            {
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);

            }
        }



        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationService?.CanGoBack == true)
                    NavigationService.GoBack();
                else
                    MessageHelper.ShowPopup(NavigationError, PopupType.Warning);
            }
            catch
            {
                MessageHelper.ShowPopup(NavigationError, PopupType.Error);
            }
        }

        private void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!MessageHelper.ShowConfirm("msg_LogoutConfirm"))
                    return;

                ClientSession.MarkIntentionalDisconnect();

                try
                {
                    var username = ClientSession.SafeUsername;
                    var sessionClient = ClientSession.SessionClient;

                    if (!string.IsNullOrWhiteSpace(username) &&
                        sessionClient != null &&
                        sessionClient.State == CommunicationState.Opened)
                    {
                        sessionClient.Unsubscribe(username);
                    }
                }
                catch
                {
                    MessageHelper.ShowPopup(UnknownError, PopupType.Error);

                }

                ClientSession.ClearForced();

                AppNavigator.NavigateToRoot(new MainWindow());
            }
            catch
            {
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);
            }
        }


        private void OnChangeDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ChangeData());
            }
            catch
            {
                MessageHelper.ShowPopup(ProfileChangeError, PopupType.Error);
            }
        }

        private void OnChangeAvatarClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new SelectAvatar());
            }
            catch
            {
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);
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
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);
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
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);
            }
        }

      

        private void OnOpenSocialClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string url = _profile?.SocialUrl;

                if (string.IsNullOrWhiteSpace(url))
                {
                    MessageHelper.ShowPopup("msg_SocialUrlMissing", PopupType.Warning);
                    return;
                }

                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                    url = "https://" + url;

                Process.Start(new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                });
            }
            catch
            {
                MessageHelper.ShowPopup("msg_UrlOpenError", PopupType.Error);
            }
        }



        private void UpdateProfileDisplay()
        {
            try
            {
                if (_profile == null)
                    return;

   
                RefreshStatsFromRanking();

                UsernameTextBlock.Text = _profile.Username;
                FullNameTextBlock.Text = $"{_profile.Name} {_profile.LastName}";
                EmailTextBlock.Text = _profile.Email;

                MatchesPlayedTextBlock.Text = _profile.MatchesPlayed.ToString();
                WinsTextBlock.Text = _profile.Wins.ToString();
                LosesTextBlock.Text = _profile.Loses.ToString();

                SocialUrlTextBlock.Text =
                    string.IsNullOrWhiteSpace(_profile.SocialUrl)
                    ? MessageTranslator.GetLocalizedMessage("msg_SocialUrlMissing")
                    : _profile.SocialUrl;

                LoadAvatar(_profile);
            }
            catch
            {
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);
            }
        }


        private void LoadAvatar(PublicProfile profile)
        {
            try
            {
                string avatarFile = string.IsNullOrWhiteSpace(profile.AvatarFile)
                    ? DefaultAvatarFile
                    : profile.AvatarFile;

                AvatarImage.Source = PathProvider.LoadAvatar(avatarFile);
            }
            catch 
            {
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);

            }
        }
    }
}
