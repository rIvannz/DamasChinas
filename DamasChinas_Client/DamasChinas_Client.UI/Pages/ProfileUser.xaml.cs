using System.Windows;
using System.Windows.Controls;

namespace DamasChinas_Client.UI.Pages
{
	public partial class ProfileUser : Page
	{
		public ProfileUser()
		{
			InitializeComponent();
		}

		private void OnBackClick(object sender, RoutedEventArgs e)
		{
			NavigationService?.GoBack();
		}

		private void OnSoundClick(object sender, RoutedEventArgs e)
		{
			NavigationService?.Navigate(new ConfiSound());
		}

		private void OnLanguageClick(object sender, RoutedEventArgs e)
		{
			NavigationService?.Navigate(new SelectLanguage());
		}

		private void OnDeleteFriendClick(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Friend removed successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void OnSendMessageClick(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Chat feature coming soon!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
}
