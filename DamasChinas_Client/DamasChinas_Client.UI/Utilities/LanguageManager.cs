using System;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace DamasChinas_Client.UI.Utilities
{
    public static class LanguageManager
    {
        private const string LangEn = "Resources/Lang.en.xaml";
        private const string LangEs = "Resources/Lang.es.xaml";
        private const string LangPt = "Resources/Lang.pt.xaml";
        private const string LangFr = "Resources/Lang.fr.xaml";

        private const string ThemePath = "Styles/Theme.xaml";
        private const string ButtonsPath = "Styles/Buttons.xaml";

        public static string CurrentCultureCode { get; private set; } = "en-US";

        public static void ChangeLanguage(string cultureCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cultureCode))
                {
                    MessageHelper.ShowPopup(MessageKeys.LanguageChangeError, PopupType.Error);
                    return;
                }

                ResourceDictionary newLanguageDictionary = CreateLanguageDictionary(cultureCode);
                ResourceDictionary existingLanguageDictionary = FindExistingLanguageDictionary();

                ReplaceOrAddDictionary(newLanguageDictionary, existingLanguageDictionary);

                EnsureDictionary(ThemePath);
                EnsureDictionary(ButtonsPath);

                CurrentCultureCode = cultureCode;
                UpdateCulture(cultureCode);
            }
            catch (InvalidOperationException)
            {
                MessageHelper.ShowPopup(MessageKeys.LanguageChangeError, PopupType.Error);
            }
            catch (ArgumentException)
            {
                MessageHelper.ShowPopup(MessageKeys.LanguageChangeError, PopupType.Error);
            }
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.LanguageChangeError, PopupType.Error);
            }
        }

        private static ResourceDictionary CreateLanguageDictionary(string cultureCode)
        {
            string relativePath = MapCultureToLanguagePath(cultureCode);

            return new ResourceDictionary
            {
                Source = PathProvider.GetPackUri(relativePath)
            };
        }

        private static string MapCultureToLanguagePath(string cultureCode)
        {
            string code = cultureCode.Trim();

            if (string.Equals(code, "es-MX", StringComparison.OrdinalIgnoreCase))
            {
                return LangEs;
            }

            if (string.Equals(code, "pt-BR", StringComparison.OrdinalIgnoreCase))
            {
                return LangPt;
            }

            if (string.Equals(code, "fr-FR", StringComparison.OrdinalIgnoreCase))
            {
                return LangFr;
            }

            return LangEn;
        }

        private static ResourceDictionary FindExistingLanguageDictionary()
        {
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                if (dictionary?.Source == null)
                {
                    continue;
                }

                string src = dictionary.Source.OriginalString;

                if (src.IndexOf("/Resources/Lang.", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    src.IndexOf("Resources/Lang.", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return dictionary;
                }
            }

            return null;
        }

        private static void ReplaceOrAddDictionary(
            ResourceDictionary newDictionary,
            ResourceDictionary existingDictionary)
        {
            if (existingDictionary == null)
            {
                Application.Current.Resources.MergedDictionaries.Insert(0, newDictionary);
                return;
            }

            int index = Application.Current.Resources.MergedDictionaries.IndexOf(existingDictionary);
            if (index >= 0)
            {
                Application.Current.Resources.MergedDictionaries[index] = newDictionary;
            }
            else
            {
                Application.Current.Resources.MergedDictionaries.Insert(0, newDictionary);
            }
        }

        private static void EnsureDictionary(string relativePackPath)
        {
            Uri targetUri = PathProvider.GetPackUri(relativePackPath);

            bool exists = Application.Current.Resources.MergedDictionaries
                .Any(d => d?.Source != null &&
                          string.Equals(d.Source.OriginalString, targetUri.OriginalString, StringComparison.OrdinalIgnoreCase));

            if (!exists)
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = targetUri
                });
            }
        }

        private static void UpdateCulture(string cultureCode)
        {
            CultureInfo culture = new CultureInfo(cultureCode);

            CurrentCultureCode = cultureCode;

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }
}
