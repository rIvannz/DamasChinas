using System;
using System.Globalization;
using System.Windows;

namespace DamasChinas_Client.UI.Utilities
{
	/// <summary>
	/// Provides methods to dynamically change the application's language without losing theme and style resources.
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
				var newLanguageDictionary = CreateLanguageDictionary(cultureCode);
				var existingLanguageDictionary = FindExistingLanguageDictionary();
				ReplaceOrAddDictionary(newLanguageDictionary, existingLanguageDictionary);
				EnsureThemeResources();
				UpdateCulture(cultureCode);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error while changing language: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private static ResourceDictionary CreateLanguageDictionary(string cultureCode)
		{
			var resourceDictionary = new ResourceDictionary();

			switch (cultureCode)
			{
				case "es-MX":
					resourceDictionary.Source = new Uri(
						"pack://application:,,,/DamasChinas_Client.UI;component/Resources/Lang.es.xaml",
						UriKind.Absolute);
					break;

				default:
					resourceDictionary.Source = new Uri(
						"pack://application:,,,/DamasChinas_Client.UI;component/Resources/Lang.en.xaml",
						UriKind.Absolute);
					break;
			}

			return resourceDictionary;
		}

		private static ResourceDictionary FindExistingLanguageDictionary()
		{
			foreach (var dictionary in Application.Current.Resources.MergedDictionaries)
			{
				if (dictionary.Source != null &&
					(dictionary.Source.OriginalString.Contains("Lang.en.xaml") ||
					 dictionary.Source.OriginalString.Contains("Lang.es.xaml")))
				{
					return dictionary;
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
			var themeDictionary = new ResourceDictionary
			{
				Source = new Uri(
					"pack://application:,,,/DamasChinas_Client.UI;component/Styles/Theme.xaml",
					UriKind.Absolute)
			};

			var buttonsDictionary = new ResourceDictionary
			{
				Source = new Uri(
					"pack://application:,,,/DamasChinas_Client.UI;component/Styles/Buttons.xaml",
					UriKind.Absolute)
			};

			Application.Current.Resources.MergedDictionaries.Add(themeDictionary);
			Application.Current.Resources.MergedDictionaries.Add(buttonsDictionary);
		}

		private static void UpdateCulture(string cultureCode)
		{
			var culture = new CultureInfo(cultureCode);
			CultureInfo.DefaultThreadCurrentCulture = culture;
			CultureInfo.DefaultThreadCurrentUICulture = culture;
		}
	}
}
