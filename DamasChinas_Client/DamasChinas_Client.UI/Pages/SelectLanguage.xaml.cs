using System.Windows;
using System.Windows.Controls;
using DamasChinas_Client.UI.Utilities;

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
			if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem)
			{
				string languageCode = selectedItem.Tag.ToString();
				LanguageManager.ChangeLanguage(languageCode);
			}
			else
			{
				MessageBox.Show("Selecciona un idioma antes de aplicar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		private void OnBackClick(object sender, RoutedEventArgs e)
		{
			if (NavigationService?.CanGoBack == true)
			{
				NavigationService.GoBack();
			}
			else
			{
				MessageBox.Show("No hay una página anterior para regresar.", "Damas Chinas", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}
	}
}
