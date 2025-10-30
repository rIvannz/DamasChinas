using System;
using System.Windows;
using System.Windows.Controls;

namespace DamasChinas_Client.UI.Pages
{
	public partial class MenuGuest : Page
	{
		public MenuGuest()
		{
		}

		private void OnJoinPartyClick(object sender, RoutedEventArgs e)
		{
			try
			{
				MessageBox.Show("Feature available only for registered users.", "Guest Access", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An unexpected error occurred while joining a party: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnHowToPlayClick(object sender, RoutedEventArgs e)
		{
			try
			{
				MessageBox.Show("Tutorial available soon.", "How to Play", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error loading tutorial: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnStatisticsClick(object sender, RoutedEventArgs e)
		{
			try
			{
				MessageBox.Show("Guest statistics are not available.", "Statistics", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred while accessing statistics: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnSoundClick(object sender, RoutedEventArgs e)
		{
			NavigationService?.Navigate(new ConfiSound());
		}

		private void OnBackClick(object sender, RoutedEventArgs e)
		{
			try
			{
				NavigationService?.Navigate(new MainWindow());
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error returning to the main menu: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void OnLanguageClick(object sender, RoutedEventArgs e)
		{
			try
			{
				NavigationService?.Navigate(new SelectLanguage());
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error while opening language settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
