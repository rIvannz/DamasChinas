using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DamasChinas_Client.UI.FriendServiceProxy;   // <-- Solo el correcto
using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI.Pages
{
    public partial class ProfileFriend : Page
    {
        private readonly PublicFriendProfile _friendProfile;
        private const string DefaultAvatarFile = "avatar1.png";

        public ProfileFriend(PublicFriendProfile friendProfile)
        {
            InitializeComponent();

            _friendProfile = friendProfile
                             ?? throw new ArgumentNullException(nameof(friendProfile));

            LoadFriendData();
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfileFriend.OnBackClick] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }

        private void LoadFriendData()
        {
            try
            {
                // ===== DATOS BÁSICOS =====
                UsernameText.Text = _friendProfile.Username;

                if (!string.IsNullOrWhiteSpace(_friendProfile.SocialUrl))
                    SocialUrlText.Text = _friendProfile.SocialUrl;

                // ===== ESTADÍSTICAS =====
                MatchesPlayedText.Text = _friendProfile.MatchesPlayed.ToString();
                WinsText.Text = _friendProfile.Wins.ToString();
                LosesText.Text = _friendProfile.Loses.ToString();

                // ===== AVATAR =====
                string avatar = string.IsNullOrWhiteSpace(_friendProfile.AvatarFile)
                                ? DefaultAvatarFile
                                : _friendProfile.AvatarFile;

                AvatarImage.Source = PathProvider.LoadAvatar(avatar);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfileFriend.LoadFriendData] {ex.Message}");
            }
        }

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ConfiSound());
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SelectLanguage());
        }
    }
}
