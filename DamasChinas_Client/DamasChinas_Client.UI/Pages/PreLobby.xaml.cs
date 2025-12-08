using DamasChinas_Client.UI.LobbyServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DamasChinas_Client.UI.Pages
{
    public partial class PreLobby : Page
    {
        private readonly LobbyManager _lobbyManager;
        private LobbySnapshotDto _snapshot;
        private readonly string _username;
        private readonly int _userId;

        public PreLobby(LobbySnapshotDto snapshot, string username, int userId)
        {
            InitializeComponent();

            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            PreLobbyPageManager.Register(this);

            _lobbyManager = LobbySession.Manager;
            _username = username;
            _userId = userId;

            _lobbyManager.RegisterUser(username);
            _lobbyManager.RegisterLobby(snapshot.LobbyCode);

            _lobbyManager.SnapshotReceived += ApplySnapshot;
            _lobbyManager.BanUpdated += ApplyBanInfo;

            ApplySnapshot(snapshot);
        }

        private bool IsCurrentUserHost()
        {
            var me = _snapshot?.Members?.FirstOrDefault(m => m.UserId == _userId);
            return me?.IsHost ?? false;
        }

        private void UpdateStartButtonState()
        {
            if (!IsCurrentUserHost())
            {
                btnStartGame.Visibility = Visibility.Collapsed;
                return;
            }

            int count = _snapshot.Members.Length;
            btnStartGame.Visibility = Visibility.Visible;
            btnStartGame.IsEnabled = (count == 2 || count == 4 || count == 6);
        }

        private void OnInviteFriendClick(object sender, RoutedEventArgs e)
        {
            MessageHelper.ShowPopup(MessageKeys.ChatComingSoon, PopupType.Info);
        }

        private void OnReportMemberClick(object sender, RoutedEventArgs e)
        {
            var member = (sender as Button)?.Tag as LobbyMemberDto;
            if (member == null || member.UserId == _userId)
                return;

            var req = new ReportPlayerRequest
            {
                LobbyCode = _snapshot.LobbyCode,
                ReporterUsername = _username,
                ReportedUsername = member.Username,
                Reason = "Reported from lobby"
            };

            _lobbyManager.ReportPlayer(req);
        }

        private void OnKickMemberClick(object sender, RoutedEventArgs e)
        {
            if (!IsCurrentUserHost())
                return;

            var member = (sender as Button)?.Tag as LobbyMemberDto;
            if (member == null || member.UserId == _userId)
                return;

            _lobbyManager.KickPlayer(member.Username);
        }

        private void OnChatTextChanged(object sender, TextChangedEventArgs e)
        {
            txtChatPlaceholder.Visibility =
                string.IsNullOrWhiteSpace(txtChatMessage.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        private void OnSendMessageClick(object sender, RoutedEventArgs e)
        {
            string text = txtChatMessage.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return;

            AddChatMessage(_username, text, DateTime.UtcNow.ToString("HH:mm:ss"));
            txtChatMessage.Clear();
        }

        public void AddChatMessage(string user, string message, string utc)
        {
            Dispatcher.Invoke(() =>
            {
                chatContainer.Children.Add(new TextBlock
                {
                    Text = $"[{utc}] {user}: {message}",
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap
                });
            });
        }



        public void ApplySnapshot(LobbySnapshotDto snapshot)
        {
            Dispatcher.Invoke(() =>
            {
                _snapshot = snapshot;

                lblLobbyCode.Text =
                    $"{MessageTranslator.GetLocalizedMessage(MessageKeys.LobbyCode)}: {snapshot.LobbyCode}";

                membersList.Items.Clear();
                foreach (var m in snapshot.Members)
                    membersList.Items.Add(m);

                UpdateStartButtonState();
            });
        }

        public void ApplyBanInfo(BanInfoDto ban)
        {
            if (ban?.IsBanned == true)
                MessageHelper.ShowPopup(MessageKeys.LobbyUserBanned, PopupType.Error);
        }

        private void OnStartGameClick(object sender, RoutedEventArgs e)
        {
            if (!IsCurrentUserHost())
            {
                MessageHelper.ShowPopup(MessageKeys.OnlyHostCanStart, PopupType.Warning);
                return;
            }

            _lobbyManager.StartGame();
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            _lobbyManager.LeaveLobby();
            PreLobbyPageManager.Unregister(this);
            NavigationService?.GoBack();
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            PreLobbyPageManager.Unregister(this);
            NavigationService?.GoBack();
        }
    }
}
