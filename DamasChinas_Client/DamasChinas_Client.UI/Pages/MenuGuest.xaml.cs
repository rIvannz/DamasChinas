using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI.Pages
{
    public partial class MenuGuest : Page
    {
        public MenuGuest()
        {
            InitializeComponent();

            try
            {
                ClientSession.EnsureGuestSession();

                // Mostrar el username real del invitado (Guest-####)
                if (txtGuestUsername != null)
                {
                    txtGuestUsername.Text = ClientSession.SafeUsername ?? "Guest";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuGuest.Init] {ex.Message}");
            }
        }

        private void OnJoinPartyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ClientSession.EnsureGuestSession();
                NavigationService?.Navigate(new JoinParty());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuGuest.OnJoinPartyClick] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

        private void OnAvatarClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ClientSession.EnsureGuestSession();
                NavigationService?.Navigate(new ProfileUser(ClientSession.SafeUsername));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuGuest.OnAvatarClick] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }

        private void OnHowToPlayClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_TutorialUnavailable"),
                    PopupType.Info
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuGuest.OnHowToPlayClick] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

        private void OnStatisticsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_GuestStatsUnavailable"),
                    PopupType.Info
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuGuest.OnStatisticsClick] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
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
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new MainWindow());
            }
            catch
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
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.NavigationError, PopupType.Error);
            }
        }
    }
}
