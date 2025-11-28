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
        // Eliminado: private PublicProfile _profile;

        public ProfilePlayer()
        {
            InitializeComponent();

            try
            {
                if (ClientSession.safeUsername == null)
                {
                    throw new InvalidOperationException("No hay usuario logueado para abrir el perfil.");
                }

                UpdateProfileDisplay();
            }
            catch (InvalidOperationException)
            {
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
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
            catch (InvalidOperationException)
            {
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }

        private void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            try
            {
                bool confirm = MessageHelper.ShowConfirm("msg_LogoutConfirm");
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
            catch (InvalidOperationException)
            {
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }

        private void OnChangeDataClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new ChangeData());
            }
            catch (InvalidOperationException)
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
            catch (InvalidOperationException)
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
            catch (InvalidOperationException)
            {
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }

        private void UpdateProfileDisplay()
        {
            try
            {
                if (!ClientSession.IsLoggedIn || ClientSession.CurrentProfile == null)
                {
                    MessageHelper.ShowPopup(MessageKeys.UserProfileNotFound, PopupType.Warning);
                    return;
                }

                var profile = ClientSession.CurrentProfile;

                UsernameTextBlock.Text = profile.Username;
                FullNameTextBlock.Text = $"{profile.Name} {profile.LastName}";
                EmailTextBlock.Text = profile.Email;
            }
            catch (Exception)
            {
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }
    }
}
