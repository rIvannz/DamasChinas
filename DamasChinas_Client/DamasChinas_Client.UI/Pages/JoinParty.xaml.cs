using DamasChinas_Client.UI.LobbyServiceProxy;
using DamasChinas_Client.UI.Models;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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


namespace DamasChinas_Client.UI.Pages
{
    public partial class JoinParty : Page
    {
        private readonly LobbyManager _lobbyManager;
        private readonly string _username;
        private readonly int _userId;

        public JoinParty(int userId, string username)
        {
            InitializeComponent();
            _lobbyManager = new LobbyManager();
            _userId = userId;
            _username = username;
            LoadPublicLobbies();
        }

        private void LoadPublicLobbies()
        {
            try
            {
                var publicLobbies = _lobbyManager.GetPublicLobbies() ?? new List<Lobby>();
                var activeLobbies = publicLobbies
                    .Where(l => l.Members != null && l.Members.Length > 0)
                    .ToList();

                var list = new List<LobbySummary>();
                foreach (var lobby in activeLobbies)
                {
                    int playerCount = lobby.Members?.Length ?? 0;

                    string hostUsername = $"User {lobby.HostUserId}";
                    var host = lobby.Members?.FirstOrDefault(m => m.IsHost);
                    if (host != null && !string.IsNullOrWhiteSpace(host.Username))
                        hostUsername = host.Username;

                    list.Add(new LobbySummary
                    {
                        Code = lobby.Code,
                        HostUsername = hostUsername,
                        PlayerCount = $"{playerCount}/6",
                        IsPrivate = lobby.IsPrivate
                            ? (string)FindResource("private")
                            : (string)FindResource("public")
                    });
                }

                lstPublicLobbies.ItemsSource = null;
                lstPublicLobbies.ItemsSource = list;
                lstPublicLobbies.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{FindResource("joiningLobbyError")} ({ex.Message})",
                    (string)FindResource("errorTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OnRefreshClick(object sender, RoutedEventArgs e) => LoadPublicLobbies();

        private void OnJoinSelectedClick(object sender, RoutedEventArgs e)
        {
            if (lstPublicLobbies.SelectedItem is LobbySummary selected)
            {
                TryJoinLobby(selected.Code);
            }
            else
            {
                MessageBox.Show(
                    (string)FindResource("noLobbySelected"),
                    (string)FindResource("errorTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void OnJoinByCodeClick(object sender, RoutedEventArgs e)
        {
            var code = txtLobbyCode.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show((string)FindResource("invalidCodeWarning"),
                    (string)FindResource("errorTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            TryJoinLobby(code);
        }

        private void TryJoinLobby(string code)
        {
            try
            {
                var lobby = _lobbyManager.JoinLobby(code, _userId, _username);
                if (lobby == null)
                    throw new Exception("No lobby returned");

                NavigationService?.Navigate(new PreLobby(lobby, _userId, _username));
            }
            catch (TimeoutException)
            {
                MessageBox.Show((string)FindResource("lobbyExpired"),
                    (string)FindResource("errorTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                LoadPublicLobbies();
            }
            catch (FaultException fault)
            {
                MessageBox.Show(fault.Message,
                    (string)FindResource("errorTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                LoadPublicLobbies();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{FindResource("joiningLobbyError")} ({ex.Message})",
                    (string)FindResource("errorTitle"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCodeBoxGotFocus(object sender, RoutedEventArgs e)
            => txtCodePlaceholder.Visibility = Visibility.Collapsed;

        private void OnCodeBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLobbyCode.Text))
                txtCodePlaceholder.Visibility = Visibility.Visible;
        }

        private void OnBackClick(object sender, RoutedEventArgs e) => NavigationService?.GoBack();
    }
}
