using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using DamasChinas_Client.UI.LobbyServiceProxy;
using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI.Pages
{
	public partial class PreLobby : Page
	{
		private readonly LobbyManager _lobbyManager;
		private readonly Lobby _lobby;
		private readonly string _username;

		public PreLobby()
		{
			InitializeComponent();

			_lobbyManager = new LobbyManager();

			_lobbyManager.MessageReceived += OnMessageReceived;
			_lobbyManager.MemberJoined += OnMemberJoined;
			_lobbyManager.MemberLeft += OnMemberLeft;
			_lobbyManager.LobbyClosed += OnLobbyClosed;
			_lobbyManager.GameStarted += OnGameStarted;

			lblLobbyCode.Text = "CODE-TEST123";
			lblUsername.Text = "HOST_USER";
		}

		public PreLobby(Lobby lobby, string username)
		{
			InitializeComponent();
			_lobby = lobby;
			_username = username;

			_lobbyManager = new LobbyManager();

			_lobbyManager.MessageReceived += OnMessageReceived;
			_lobbyManager.MemberJoined += OnMemberJoined;
			_lobbyManager.MemberLeft += OnMemberLeft;
			_lobbyManager.LobbyClosed += OnLobbyClosed;
			_lobbyManager.GameStarted += OnGameStarted;

			lblLobbyCode.Text = lobby.Code;
			lblUsername.Text = username;
			int memberCount = lobby.Members?.Length ?? 0;
			txtLobbyPlayers.Text = $"{memberCount}/6";
		}

		private void OnMessageReceived(int userId, string username, string message, string utc)
		{
			Dispatcher.Invoke(() =>
			{
				var text = $"[{utc}] {username}: {message}";
				var textBlock = new TextBlock
				{
					Text = text,
					Foreground = Brushes.White,
					TextWrapping = TextWrapping.Wrap
				};
				chatContainer.Children.Add(textBlock);
			});
		}

		private void OnMemberJoined(LobbyMember member)
		{
			Dispatcher.Invoke(() =>
			{
				var item = new ListBoxItem
				{
					Content = $"{member.Username} joined the lobby",
					Foreground = Brushes.LightGreen
				};
				friendsList.Items.Add(item);
			});
		}

		private void OnMemberLeft(int userId)
		{
			Dispatcher.Invoke(() =>
			{
				var item = new ListBoxItem
				{
					Content = $"Player {userId} left the lobby",
					Foreground = Brushes.OrangeRed
				};
				friendsLDist.Items.Add(item);
			});
		}

		private void OnLobbyClosed(string reason)
		{
			Dispatcher.Invoke(() =>
			{
				MessageBox.Show($"Lobby closed: {reason}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
				NavigationService.GoBack();
			});
		}

		private void OnGameStarted(string code)
		{
			Dispatcher.Invoke(() =>
			{
				MessageBox.Show($"Game {code} started!", "Game", MessageBoxButton.OK, MessageBoxImage.Information);
			});
		}

		private void OnSendMessageClick(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(txtChatMessage.Text))
			{
				_lobbyManager.SendMessage(txtChatMessage.Text);
				txtChatMessage.Clear();
			}
		}

		private void OnStartGameClick(object sender, RoutedEventArgs e)
		{
			_lobbyManager.StartGame();
		}

		private void OnExitClick(object sender, RoutedEventArgs e)
		{
			_lobbyManager.LeaveLobby();
			NavigationService.GoBack();
		}

		private void OnBackClick(object sender, RoutedEventArgs e)
		{
			NavigationService.GoBack();
		}
	}
}
