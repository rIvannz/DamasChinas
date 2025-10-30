using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using DamasChinas_Client.UI.FriendServiceProxy;
using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI.Pages
{
	public partial class Friends : Page
	{
		public ObservableCollection<FriendList> FriendsList { get; } = new ObservableCollection<FriendList>();

		public Friends(string username)
		{
			InitializeComponent();
			DataContext = this;

			LoadFriends(username);
		}

		private void LoadFriends(string username)
		{
			try
			{
				using (var client = new FriendServiceClient())
				{
					var friends = client.GetFriends(username);

					FriendsList.Clear();
					foreach (var friend in friends)
					{
						FriendsList.Add(new FriendList
						{
							Username = friend.Username,
							EnLinea = friend.ConnectionState,
							Avatar = "pack://application:,,,/DamasChinas_Client.UI;component/Assets/Icons/avatarIcon.png"
						});
					}
				}
			}
			catch (System.Exception ex)
			{
				MessageHelper.ShowError($"No se pudo cargar la lista de amigos: {ex.Message}", "Amigos");
			}
		}

		private void OnBackClick(object sender, RoutedEventArgs e)
		{
			NavigationService?.GoBack();
		}

		private void OnViewProfileClick(object sender, RoutedEventArgs e)
		{
			NavigationService?.Navigate(new ProfileUser());
		}

		private void OnChatClick(object sender, RoutedEventArgs e)
		{
			if (sender is FrameworkElement frameworkElement && frameworkElement.DataContext is FriendList friend)
			{
				var chatWindow = new ChatWindow(ClientSession.CurrentProfile, friend.Username);
				chatWindow.Show();
			}
		}

		private void OnSoundClick(object sender, RoutedEventArgs e)
		{
			NavigationService?.Navigate(new ConfiSound());
		}

		private void OnLanguageClick(object sender, RoutedEventArgs e)
		{
			NavigationService?.Navigate(new SelectLanguage());
		}
	}
}
