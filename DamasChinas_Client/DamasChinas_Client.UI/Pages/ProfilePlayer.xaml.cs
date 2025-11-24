using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DamasChinas_Client.UI.AccountManagerServiceProxy;
using System.Diagnostics;
using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI.Pages
{
    public partial class ProfilePlayer : Page
    {
        private PublicProfile _profile;

        public ProfilePlayer()
        {
            InitializeComponent();
        }

        public ProfilePlayer(PublicProfile profile)
        {
            InitializeComponent();
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            UpdateProfileDisplay();
        }

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
                    MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Warning);
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ProfilePlayer.OnBackClick - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePlayer.OnBackClick - General] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

        private void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            try
            {
                bool confirm = MessageHelper.ShowConfirmLogout();
                if (!confirm)
                    return;

                ClientSession.Clear();

                if (NavigationService == null)
                {
                    MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
                    return;
                }

                NavigationService.Navigate(new MainWindow());
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ProfilePlayer.OnLogoutClick - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePlayer.OnLogoutClick - General] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

        private void OnChangeDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ChangeData(_profile));
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ProfilePlayer.OnChangeDataClick - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePlayer.OnChangeDataClick - General] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.ProfileChangeError, PopupType.Error);
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
                Debug.WriteLine($"[ProfilePlayer.OnSoundClick - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePlayer.OnSoundClick - General] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
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
                Debug.WriteLine($"[ProfilePlayer.OnLanguageClick - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePlayer.OnLanguageClick - General] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

        private void UpdateProfileDisplay()
        {
            try
            {
                if (_profile == null)
                    return;

                UsernameTextBlock.Text = _profile.Username;
                FullNameTextBlock.Text = $"{_profile.Name} {_profile.LastName}";
                EmailTextBlock.Text = _profile.Email;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfilePlayer.UpdateProfileDisplay] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }
    }
}

