using DamasChinas_Client.UI.Callbacks;
using DamasChinas_Client.UI.FriendServiceProxy;
using DamasChinas_Client.UI.LobbyServiceProxy;
using DamasChinas_Client.UI.Utilities;
using DamasChinas_Shared.Contracts.Dtos;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DamasChinas_Client.UI.Pages
{
    public partial class PreLobby : Page
    {
        private const string DefaultAvatarFile = "avatarIcon.png";

        private readonly LobbyManager _lobbyManager;
        private LobbySnapshotDto _snapshot;
        private readonly string _username;
        private readonly int _userId;

        public ObservableCollection<LobbyMemberViewModel> MembersCollection { get; set; } =
            new ObservableCollection<LobbyMemberViewModel>();

        public ObservableCollection<FriendViewModel> FriendsCollection { get; set; } =
            new ObservableCollection<FriendViewModel>();

        public PreLobby(LobbySnapshotDto snapshot, string username, int userId)
        {
            InitializeComponent();

            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            PreLobbyPageManager.Register(this);

            _lobbyManager = LobbySession.Manager;
            _username = username;
            _userId = userId;

            membersList.ItemsSource = MembersCollection;
            friendsList.ItemsSource = FriendsCollection;

            _lobbyManager.RegisterUser(username);
            _lobbyManager.RegisterLobby(snapshot.LobbyCode);

            SubscribeEvents();
            ApplySnapshot(snapshot);
            LoadFriends();
        }

        private void SubscribeEvents()
        {
            _lobbyManager.SnapshotReceived += ApplySnapshot;
            _lobbyManager.BanUpdated += ApplyBanInfo;
            _lobbyManager.ChatMessageReceived += OnChatMessageReceived;
            _lobbyManager.Kicked += OnKicked;
            _lobbyManager.Closed += OnLobbyClosed;
            _lobbyManager.GameStarting += OnGameStarting;
        }

        private void UnsubscribeEvents()
        {
            _lobbyManager.SnapshotReceived -= ApplySnapshot;
            _lobbyManager.BanUpdated -= ApplyBanInfo;
            _lobbyManager.ChatMessageReceived -= OnChatMessageReceived;
            _lobbyManager.Kicked -= OnKicked;
            _lobbyManager.Closed -= OnLobbyClosed;
            _lobbyManager.GameStarting -= OnGameStarting;
        }

        public void ApplySnapshot(LobbySnapshotDto snapshot)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _snapshot = snapshot;

                lblLobbyCode.Text =
                    $"{MessageTranslator.GetLocalizedMessage(MessageKeys.LobbyCode)}: {snapshot.LobbyCode}";

                if (FindName("lblPlayerCount") is TextBlock lblCount)
                    lblCount.Text = $"{snapshot.Members.Length} / {snapshot.MaxPlayers}";

                MembersCollection.Clear();

                bool amIHost = snapshot.Members.Any(m => m.Username == _username && m.IsHost);

                foreach (var m in snapshot.Members)
                {
                    bool isMe = m.Username == _username;
                    string displayName = m.IsHost ? $"?? {m.Username}" : m.Username;

                    // ? Host registrado puede kickear a cualquiera (incluyendo guests)
                    Visibility kickVis =
                        (amIHost && !isMe && !ClientSession.IsGuest)
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                    // ? Report NO aparece si el target es guest (y guest tampoco reporta)
                    Visibility reportVis =
                        (!isMe &&
                         !ClientSession.IsGuest &&
                         !ClientSession.IsGuestUsername(m.Username))
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                    string avatarFile = string.IsNullOrWhiteSpace(m.AvatarFile)
                        ? DefaultAvatarFile
                        : m.AvatarFile;

                    MembersCollection.Add(new LobbyMemberViewModel
                    {
                        UserId = m.UserId,
                        Username = m.Username,
                        DisplayName = displayName,
                        AvatarFile = avatarFile,
                        AvatarSource = PathProvider.LoadAvatar(avatarFile),
                        IsHost = m.IsHost,
                        KickVisibility = kickVis,
                        ReportVisibility = reportVis,
                        OriginalDto = m
                    });
                }

                UpdateStartButtonState(amIHost, snapshot.Members.Length);
            }));
        }

        private void LoadFriends()
        {
            // ? Invitados no tienen amigos (no existen en BD)
            if (ClientSession.IsGuest)
            {
                Dispatcher.BeginInvoke(new Action(() => FriendsCollection.Clear()));
                return;
            }

            try
            {
                using (var client = new FriendServiceClient(
                    new InstanceContext(new FriendCallbackHandler()),
                    "NetTcpBinding_IFriendService"))
                {
                    var friends = client.GetFriends(_username);

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        FriendsCollection.Clear();

                        if (friends == null)
                            return;

                        foreach (var f in friends)
                        {
                            var status = f.ConnectionState
                                ? FriendStatus.Online
                                : FriendStatus.Offline;

                            string avatarFile = string.IsNullOrWhiteSpace(f.Avatar)
                                ? DefaultAvatarFile
                                : f.Avatar;

                            FriendsCollection.Add(new FriendViewModel
                            {
                                Username = f.Username,
                                Status = status,
                                AvatarFile = avatarFile,
                                AvatarSource = PathProvider.LoadAvatar(avatarFile)
                            });
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PreLobby.LoadFriends] {ex.Message}");
            }
        }

        private void UpdateStartButtonState(bool amIHost, int count)
        {
            if (!amIHost)
            {
                btnStartGame.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnStartGame.Visibility = Visibility.Visible;
                btnStartGame.IsEnabled = (count == 2 || count == 4 || count == 6);
            }
        }

        private void OnSendMessageClick(object sender, RoutedEventArgs e)
        {
            if (ClientSession.IsGuest)
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_GuestFeatureOnly"),
                    PopupType.Info
                );
                txtChatMessage.Clear();
                return;
            }

            string text = txtChatMessage.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return;

            _lobbyManager.SendChatMessage(text);
            txtChatMessage.Clear();
        }

        private void OnChatMessageReceived(string user, string message, string serverTimeIso)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                string localTime = DateTime.TryParse(
                        serverTimeIso,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime dt)
                    ? dt.ToLocalTime().ToString("HH:mm")
                    : DateTime.Now.ToString("HH:mm");

                chatContainer.Children.Add(new TextBlock
                {
                    Text = $"[{localTime}] {user}: {message}",
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 2, 0, 2)
                });

                if (chatContainer.Parent is ScrollViewer sv)
                    sv.ScrollToEnd();
            }));
        }


        private void OnChatTextChanged(object sender, TextChangedEventArgs e)
        {
            txtChatPlaceholder.Visibility = string.IsNullOrWhiteSpace(txtChatMessage.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void OnKickMemberClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is LobbyMemberViewModel vm)
                _lobbyManager.KickPlayer(vm.Username);
        }

        private void OnReportMemberClick(object sender, RoutedEventArgs e)
        {
            if (ClientSession.IsGuest)
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_GuestFeatureOnly"),
                    PopupType.Info
                );
                return;
            }

            if (!(sender is Button btn) || !(btn.DataContext is LobbyMemberViewModel vm))
                return;

            if (string.Equals(vm.Username, _username, StringComparison.OrdinalIgnoreCase))
                return;

            // ? No se reporta a invitados
            if (ClientSession.IsGuestUsername(vm.Username))
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage("msg_GuestFeatureOnly"),
                    PopupType.Info
                );
                return;
            }

            var req = new ReportPlayerRequest
            {
                CodigoLobby = _snapshot?.LobbyCode,
                IdPartida = null,
                ReporterUsername = _username,
                ReportedUsername = vm.Username,
                Reason = "Reported from lobby"
            };

            try
            {
                _lobbyManager.ReportPlayer(req);
                MessageHelper.ShowPopup(MessageKeys.PlayerReported, PopupType.Success);
            }
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

        private void OnInviteFriendClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.DataContext is FriendViewModel friend))
                return;

            int lobbyCode = _snapshot.LobbyCode;
            string hostUsername = _username;

            var result = _lobbyManager.InviteFriend(hostUsername, friend.Username, lobbyCode);

            if (!result.Success)
                MessageHelper.ShowFromResult(result);
        }

        public void ApplyBanInfo(BanInfoDto ban)
        {
            if (ban != null && ban.IsBanned)
            {
                MessageHelper.ShowPopup(MessageKeys.LobbyUserBanned, PopupType.Error);
                ExitCleanly();
            }
        }

        private void OnKicked(string reason)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageHelper.ShowPopup(MessageKeys.YouWereKicked, PopupType.Warning);
                ExitCleanly();
            }));
        }

        private void OnLobbyClosed(string reason)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageHelper.ShowPopup(MessageKeys.LobbyClosed, PopupType.Info);
                ExitCleanly();
            }));
        }

        private void OnGameStarting()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                UnsubscribeEvents();
                PreLobbyPageManager.Unregister(this);
                NavigationService?.Navigate(new MatchRoom(_snapshot.LobbyCode));
            }));
        }

        private void ExitCleanly()
        {
            UnsubscribeEvents();
            PreLobbyPageManager.Unregister(this);
            NavigationService?.GoBack();
        }

        private void OnStartGameClick(object sender, RoutedEventArgs e)
        {
            _lobbyManager.StartGame();
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            var res = _lobbyManager.LeaveLobby();

            if (!res.Success)
                ClientSession.Clear();

            ExitCleanly();
        }

        public class LobbyMemberViewModel
        {
            public int UserId { get; set; }
            public string Username { get; set; }
            public string DisplayName { get; set; }
            public string AvatarFile { get; set; }
            public ImageSource AvatarSource { get; set; }
            public bool IsHost { get; set; }
            public Visibility KickVisibility { get; set; }
            public Visibility ReportVisibility { get; set; }
            public LobbyMemberDto OriginalDto { get; set; }
        }

        public class FriendViewModel
        {
            public string Username { get; set; }
            public FriendStatus Status { get; set; }
            public string AvatarFile { get; set; }
            public ImageSource AvatarSource { get; set; }
        }
    }
}
