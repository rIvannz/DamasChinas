using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows;


namespace DamasChinas_Client.Utilities
{
    /// <summary>
    /// Provides methods to dynamically change the application's language.
    /// </summary>
    public static class LanguageManager
    {
        /// <summary>
        /// Loads the corresponding ResourceDictionary according to the specified culture code.
        /// </summary>
        /// <param name="cultureCode">Culture code (e.g., "en-US" or "es-MX").</param>
        public static void ChangeLanguage(string cultureCode)
        {
            try
            {
                var newLangDict = new ResourceDictionary();

                switch (cultureCode)
                {
                    case "es-MX":
                        newLangDict.Source = new Uri(
                            "pack://application:,,,/DamasChinas_Client;component/DamasChinas_Client.UI/Resources/Lang.es.xaml",
                            UriKind.Absolute);
                        break;

                    default:
                        newLangDict.Source = new Uri(
                            "pack://application:,,,/DamasChinas_Client;component/DamasChinas_Client.UI/Resources/Lang.en.xaml",
                            UriKind.Absolute);
                        break;
                }


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

                if (existingLangDict != null)
                {
                    int index = Application.Current.Resources.MergedDictionaries.IndexOf(existingLangDict);
                    Application.Current.Resources.MergedDictionaries[index] = newLangDict;
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries.Add(newLangDict);
                }

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


