using DamasChinas_Client.UI.Utilities;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace DamasChinas_Client.UI.Pages
{
    public partial class HowToPlay : Page
    {
        public HowToPlay()
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
                        MessageKeys.NavigationError,
                        PopupType.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HowToPlay.OnBackClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageKeys.NavigationError,
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
            catch (Exception ex)
            {
                Debug.WriteLine($"[HowToPlay.OnSoundClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageKeys.NavigationError,
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
            catch (Exception ex)
            {
                Debug.WriteLine($"[HowToPlay.OnLanguageClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageKeys.NavigationError,
                    PopupType.Error
                );
            }
        }
    }
}
