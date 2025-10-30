using System;
using System.Windows;
using System.Windows.Controls;

namespace DamasChinas_Client.UI.Pages
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Page
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void OnExitClick(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void OnGoToLoginClick(object sender, RoutedEventArgs e)
		{
			NavigationService?.Navigate(new Login());
		}

		private void OnGoToSignInClick(object sender, RoutedEventArgs e)
		{
			NavigationService?.Navigate(new SignIn());
		}

		private void OnPlayAsGuestClick(object sender, RoutedEventArgs e)
		{
			NavigationService?.Navigate(new MenuGuest());
		}

		private void OnSoundClick(object sender, RoutedEventArgs e)
		{
			NavigationService?.Navigate(new ConfiSound());
		}

		private void OnLanguageClick(object sender, RoutedEventArgs e)
		{
			try
			{
				NavigationService?.Navigate(new SelectLanguage());
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error al abrir la configuración de idioma: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
