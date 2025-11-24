using DamasChinas_Client.UI.LobbyServiceProxy;
using DamasChinas_Client.UI.Models;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;

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
                    {
                        hostUsername = host.Username;
                    }

                    list.Add(new LobbySummary
                    {
                        Code = lobby.Code,
                        HostUsername = hostUsername,
                        PlayerCount = $"{playerCount}/6",
                        IsPrivate = lobby.IsPrivate
                            ? MessageTranslator.GetLocalizedMessage(MessageKeys.PrivateLobby)
                            : MessageTranslator.GetLocalizedMessage(MessageKeys.PublicLobby)
                    });
                }

                lstPublicLobbies.ItemsSource = null;
                lstPublicLobbies.ItemsSource = list;
                lstPublicLobbies.Items.Refresh();
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[JoinParty.LoadPublicLobbies - Communication] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageKeys.ServerUnavailable,
                    PopupType.Error
                );
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"[JoinParty.LoadPublicLobbies - Timeout] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageKeys.NetworkLatency,
                    PopupType.Error
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[JoinParty.LoadPublicLobbies - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageKeys.UnknownError,
                    PopupType.Error
                );
            }
        }

        private void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            LoadPublicLobbies();
        }

        private void OnJoinSelectedClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstPublicLobbies.SelectedItem is LobbySummary selected)
                {
                    TryJoinLobby(selected.Code);
                }
                else
                {
                    MessageHelper.ShowPopup(
                        MessageKeys.NoLobbySelected,
                        PopupType.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[JoinParty.OnJoinSelectedClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageKeys.UnknownError,
                    PopupType.Error
                );
            }
        }

        private void OnJoinByCodeClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string code = txtLobbyCode.Text.Trim();

                if (string.IsNullOrWhiteSpace(code))
                {
                    MessageHelper.ShowPopup(
                        MessageKeys.InvalidCodeWarning,
                        PopupType.Warning
                    );
                    return;
                }

                TryJoinLobby(code);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[JoinParty.OnJoinByCodeClick] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageKeys.UnknownError,
                    PopupType.Warning
                );
            }
        }

        private void TryJoinLobby(string code)
        {
            try
            {
                var lobby = _lobbyManager.JoinLobby(code, _userId, _username);

                if (lobby == null)
                {
                    MessageHelper.ShowPopup(
                        MessageKeys.JoiningLobbyError,
                        PopupType.Warning
                    );
                    return;
                }

                NavigationService?.Navigate(new PreLobby(lobby, _userId, _username));
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"[JoinParty.TryJoinLobby - Timeout] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageKeys.NetworkLatency,
                    PopupType.Warning
                );

                LoadPublicLobbies();
            }
            catch (FaultException ex)
            {
                Debug.WriteLine($"[JoinParty.TryJoinLobby - Fault] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageKeys.JoiningLobbyError,
                    PopupType.Error
                );

                LoadPublicLobbies();
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[JoinParty.TryJoinLobby - Communication] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageKeys.ServerUnavailable,
                    PopupType.Error
                );
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[JoinParty.TryJoinLobby - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageKeys.NavigationError,
                    PopupType.Error
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[JoinParty.TryJoinLobby - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageKeys.JoiningLobbyError,
                    PopupType.Error
                );
            }
        }

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

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.GoBack();
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[JoinParty.OnBackClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageKeys.NavigationError,
                    PopupType.Error
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[JoinParty.OnBackClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageKeys.UnknownError,
                    PopupType.Error
                );
            }
        }
    }
}
