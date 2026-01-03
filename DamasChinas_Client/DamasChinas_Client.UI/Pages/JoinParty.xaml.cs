using DamasChinas_Client.UI.LobbyServiceProxy;
using DamasChinas_Client.UI.Models;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

namespace DamasChinas_Client.UI.Pages
{
    public partial class JoinParty : Page
    {
        private readonly LobbyManager _lobbyManager;
        private readonly string _username;
        private readonly int _userId;

        // ============================================================
        // CONSTRUCTOR: INVITADO
        // ============================================================
        public JoinParty()
        {
            InitializeComponent();

            ClientSession.EnsureGuestSession();

            _userId = 0;
            _username = ClientSession.SafeUsername;

            _lobbyManager = LobbySession.Manager;
            _lobbyManager.RegisterUser(_username);

            LoadPublicLobbies();
        }

        // ============================================================
        // CONSTRUCTOR: REGISTRADO
        // ============================================================
        public JoinParty(int userId, string username)
        {
            InitializeComponent();

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            _userId = userId;
            _username = username;

            _lobbyManager = LobbySession.Manager;
            _lobbyManager.RegisterUser(_username);

            LoadPublicLobbies();
        }

        private void LoadPublicLobbies()
        {
            try
            {
                var lobbies = _lobbyManager.GetPublicLobbies() ?? Array.Empty<LobbySummaryDto>();

                lstPublicLobbies.ItemsSource = lobbies.Select(l => new LobbySummary
                {
                    LobbyCode = l.LobbyCode,
                    Code = l.LobbyCode.ToString(),
                    HostUsername = l.HostUsername,
                    PlayerCount = $"{l.CurrentPlayers}/{l.MaxPlayers}",
                    IsPrivate = l.Visibility == LobbyVisibility.Private
                        ? MessageTranslator.GetLocalizedMessage(MessageKeys.PrivateLobby)
                        : MessageTranslator.GetLocalizedMessage(MessageKeys.PublicLobby)
                }).ToList();
            }
            catch (Exception ex) when (
                ex is EndpointNotFoundException ||
                ex is CommunicationException ||
                ex is TimeoutException)
            {
                Debug.WriteLine($"[JoinParty.LoadPublicLobbies] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);

                lstPublicLobbies.ItemsSource = Array.Empty<LobbySummary>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[JoinParty.LoadPublicLobbies] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }


        private void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            LoadPublicLobbies();
        }

        private void OnJoinSelectedClick(object sender, RoutedEventArgs e)
        {
            var selected = lstPublicLobbies.SelectedItem as LobbySummary;

            if (selected == null)
            {
                MessageHelper.ShowPopup(MessageKeys.NoLobbySelected, PopupType.Warning);
                return;
            }

            TryJoinLobby(selected.LobbyCode);
        }

        private void OnCodeBoxGotFocus(object sender, RoutedEventArgs e)
        {
            txtCodePlaceholder.Visibility = Visibility.Collapsed;
        }

        private void OnCodeBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLobbyCode.Text))
                txtCodePlaceholder.Visibility = Visibility.Visible;
        }

        private void OnJoinByCodeClick(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtLobbyCode.Text.Trim(), out int code))
            {
                MessageHelper.ShowPopup(MessageKeys.InvalidCodeWarning, PopupType.Warning);
                return;
            }

            TryJoinLobby(code);
        }

        private void TryJoinLobby(int lobbyCode)
        {
            try
            {
                OperationResult result;

                // ✅ Invitado usa endpoint especial
                if (ClientSession.IsGuest)
                {
                    result = _lobbyManager.JoinLobbyGuest(lobbyCode, _username);
                }
                else
                {
                    result = _lobbyManager.JoinLobby(lobbyCode, _username);
                }

                if (result.Success)
                {
                    var snapshot = _lobbyManager.GetCurrentLobby(_username);

                    if (snapshot == null)
                    {
                        MessageHelper.ShowPopup(MessageKeys.JoiningLobbyError, PopupType.Error);
                        return;
                    }

                    NavigationService?.Navigate(
                        new PreLobby(snapshot, _username, _userId)
                    );
                }
                else
                {
                    MessageHelper.ShowFromResult(result);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[JoinParty.TryJoinLobby] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.JoiningLobbyError, PopupType.Error);
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
