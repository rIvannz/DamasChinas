using System;
using System.Globalization;
using System.Windows;

namespace DamasChinas_Client.UI.Utilities
{
   
    public static class LanguageManager
    {
     
        public static void ChangeLanguage(string cultureCode)
        {
            try
            {
                ResourceDictionary newLanguageDictionary = CreateLanguageDictionary(cultureCode);
                ResourceDictionary existingLanguageDictionary = FindExistingLanguageDictionary();

                ReplaceOrAddDictionary(newLanguageDictionary, existingLanguageDictionary);
                EnsureThemeResources();
                UpdateCulture(cultureCode);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error while changing language: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private static ResourceDictionary CreateLanguageDictionary(string cultureCode)
        {
            string relativePath;

            if (cultureCode == "es-MX")
            {
                relativePath = "Resources/Lang.es.xaml";
            }
            else
            {
                relativePath = "Resources/Lang.en.xaml";
            }

            return new ResourceDictionary
            {
                Source = PathProvider.GetPackUri(relativePath)
            };
        }

      
        private static ResourceDictionary FindExistingLanguageDictionary()
        {
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                if (dictionary.Source != null)
                {
                    string src = dictionary.Source.OriginalString;

                    if (src.Contains("Lang.en.xaml") || src.Contains("Lang.es.xaml"))
                    {
                        return dictionary;
                    }
                }
            }

            return null;
        }

        
        private static void ReplaceOrAddDictionary(ResourceDictionary newDictionary, ResourceDictionary existingDictionary)
        {
            if (existingDictionary != null)
            {
                int index = Application.Current.Resources.MergedDictionaries.IndexOf(existingDictionary);
                Application.Current.Resources.MergedDictionaries[index] = newDictionary;
            }
            else
            {
                Application.Current.Resources.MergedDictionaries.Add(newDictionary);
            }
        }

      
        private static void EnsureThemeResources()
        {
            ResourceDictionary themeDictionary = new ResourceDictionary
            {
                Source = PathProvider.GetPackUri("Styles/Theme.xaml")
            };

            ResourceDictionary buttonsDictionary = new ResourceDictionary
            {
                Source = PathProvider.GetPackUri("Styles/Buttons.xaml")
            };

            Application.Current.Resources.MergedDictionaries.Add(themeDictionary);
            Application.Current.Resources.MergedDictionaries.Add(buttonsDictionary);
        }

     
        private static void UpdateCulture(string cultureCode)
        {
            CultureInfo culture = new CultureInfo(cultureCode);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }
}
