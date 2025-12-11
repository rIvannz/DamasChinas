using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DamasChinas_Client.UI.Utilities;
using static DamasChinas_Client.UI.Utilities.MessageKeys;

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
                    string languageCode = selectedItem.Tag?.ToString();

                    if (string.IsNullOrWhiteSpace(languageCode))
                    {
                        MessageHelper.ShowPopup(SelectLanguageFirst, PopupType.Warning);
                        return;
                    }

                    LanguageManager.ChangeLanguage(languageCode);

                    MessageHelper.ShowPopup(LanguageChangeSuccess, PopupType.Success);

              
                    var hostWindow = Window.GetWindow(this);
                    if (hostWindow != null && hostWindow.Owner != null)
                    {
                        hostWindow.Close();
                    }
                }
                else
                {
                    MessageHelper.ShowPopup(SelectLanguageFirst, PopupType.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SelectLanguage.OnApplyClick] {ex.Message}");
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
         
                if (NavigationService?.CanGoBack == true)
                {
                    NavigationService.GoBack();
                    return;
                }

       
                var hostWindow = Window.GetWindow(this);
                if (hostWindow != null && hostWindow.Owner != null)
                {
                    hostWindow.Close();
                    return;
                }

                MessageHelper.ShowPopup(NavigationError, PopupType.Warning);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[SelectLanguage.OnBackClick - InvalidOperation] {ex.Message}");
                MessageHelper.ShowPopup(NavigationError, PopupType.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SelectLanguage.OnBackClick - General] {ex.Message}");
                MessageHelper.ShowPopup(UnknownError, PopupType.Error);
            }
        }
    }
}
