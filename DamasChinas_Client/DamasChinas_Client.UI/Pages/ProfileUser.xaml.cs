using System.Windows;
using System.Windows.Controls;

using System;
using System.Diagnostics;
using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI.Pages
{
    public partial class ProfileUser : Page
    {
        public ProfileUser()
        {
            InitializeComponent();
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
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                        PopupType.Warning
                    );
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ProfileUser.OnBackClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    PopupType.Error
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfileUser.OnBackClick - General] {ex.Message}");

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
                Debug.WriteLine($"[ProfileUser.OnSoundClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    PopupType.Error
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfileUser.OnSoundClick - General] {ex.Message}");

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
                Debug.WriteLine($"[ProfileUser.OnLanguageClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    PopupType.Error
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfileUser.OnLanguageClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    PopupType.Error
                );
            }
        }

     

        private void OnDeleteFriendClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_FriendRemoved"),
                    PopupType.Success
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfileUser.OnDeleteFriendClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    PopupType.Error
                );
            }
        }


        private void OnSendMessageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_ChatComingSoon"),
                    PopupType.Info
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProfileUser.OnSendMessageClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    PopupType.Error
                );
            }
        }
    }
}

