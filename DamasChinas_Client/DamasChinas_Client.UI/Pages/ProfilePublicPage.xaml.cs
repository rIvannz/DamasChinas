using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using DamasChinas_Client.UI.FriendServiceProxy;
using DamasChinas_Client.UI.Utilities;
using DamasChinas_Client.UI.Callbacks;

namespace DamasChinas_Client.UI.Pages
{
    public partial class ProfilePublicPage : Page
    {
        private readonly string _username;
        private readonly string _avatarFile;
        private readonly int _matches;
        private readonly int _wins;
        private readonly int _loses;

        private const string DefaultAvatarFile = "avatarIcon.png";

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
            try
            {
                UsernameText.Text = _username;

                MatchesPlayedText.Text = _matches.ToString();
                WinsText.Text = _wins.ToString();
                LosesText.Text = _loses.ToString();

                string avatar = string.IsNullOrWhiteSpace(_avatarFile)
                    ? DefaultAvatarFile
                    : _avatarFile;

                AvatarImage.Source = PathProvider.LoadAvatar(avatar);
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[ProfilePublicPage.LoadData] {ex.Message}");
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.GoBack();
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ProfilePublicPage.OnBackClick] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }

        private void OnAddFriendClick(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var client = new FriendServiceClient(
                    new InstanceContext(new FriendCallbackHandler()),
                    "NetTcpBinding_IFriendService"))
                {
                    var result = client.SendFriendRequest(
                        ClientSession.CurrentProfile.Username,
                        _username);

                    MessageHelper.ShowFromResult(result);
                }
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[ProfilePublicPage.AddFriend] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
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
                Debug.WriteLine($"[ProfilePublicPage.OnSoundClick] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
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
                Debug.WriteLine($"[ProfilePublicPage.OnLanguageClick] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }
    }
}
