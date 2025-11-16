using DamasChinas_Client.UI.LobbyServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

using System.Diagnostics;

namespace DamasChinas_Client.UI.Pages
{
    public partial class PreLobby : Page
    {
        private readonly LobbyManager _lobbyManager;
        private readonly Lobby _lobby;
        private readonly string _username;
        private readonly int _currentUserId;

     

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
            {
                foreach (var m in lobby.Members)
                {
                    membersList.Items.Add(m);
                }
            }
        }


        private void HookEvents()
        {
            _lobbyManager.MessageReceived += OnMessageReceived;
            _lobbyManager.MemberJoined += OnMemberJoined;
            _lobbyManager.MemberLeft += OnMemberLeft;
            _lobbyManager.LobbyClosed += OnLobbyClosed;
            _lobbyManager.GameStarted += OnGameStarted;
        }

       

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

       

        private void OnInviteFriendClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is string friendName)
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_InviteSent")
                        .Replace("{friend}", friendName),
                    "info"
                );
            }
        }

    

        private void OnMemberJoined(LobbyMember member)
        {
            Dispatcher.Invoke(() =>
            {
                if (!membersList.Items.OfType<LobbyMember>().Any(m => m.UserId == member.UserId))
                {
                    membersList.Items.Add(member);
                }

                txtLobbyPlayers.Text = $"{membersList.Items.Count}/6";
            });
        }


        private void OnMemberLeft(int userId)
        {
            Dispatcher.Invoke(() =>
            {
                var target = membersList.Items.OfType<LobbyMember>().FirstOrDefault(m => m.UserId == userId);

                if (target != null)
                {
                    membersList.Items.Remove(target);
                }

                txtLobbyPlayers.Text = $"{membersList.Items.Count}/6";
            });
        }


        private void OnLobbyClosed(string reason)
        {
            Dispatcher.Invoke(() =>
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("lobbyClosed") +
                    $" ({reason})",
                    "info"
                );

                NavigationService?.GoBack();
            });
        }

     

        private void OnGameStarted(string code)
        {
            Dispatcher.Invoke(() =>
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("gameStarting") + $" {code}",
                    "success"
                );
            });
        }

     

        private void OnSendMessageClick(object sender, RoutedEventArgs e)
        {
            var text = txtChatMessage.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var tb = new TextBlock
            {
                Text = $"[{DateTime.Now:HH:mm:ss}] {_username}: {text}",
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap
            };

            chatContainer.Children.Add(tb);
            txtChatMessage.Clear();
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
                    MessageHelper.ShowPopup(
                        MessageTranslator.GetLocalizedMessage("onlyHostCanStart"),
                        "warning"
                    );

                    return;
                }

                NavigationService?.Navigate(new MatchRoom());
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[PreLobby.OnStartGameClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PreLobby.OnStartGameClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("errorStartingGame"),
                    "error"
                );
            }
        }


        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("confirmExitLobby"),
                    "warning"
                );

                _lobbyManager.LeaveLobby();

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("lobbyLeft"),
                    "success"
                );

                NavigationService?.GoBack();
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[PreLobby.OnExitClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PreLobby.OnExitClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }



        private void OnInviteClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.DataContext is string friend)
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_InviteSent")
                        .Replace("{friend}", friend),
                    "info"
                );
            }
        }

        private void OnBanMemberClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is LobbyMember m)
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_PlayerBanned")
                        .Replace("{username}", m.Username),
                    "info"
                );
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
                Debug.WriteLine($"[PreLobby.OnBackClick - InvalidOperation] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_NavigationError"),
                    "error"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PreLobby.OnBackClick - General] {ex.Message}");

                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_UnknownError"),
                    "error"
                );
            }
        }
    }
}
