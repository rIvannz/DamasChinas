using DamasChinas_Client.UI.AccountManagerServiceProxy;
using DamasChinas_Client.UI.FriendServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace DamasChinas_Client.UI.Pages
{
    public partial class ProfileUser : Page
    {
        private readonly string _username;
        private PublicFriendProfile _profile;

        public ProfileUser(string username)
        {
            InitializeComponent();

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            _username = username;

            Loaded += ProfileUser_Loaded;
        }

        private void ProfileUser_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ProfileUser_Loaded;
            LoadProfile();
        }

        private void LoadProfile()
        {
            try
            {
                using (var client = new AccountManagerClient())
                {
                    _profile = client.GetFriendPublicProfile(_username);
                }

                UpdateProfileDisplay();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfileUser.LoadProfile] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

        private void UpdateProfileDisplay()
        {
            try
            {
                if (_profile == null)
                {
                    MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
                    return;
                }

                UsernameTextBlock.Text = _profile.Username;
                FullNameTextBlock.Text = $"{_profile.Name} {_profile.LastName}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfileUser.UpdateProfileDisplay] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new Friends());
            }
            catch (InvalidOperationException)
            {
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }

        private void OnDeleteFriendClick(object sender, RoutedEventArgs e)
        {
            try
            {
                bool confirm = MessageHelper.ShowConfirm("¿Eliminar a este amigo?");
                if (!confirm)
                {
                    return;
                }

                using (var client = new FriendServiceClient())
                {
                    client.DeleteFriend(
                        ClientSession.CurrentProfile.Username,
                        _username
                    );
                }

                MessageHelper.ShowPopup(MessageKeys.FriendRemoved, PopupType.Success);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfileUser.OnDeleteFriendClick] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }


        private void OnBlockUserClick(object sender, RoutedEventArgs e)
        {
            try
            {
                bool confirm = MessageHelper.ShowConfirm("¿Bloquear a este usuario?");
                if (!confirm)
                {
                    return;
                }

                MessageHelper.ShowPopup("Usuario bloqueado", PopupType.Info);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfileUser.OnBlockUserClick] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
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
                Debug.WriteLine($"[ProfileUser.OnSoundClick] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
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
                Debug.WriteLine($"[ProfileUser.OnLanguageClick] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }
    }
}
