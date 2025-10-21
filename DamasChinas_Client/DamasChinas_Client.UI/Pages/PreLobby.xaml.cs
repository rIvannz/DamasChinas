using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using DamasChinas_Client.UI.Utilities;           
using DamasChinas_Client.UI.LobbyServiceProxy;  




namespace DamasChinas_Client.UI.Pages
{
    public partial class PreLobby : Page
    {
        private readonly LobbyManager _lobbyManager;
        private readonly Lobby _lobby;
        private readonly string _username;

        // Constructor por defecto (solo diseño o pruebas)
        public PreLobby()
        {
            InitializeComponent();

            _lobbyManager = new LobbyManager();

            // Suscripción de eventos
            _lobbyManager.MessageReceived += OnMessageReceived;
            _lobbyManager.MemberJoined += OnMemberJoined;
            _lobbyManager.MemberLeft += OnMemberLeft;
            _lobbyManager.LobbyClosed += OnLobbyClosed;
            _lobbyManager.GameStarted += OnGameStarted;

            // Datos de prueba
            lblLobbyCode.Text = "CODE-TEST123";
            lblUsername.Text = "HOST_USER";
        }

        // Constructor que recibe datos reales desde MenuRegisteredPlayer
        public PreLobby(Lobby lobby, string username)
        {
            InitializeComponent();
            _lobby = lobby;
            _username = username;

            _lobbyManager = new LobbyManager();

            // Suscripción de eventos
            _lobbyManager.MessageReceived += OnMessageReceived;
            _lobbyManager.MemberJoined += OnMemberJoined;
            _lobbyManager.MemberLeft += OnMemberLeft;
            _lobbyManager.LobbyClosed += OnLobbyClosed;
            _lobbyManager.GameStarted += OnGameStarted;

            // Mostrar datos del lobby
            lblLobbyCode.Text = lobby.Code;
            lblUsername.Text = username;
            txtLobbyPlayers.Text = $"{lobby.Members.Count()}/6";

        }

        // ==========================
        //  CALLBACKS DEL LOBBY
        // ==========================

        private void OnMessageReceived(int userId, string username, string message, string utc)
        {
            Dispatcher.Invoke(() =>
            {
                var text = $"[{utc}] {username}: {message}";
                TextBlock msg = new TextBlock
                {
                    Text = text,
                    Foreground = System.Windows.Media.Brushes.White,
                    TextWrapping = TextWrapping.Wrap
                };
                chatContainer.Children.Add(msg);
            });
        }

        private void OnMemberJoined(LobbyMember member)
        {
            Dispatcher.Invoke(() =>
            {
                ListBoxItem item = new ListBoxItem
                {
                    Content = $"{member.Username} joined the lobby",
                    Foreground = System.Windows.Media.Brushes.LightGreen
                };
                friendsList.Items.Add(item);
            });
        }

        private void OnMemberLeft(int userId)
        {
            Dispatcher.Invoke(() =>
            {
                ListBoxItem item = new ListBoxItem
                {
                    Content = $"Player {userId} left the lobby",
                    Foreground = System.Windows.Media.Brushes.OrangeRed
                };
                friendsLDist.Items.Add(item);
            });
        }

        private void OnLobbyClosed(string reason)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Lobby closed: {reason}",
                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack();
            });
        }

        private void OnGameStarted(string code)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Game {code} started!",
                    "Game", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        // ==========================
        //  EVENTOS DE BOTONES
        // ==========================

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

