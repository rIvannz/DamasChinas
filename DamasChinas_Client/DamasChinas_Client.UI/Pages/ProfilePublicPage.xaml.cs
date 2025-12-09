using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DamasChinas_Client.UI.FriendServiceProxy;
using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI.Pages
{
    public partial class ProfilePublicPage : Page
    {
        private readonly string _username;
        private readonly string _avatarFile;
        private readonly int _matches;
        private readonly int _wins;
        private readonly int _loses;

        public ProfilePublicPage(string username, string avatarFile, int matches, int wins, int loses)
        {
            InitializeComponent();

            _username = username;
            _avatarFile = avatarFile;
            _matches = matches;
            _wins = wins;
            _loses = loses;

            LoadData();
        }

        private void LoadData()
        {
            UsernameText.Text = _username;
            AvatarImage.Source = PathProvider.LoadAvatar(_avatarFile);

            MatchesPlayedText.Text = _matches.ToString();
            WinsText.Text = _wins.ToString();
            LosesText.Text = _loses.ToString();
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try { NavigationService?.GoBack(); }
            catch { MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error); }
        }

        private void OnAddFriendClick(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var client = new FriendServiceClient())
                {
                    var result = client.SendFriendRequest(
                        ClientSession.CurrentProfile.Username,
                        _username
                    );

                    MessageHelper.ShowFromResult(result);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePublicPage.AddFriend] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
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
