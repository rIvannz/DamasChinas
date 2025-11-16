using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI.Pages
{
    public partial class MenuGuest : Page
    {
        public MenuGuest()
        {
            InitializeComponent();
        }

     

        private void OnJoinPartyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_GuestFeatureOnly"),
                    "info"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuGuest.OnJoinPartyClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }

     
   

        private void OnHowToPlayClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_TutorialUnavailable"),
                    "info"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuGuest.OnHowToPlayClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }

       

        private void OnStatisticsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_GuestStatsUnavailable"),
                    "info"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuGuest.OnStatisticsClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
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
                Debug.WriteLine($"[MenuGuest.OnSoundClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuGuest.OnSoundClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }


        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new MainWindow());
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[MenuGuest.OnBackClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuGuest.OnBackClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
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
                Debug.WriteLine($"[MenuGuest.OnLanguageClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MenuGuest.OnLanguageClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }
    }
}
