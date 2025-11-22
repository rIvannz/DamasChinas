using System;
using System.Windows;
using System.Windows.Controls;

using System.Diagnostics;

using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI.Pages
{
  
    public partial class MainWindow : Page
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow.OnExitClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    PopupType.Error
                );
            }
        }

 

        private void OnGoToLoginClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new Login());
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[MainWindow.OnGoToLoginClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    PopupType.Error
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow.OnGoToLoginClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    PopupType.Error
                );
            }
        }

        private void OnGoToSignInClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new SignIn());
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[MainWindow.OnGoToSignInClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    PopupType.Error
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow.OnGoToSignInClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    PopupType.Error
                );
            }
        }

        private void OnPlayAsGuestClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new MenuGuest());
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[MainWindow.OnPlayAsGuestClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    PopupType.Error
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow.OnPlayAsGuestClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    PopupType.Error
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
                Debug.WriteLine($"[MainWindow.OnSoundClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    PopupType.Error
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow.OnSoundClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    PopupType.Error
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
                Debug.WriteLine($"[MainWindow.OnLanguageClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    PopupType.Error
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow.OnLanguageClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    PopupType.Error
                );
            }
        }
    }
}
