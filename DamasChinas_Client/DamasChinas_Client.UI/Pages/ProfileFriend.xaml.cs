using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using DamasChinas_Client.UI.FriendServiceProxy;
using DamasChinas_Client.UI.Utilities;
using DamasChinas_Client.UI.PopUps;
using DamasChinas_Client.UI.Callbacks;

namespace DamasChinas_Client.UI.Pages
{
    public partial class ProfileFriend : Page
    {
        private readonly PublicFriendProfile _friendProfile;
        private const string DefaultAvatarFile = "avatar1.png";

        public ProfileFriend(PublicFriendProfile friendProfile)
        {
            InitializeComponent();
            _friendProfile = friendProfile ?? throw new ArgumentNullException(nameof(friendProfile));
            LoadFriendData();
        }

     
        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.GoBack();
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ProfileFriend.OnBackClick] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }

  
        private void LoadFriendData()
        {
            try
            {
                UsernameText.Text = _friendProfile.Username;
                SocialUrlText.Text = _friendProfile.SocialUrl ?? string.Empty;

                MatchesPlayedText.Text = _friendProfile.MatchesPlayed.ToString();
                WinsText.Text = _friendProfile.Wins.ToString();
                LosesText.Text = _friendProfile.Loses.ToString();

                string avatar = string.IsNullOrWhiteSpace(_friendProfile.AvatarFile)
                    ? DefaultAvatarFile
                    : _friendProfile.AvatarFile;

                AvatarImage.Source = PathProvider.LoadAvatar(avatar);
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[ProfileFriend.LoadFriendData] {ex.Message}");
            }
        }


        private void OnRemoveFriendClick(object sender, RoutedEventArgs e)
        {
            var popup = new ConfirmPopupWindow(MessageKeys.ConfirmRemoveFriend);
            popup.ShowDialog();

            if (!popup.Result)
            {
                return;
            }

            try
            {
                using (var client = new FriendServiceClient(
                    new InstanceContext(new FriendCallbackHandler()),
                    "NetTcpBinding_IFriendService"))
                {
                    var result = client.DeleteFriend(
                        ClientSession.SafeUsernameNormalized,
                        _friendProfile.Username);

                    if (!result.Success)
                    {
                        MessageHelper.ShowPopup(
                            MessageTranslator.GetLocalizedMessage(result.Code),
                            PopupType.Warning);
                        return;
                    }

                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage(MessageKeys.FriendRemovedSuccess),
                        PopupType.Success);

                    NavigationService?.GoBack();
                }
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[ProfileFriend.OnRemoveFriendClick] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

 

        private void OnBlockUserClick(object sender, RoutedEventArgs e)
        {
            var popup = new ConfirmPopupWindow(MessageKeys.ConfirmBlockUser);
            popup.ShowDialog();

            if (!popup.Result)
            {
                return;
            }

            try
            {
                using (var client = new FriendServiceClient(
                    new InstanceContext(new FriendCallbackHandler()),
                    "NetTcpBinding_IFriendService"))
                {
                    var result = client.UpdateBlockStatus(
                        ClientSession.SafeUsernameNormalized,
                        _friendProfile.Username,
                        true);

                    if (!result.Success)
                    {
                        MessageHelper.ShowPopup(
                            MessageTranslator.GetLocalizedMessage(result.Code),
                            PopupType.Warning);
                        return;
                    }

                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage(MessageKeys.UserBlockedSuccess),
                        PopupType.Success);

                    NavigationService?.GoBack();
                }
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[ProfileFriend.OnBlockUserClick] {ex.Message}");
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
