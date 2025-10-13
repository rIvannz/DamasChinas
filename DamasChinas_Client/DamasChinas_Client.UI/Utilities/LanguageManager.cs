using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows;


namespace DamasChinas_Client.UI.Utilities
{
    /// <summary>
    /// Provides methods to dynamically change the application's language
    /// without losing theme and style resources.
    /// </summary>
    public static class LanguageManager
    {
        /// <summary>
        /// Loads the ResourceDictionary corresponding to the selected language.
        /// </summary>
        /// <param name="cultureCode">Culture code (e.g., "en-US" or "es-MX").</param>
        public static void ChangeLanguage(string cultureCode)
        {
            try
            {
                // Load new language dictionary
                var newLangDict = new ResourceDictionary();

                switch (cultureCode)
                {
                    case "es-MX":
                        newLangDict.Source = new Uri(
                            "pack://application:,,,/DamasChinas_Client.UI;component/Resources/Lang.es.xaml",
                            UriKind.Absolute);
                        break;

                    default:
                        newLangDict.Source = new Uri(
                            "pack://application:,,,/DamasChinas_Client.UI;component/Resources/Lang.en.xaml",
                            UriKind.Absolute);
                        break;
                }

                // Find if a language dictionary already exists
                ResourceDictionary existingLangDict = null;

                foreach (var dict in Application.Current.Resources.MergedDictionaries)
                {
                    if (dict.Source != null &&
                        (dict.Source.OriginalString.Contains("Lang.en.xaml") ||
                         dict.Source.OriginalString.Contains("Lang.es.xaml")))
                    {
                        existingLangDict = dict;
                        break;
                    }
                }

                // Replace or add the new language dictionary
                if (existingLangDict != null)
                {
                    int index = Application.Current.Resources.MergedDictionaries.IndexOf(existingLangDict);
                    Application.Current.Resources.MergedDictionaries[index] = newLangDict;
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries.Add(newLangDict);
                }

                // Preserve theme and button styles
                var themeDict = new ResourceDictionary
                {
                    Source = new Uri(
                        "pack://application:,,,/DamasChinas_Client.UI;component/Styles/Theme.xaml",
                        UriKind.Absolute)
                };

                var buttonsDict = new ResourceDictionary
                {
                    Source = new Uri(
                        "pack://application:,,,/DamasChinas_Client.UI;component/Styles/Buttons.xaml",
                        UriKind.Absolute)
                };

                // Ensure they're always merged after the language file
                Application.Current.Resources.MergedDictionaries.Add(themeDict);
                Application.Current.Resources.MergedDictionaries.Add(buttonsDict);

                // Update global culture info
                var culture = new CultureInfo(cultureCode);
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while changing language: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}


