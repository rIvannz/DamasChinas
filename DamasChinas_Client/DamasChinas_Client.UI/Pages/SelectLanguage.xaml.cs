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

        // ============================================================
        // 🔹 Apply Language
        // ============================================================

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
                        "success"
                    );
                }
                else
                {
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage("msg_SelectLanguageFirst"),
                        "warning"
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SelectLanguage.OnApplyClick] {ex.Message}");

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
                if (NavigationService?.CanGoBack == true)
                {
                    NavigationService.GoBack();
                }
                else
                {
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                        "warning"
                    );
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[SelectLanguage.OnBackClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SelectLanguage.OnBackClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }
    }
}
