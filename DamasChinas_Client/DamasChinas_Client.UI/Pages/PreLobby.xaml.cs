using DamasChinas_Client.UI.LobbyServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace DamasChinas_Client.UI.Pages
{
    public partial class PreLobby : Page
    {
        private readonly LobbyManager _lobbyManager;
        private readonly Lobby _lobby;
        private readonly string _username;
        private readonly int _currentUserId;

        // Diseño
        public PreLobby()
        {
            InitializeComponent();
            _lobbyManager = new LobbyManager();
            HookEvents();
            lblLobbyCode.Text = "CODE-TEST123";
            txtLobbyPlayers.Text = "0/6";
        }

        public PreLobby(Lobby lobby, int userId, string username)
        {
            InitializeComponent();
            _lobby = lobby;
            _username = username;
            _currentUserId = userId;

            _lobbyManager = new LobbyManager();
            HookEvents();

            lblLobbyCode.Text = lobby.Code;
            var count = lobby.Members?.Length ?? 0;
            txtLobbyPlayers.Text = $"{count}/6";

            membersList.Items.Clear();
            if (lobby.Members != null)
                foreach (var m in lobby.Members)
                    membersList.Items.Add(m);
        }

        private void HookEvents()
        {
            _lobbyManager.MessageReceived += OnMessageReceived;
            _lobbyManager.MemberJoined += OnMemberJoined;
            _lobbyManager.MemberLeft += OnMemberLeft;
            _lobbyManager.LobbyClosed += OnLobbyClosed;
            _lobbyManager.GameStarted += OnGameStarted;
        }

        // === Callbacks ===
        private void OnMessageReceived(int userId, string username, string message, string utc)
        {
            Dispatcher.Invoke(() =>
            {
                var tb = new TextBlock
                {
                    Text = $"[{utc}] {username}: {message}",
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap
                };
                chatContainer.Children.Add(tb);
            });
        }

        // ===== INVITAR AMIGO (mock temporal) =====
        private void OnInviteFriendClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is string friendName)
            {
                MessageBox.Show(
                    $"Invitación enviada a {friendName}.",
                    (string)FindResource("infoTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void OnMemberJoined(LobbyMember member)
        {
            Dispatcher.Invoke(() =>
            {
                if (!membersList.Items.OfType<LobbyMember>().Any(m => m.UserId == member.UserId))
                    membersList.Items.Add(member);
                txtLobbyPlayers.Text = $"{membersList.Items.Count}/6";
            });
        }

        private void OnMemberLeft(int userId)
        {
            Dispatcher.Invoke(() =>
            {
                var item = membersList.Items.OfType<LobbyMember>().FirstOrDefault(m => m.UserId == userId);
                if (item != null) membersList.Items.Remove(item);
                txtLobbyPlayers.Text = $"{membersList.Items.Count}/6";
            });
        }

        private void OnLobbyClosed(string reason)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{FindResource("lobbyClosed")} ({reason})",
                    (string)FindResource("infoTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService?.GoBack();
            });
        }

        private void OnGameStarted(string code)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{FindResource("gameStarting")} {code}",
                    (string)FindResource("success"), MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        // === Botones ===
        private void OnSendMessageClick(object sender, RoutedEventArgs e)
        {
            var text = txtChatMessage.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            // MOCK local: pinta el mensaje en el panel y limpia
            var tb = new TextBlock
            {
                Text = $"[{DateTime.Now:HH:mm:ss}] {_username}: {text}",
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap
            };
            chatContainer.Children.Add(tb);
            txtChatMessage.Clear();

            // cuando conectes backend: _lobbyManager.SendMessage(text);
        }

        private void OnChatTextChanged(object sender, TextChangedEventArgs e)
        {
            txtChatPlaceholder.Visibility =
                string.IsNullOrWhiteSpace(txtChatMessage.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnStartGameClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var me = _lobby?.Members?.FirstOrDefault(m => m.UserId == _currentUserId);
                if (me == null || !me.IsHost)
                {
                    MessageBox.Show((string)FindResource("onlyHostCanStart"),
                        (string)FindResource("errorTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // cuando conectes al servidor: _lobbyManager.StartGame();
                // navegación a MatchRoom
                NavigationService?.Navigate(new MatchRoom());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{FindResource("errorStartingGame")} ({ex.Message})",
                    (string)FindResource("errorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                (string)FindResource("confirmExitLobby"),
                (string)FindResource("confirmTitle"),
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                _lobbyManager.LeaveLobby(); // sin args
                MessageBox.Show((string)FindResource("lobbyLeft"),
                    (string)FindResource("infoTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error leaving lobby: {ex.Message}",
                    (string)FindResource("errorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnInviteClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.DataContext is string friend)
                MessageBox.Show($"{friend} invited!", (string)FindResource("infoTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnBackClick(object sender, RoutedEventArgs e) => NavigationService?.GoBack();

        private void OnBanMemberClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is LobbyMember m)
                MessageBox.Show($"{m.Username} banned (mock).", (string)FindResource("infoTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

