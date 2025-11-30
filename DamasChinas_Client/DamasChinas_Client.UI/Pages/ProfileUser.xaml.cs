using DamasChinas_Client.UI.FriendServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DamasChinas_Client.UI.Pages
{
    public partial class ProfileUser : Page
    {
        private readonly string _username;

        public ProfileUser(string username)
        {
            InitializeComponent();

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            _username = username;
            Loaded += ProfileUser_Loaded;
        }

        private void ProfileUser_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ProfileUser_Loaded;
            LoadGuestProfile();
        }

        private void LoadGuestProfile()
        {
            try
            {
                UsernameTextBlock.Text = _username;


                AvatarImage.Source = PathProvider.LoadAvatar("avatarIcon.png");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfileUser.LoadGuestProfile] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
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
