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
            txtLobbyPlayers.Text = "0/6";
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
                // Evita duplicados visuales
                if (!membersList.Items.OfType<LobbyMember>().Any(m => m.UserId == member.UserId))
                {
                    membersList.Items.Add(member);
                }

                // Actualiza contador
                int count = membersList.Items.Count;
                txtLobbyPlayers.Text = $"{count}/6";
            });
        }



        private void OnBanMemberClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is LobbyMember member)
            {
                _lobbyManager.BanMember(member.UserId);
                MessageBox.Show($"{member.Username} has been banned.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void OnMemberLeft(int userId)
        {
            Dispatcher.Invoke(() =>
            {
                // Elimina de la lista visual
                var item = membersList.Items.OfType<LobbyMember>()
                                  .FirstOrDefault(m => m.UserId == userId);
                if (item != null)
                {
                    membersList.Items.Remove(item);
                }

                // Actualiza contador
                int count = membersList.Items.Count;
                txtLobbyPlayers.Text = $"{count}/6";
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

       
        private void OnChatTextChanged(object sender, TextChangedEventArgs e)
        {
            txtChatPlaceholder.Visibility =
                string.IsNullOrWhiteSpace(txtChatMessage.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }


        private void OnStartGameClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Solo el host puede iniciar la partida
                if (_lobby.HostUserId != _lobby.Members.FirstOrDefault(m => m.Username == _username)?.UserId)
                {
                    MessageBox.Show(
                        (string)FindResource("onlyHostCanStart"),
                        (string)FindResource("errorTitle"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Llamar al servidor para iniciar la partida
                _lobbyManager.StartGame();

                MessageBox.Show(
                    (string)FindResource("gameStarting"),
                    (string)FindResource("success"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{FindResource("errorStartingGame")} ({ex.Message})",
                    (string)FindResource("errorTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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
