using DamasChinas_Client.UI.LogInServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Diagnostics;
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
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePlayer.Ctor(default)] {ex.Message}");
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
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePlayer.OnPageLoaded] {ex.Message}");
            }
        }

        // ================= NAVEGACIÓN =================

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationService?.CanGoBack == true)
                {
                    NavigationService.GoBack();
                }
                else
                {
                    MessageHelper.ShowPopup(NavigationError, PopupType.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePlayer.OnBackClick] {ex.Message}");
                MessageHelper.ShowPopup(NavigationError, PopupType.Error);
            }
        }

        private void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            try
            {
                bool confirm = MessageHelper.ShowConfirm("msg_LogoutConfirm");
                if (!confirm)
                    return;

                var username = ClientSession.CurrentProfile?.Username;
                var sessionClient = ClientSession.SessionClient;

                if (!string.IsNullOrWhiteSpace(username) &&
                    sessionClient != null &&
                    sessionClient.State == CommunicationState.Opened)
                {
                    try
                    {
                        sessionClient.Unsubscribe(username);
                        sessionClient.Close();
                    }
                    catch
                    {
                        sessionClient.Abort();
                    }
                }

                ClientSession.Clear();

                NavigationService?.Navigate(new MainWindow());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePlayer.OnLogoutClick] {ex.Message}");
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);
            }
        }

        private void OnChangeDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ChangeData());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePlayer.OnChangeDataClick] {ex.Message}");
                MessageHelper.ShowPopup(ProfileChangeError, PopupType.Error);
            }
        }

        private void OnChangeAvatarClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new SelectAvatar());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePlayer.OnChangeAvatarClick] {ex.Message}");
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);
            }
        }

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ConfiSound());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePlayer.OnSoundClick] {ex.Message}");
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);
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
                Debug.WriteLine($"[ProfilePlayer.OnLanguageClick] {ex.Message}");
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);
            }
        }

        // ================= PERFIL / STATS =================

        private void UpdateProfileDisplay()
        {
            try
            {
                if (_profile == null)
                    return;

                UsernameTextBlock.Text = _profile.Username;
                FullNameTextBlock.Text = $"{_profile.Name} {_profile.LastName}";
                EmailTextBlock.Text = _profile.Email;

                // Estadísticas
                MatchesPlayedTextBlock.Text = _profile.MatchesPlayed.ToString();
                WinsTextBlock.Text = _profile.Wins.ToString();
                LosesTextBlock.Text = _profile.Loses.ToString();

                LoadAvatar(_profile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePlayer.UpdateProfileDisplay] {ex.Message}");
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
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePlayer.LoadAvatar] {ex.Message}");
            }
        }
    }
}
