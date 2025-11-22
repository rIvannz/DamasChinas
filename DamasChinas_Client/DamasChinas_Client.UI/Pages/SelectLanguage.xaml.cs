using System.Windows;
using System.Windows.Controls;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Diagnostics;


namespace DamasChinas_Client.UI.Pages
{
    public partial class SelectLanguage : Page
    {
        public SelectLanguage()
        {
            InitializeComponent();
        }

        private void OnApplyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    string languageCode = selectedItem.Tag.ToString();
                    LanguageManager.ChangeLanguage(languageCode);

                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage("applyLanguage"),
                        PopupType.Success
                    );
                }
                else
                {
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage("msg_SelectLanguageFirst"),
                        PopupType.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SelectLanguage.OnApplyClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    PopupType.Error
                );
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
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                        PopupType.Warning                    );
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[SelectLanguage.OnBackClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    PopupType.Error
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SelectLanguage.OnBackClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    PopupType.Error
                );
            }
        }
    }
}
