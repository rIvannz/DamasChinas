using DamasChinas_Client.UI.LobbyServiceProxy;
using DamasChinas_Client.UI.FriendServiceProxy; // Para FriendServiceClient y FriendDto
using DamasChinas_Client.UI.Utilities;
using System;
using System.Collections.ObjectModel;
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

        // Colecciones para el UI
        public ObservableCollection<LobbyMemberViewModel> MembersCollection { get; set; } = new ObservableCollection<LobbyMemberViewModel>();
        public ObservableCollection<FriendViewModel> FriendsCollection { get; set; } = new ObservableCollection<FriendViewModel>();

        public PreLobby(LobbySnapshotDto snapshot, string username, int userId)
        {
            InitializeComponent();

            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            PreLobbyPageManager.Register(this);

            _lobbyManager = LobbySession.Manager;
            _username = username;
            _userId = userId;

            // Bindings de las listas
            membersList.ItemsSource = MembersCollection;
            friendsList.ItemsSource = FriendsCollection;

            _lobbyManager.RegisterUser(username);
            _lobbyManager.RegisterLobby(snapshot.LobbyCode);

            SubscribeEvents();
            ApplySnapshot(snapshot);

            // Cargar lista de amigos
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
            Dispatcher.Invoke(() =>
            {
                _snapshot = snapshot;

                lblLobbyCode.Text = $"{MessageTranslator.GetLocalizedMessage(MessageKeys.LobbyCode)}: {snapshot.LobbyCode}";

                if (this.FindName("lblPlayerCount") is TextBlock lblCount)
                {
                    lblCount.Text = $"{snapshot.Members.Length} / {snapshot.MaxPlayers}";
                }

                MembersCollection.Clear();
                bool amIHost = snapshot.Members.Any(m => m.Username == _username && m.IsHost);

                foreach (var m in snapshot.Members)
                {
                    bool isMe = m.Username == _username;
                    string displayName = m.IsHost ? $"? {m.Username}" : m.Username;

                    Visibility kickVis = (amIHost && !isMe) ? Visibility.Visible : Visibility.Collapsed;
                    Visibility reportVis = (!isMe) ? Visibility.Visible : Visibility.Collapsed;

                    MembersCollection.Add(new LobbyMemberViewModel
                    {
                        UserId = m.UserId,
                        Username = m.Username,
                        DisplayName = displayName,
                        AvatarFile = m.AvatarFile,
                        IsHost = m.IsHost,
                        KickVisibility = kickVis,
                        ReportVisibility = reportVis,
                        OriginalDto = m
                    });
                }

                UpdateStartButtonState(amIHost, snapshot.Members.Length);
            });
        }

        private void LoadFriends()
        {
            // CORRECCIÓN: Instancia directa para evitar error de ServiceHelper
            try
            {
                using (var friendClient = new FriendServiceClient())
                {
                    var friends = friendClient.GetFriends(_username); // Usamos username como pide IFriendService

                    Dispatcher.Invoke(() =>
                    {
                        FriendsCollection.Clear();
                        if (friends != null)
                        {
                            foreach (var f in friends)
                            {
                                // CORRECCIÓN: Mapeo manual de ConnectionState(bool) a Status(Enum)
                                var status = f.ConnectionState ? FriendStatus.Online : FriendStatus.Offline;

                                FriendsCollection.Add(new FriendViewModel
                                {
                                    Username = f.Username,
                                    Status = status
                                    // Agrega Avatar si lo necesitas en el XAML
                                });
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading friends: {ex.Message}");
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

        // ======================= CHAT =======================

        private void OnSendMessageClick(object sender, RoutedEventArgs e)
        {
            string text = txtChatMessage.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            _lobbyManager.SendChatMessage(text);
            txtChatMessage.Clear();
        }

        private void OnChatMessageReceived(string user, string message, string serverTimeIso)
        {
            Dispatcher.Invoke(() =>
            {
                string localTime = DateTime.TryParse(serverTimeIso, out DateTime dt)
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
                {
                    sv.ScrollToEnd();
                }
            });
        }

        private void OnChatTextChanged(object sender, TextChangedEventArgs e)
        {
            txtChatPlaceholder.Visibility = string.IsNullOrWhiteSpace(txtChatMessage.Text)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        // ======================= ACCIONES =======================

        private void OnKickMemberClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is LobbyMemberViewModel vm)
            {
                _lobbyManager.KickPlayer(vm.Username);
            }
        }

        private void OnReportMemberClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is LobbyMemberViewModel vm)
            {
                if (vm.Username == _username) return;

                var req = new ReportPlayerRequest
                {
                    LobbyCode = _snapshot.LobbyCode,
                    ReporterUsername = _username,
                    ReportedUsername = vm.Username,
                    Reason = "Reported from lobby"
                };

                _lobbyManager.ReportPlayer(req);
                MessageHelper.ShowPopup(MessageKeys.PlayerReported, PopupType.Success);
            }
        }

        private void OnStartGameClick(object sender, RoutedEventArgs e)
        {
            _lobbyManager.StartGame();
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            var res = _lobbyManager.LeaveLobby();

            if (!res.Success)
            {
                ClientSession.Clear();
            }

            ExitCleanly();
        }

        private void OnInviteFriendClick(object sender, RoutedEventArgs e)
        {
            // CORRECCIÓN: Usar FriendViewModel
            if (sender is Button btn && btn.DataContext is FriendViewModel friend)
            {
                if (friend.Status != FriendStatus.Online)
                {
                    // Podrías mostrar un mensaje de error aquí
                    return;
                }

                // _lobbyManager.InviteFriend(...)
            }

            MessageHelper.ShowPopup(MessageKeys.ChatComingSoon, PopupType.Info);
        }

        // ======================= EVENTOS SERVER =======================

        public void ApplyBanInfo(BanInfoDto ban)
        {
            if (ban?.IsBanned == true)
            {
                MessageHelper.ShowPopup(MessageKeys.LobbyUserBanned, PopupType.Error);
                ExitCleanly();
            }
        }

        private void OnKicked(string reason)
        {
            Dispatcher.Invoke(() =>
            {
                MessageHelper.ShowPopup(MessageKeys.YouWereKicked, PopupType.Warning);
                ExitCleanly();
            });
        }

        private void OnLobbyClosed(string reason)
        {
            Dispatcher.Invoke(() =>
            {
                MessageHelper.ShowPopup(MessageKeys.LobbyClosed, PopupType.Info);
                ExitCleanly();
            });
        }

        private void OnGameStarting()
        {
            Dispatcher.Invoke(() =>
            {
                UnsubscribeEvents();
                PreLobbyPageManager.Unregister(this);
                NavigationService?.Navigate(new MatchRoom(_snapshot.LobbyCode));
            });
        }

        private void ExitCleanly()
        {
            UnsubscribeEvents();
            PreLobbyPageManager.Unregister(this);
            NavigationService?.GoBack();
        }

        // ======================= VIEW MODELS =======================

        public class LobbyMemberViewModel
        {
            public int UserId { get; set; }
            public string Username { get; set; }
            public string DisplayName { get; set; }
            public string AvatarFile { get; set; }
            public bool IsHost { get; set; }
            public Visibility KickVisibility { get; set; }
            public Visibility ReportVisibility { get; set; }
            public LobbyMemberDto OriginalDto { get; set; }
        }

        // CORRECCIÓN: ViewModel local para adaptar DTO a UI
        public class FriendViewModel
        {
            public string Username { get; set; }
            public FriendStatus Status { get; set; } // Necesario para el Converter
        }
    }
}