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

        // ===== CARGAR PARTIDAS PÚBLICAS =====
        private void LoadPublicLobbies()
        {
            try
            {
                var publicLobbies = _lobbyManager.GetPublicLobbies();

                var list = new List<LobbySummary>();
                foreach (var lobby in publicLobbies)
                {
                    int playerCount = lobby.Members?.Length ?? 0;

                    string hostUsername = $"User {lobby.HostUserId}";
                    if (lobby.Members != null)
                    {
                        foreach (var member in lobby.Members)
                        {
                            if (member.IsHost)
                            {
                                hostUsername = string.IsNullOrWhiteSpace(member.Username)
                                    ? hostUsername
                                    : member.Username;
                                break;
                            }
                        }
                    }

                    list.Add(new LobbySummary
                    {
                        Code = lobby.Code,
                        HostUsername = hostUsername,
                        PlayerCount = $"{playerCount}/6",
                        IsPrivate = lobby.IsPrivate
                            ? (string)FindResource("yes")
                            : (string)FindResource("no")
                    });
                }

                lstPublicLobbies.ItemsSource = list;
            }
            catch (FaultException faultEx)
            {
                MessageBox.Show(
                    faultEx.Message,
                    (string)FindResource("errorTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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


        // ===== REFRESCAR LISTA =====
        private void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            LoadPublicLobbies();
        }

        // ===== UNIRSE A PARTIDA SELECCIONADA =====
        private void OnJoinSelectedClick(object sender, RoutedEventArgs e)
        {
            if (lstPublicLobbies.SelectedItem is LobbySummary selectedLobby)
            {
                TryJoinLobby(selectedLobby.Code);
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

        // ===== UNIRSE POR CÓDIGO MANUAL =====
        private void OnJoinByCodeClick(object sender, RoutedEventArgs e)
        {
            string code = txtLobbyCode.Text.Trim();

            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show(
                    (string)FindResource("invalidCodeWarning"),
                    (string)FindResource("errorTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            TryJoinLobby(code);
        }

        // ===== UNIÓN A LOBBY (REAL) =====
        private void TryJoinLobby(string code)
        {
            try
            {
                var lobby = _lobbyManager.JoinLobby(code, _userId, _username);

                if (lobby != null)
                {
                    var preLobby = new PreLobby(lobby, _username);
                    NavigationService?.Navigate(preLobby);
                }
                else
                {
                    MessageBox.Show(
                        (string)FindResource("joiningLobbyError"),
                        (string)FindResource("errorTitle"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
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

        // ===== PLACEHOLDER DEL CÓDIGO =====
        private void OnCodeBoxGotFocus(object sender, RoutedEventArgs e)
        {
            txtCodePlaceholder.Visibility = Visibility.Collapsed;
        }

        private void OnCodeBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLobbyCode.Text))
            {
                txtCodePlaceholder.Visibility = Visibility.Visible;
            }
        }

        // ===== REGRESAR AL MENÚ =====
        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }

    // ===== CLASE AUXILIAR PARA LA LISTA DE PARTIDAS =====
    public class LobbySummary
    {
        public string Code { get; set; }
        public string HostUsername { get; set; }
        public string PlayerCount { get; set; }
        public string IsPrivate { get; set; }
    }
}
