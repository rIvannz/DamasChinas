using DamasChinas_Client.UI.Callbacks;
using DamasChinas_Client.UI.LobbyServiceProxy;
using DamasChinas_Client.UI.MatchServiceProxy;
using DamasChinas_Client.UI.Utilities;
using DamasChinas_Shared.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using AccountProxy = DamasChinas_Client.UI.AccountManagerServiceProxy;

namespace DamasChinas_Client.UI.Pages
{
    public partial class MatchRoom : Page
    {
        private const int ReconnectWindowSeconds = 20;
        private const int BoardRadius = 4;

        private const double HexSize = 32.0;
        private const double CenterX = 600.0;
        private const double CenterY = 600.0;

        private readonly int _lobbyCode;
        private readonly string _myUsername;

        private bool _matchEnded;
        private bool _isReconnecting;
        private DateTime _reconnectDeadlineUtc;

        private bool _wasRemovedByServer;
        private string _removedReasonKey;

        private MatchServiceClient _proxy;
        private MatchCallbackHandler _callbackHandler;
        private LobbySnapshotDto _lobbySnapshot;

        private Dictionary<Point, Ellipse> _holesVisuals;
        private Dictionary<Point, Ellipse> _marblesVisuals;
        private Point? _selectedCoord;
        private string _currentPlayerTurn;

        public ObservableCollection<PlayerViewModel> Players { get; set; }
        private readonly Dictionary<string, Brush> _userColors;
        private readonly List<Brush> _availableColors;

        public MatchRoom(int lobbyCode)
        {
            InitializeComponent();

            _lobbyCode = lobbyCode;
            _myUsername = ClientSession.CurrentProfile.Username;

            Players = new ObservableCollection<PlayerViewModel>();
            _holesVisuals = new Dictionary<Point, Ellipse>();
            _marblesVisuals = new Dictionary<Point, Ellipse>();
            _userColors = new Dictionary<string, Brush>();

            _availableColors = new List<Brush>
            {
                Brushes.Red,
                Brushes.Green,
                Brushes.Blue,
                Brushes.Yellow,
                Brushes.Orange,
                Brushes.Purple
            };

            icPlayers.ItemsSource = Players;
            txtLobbyCode.Text = _lobbyCode.ToString();

            Loaded += OnPageLoaded;
            Unloaded += OnPageUnloaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _matchEnded = false;
                _wasRemovedByServer = false;
                _removedReasonKey = null;

                SoundManager.Initialize();

                Players.Clear();
                _userColors.Clear();

                _lobbySnapshot = LobbySession.Manager.GetCurrentLobby(_myUsername);
                SetupPlayersMetadata();

                DrawBoardBackground();
                DrawPlayerLabels();

                LobbySession.Manager.ChatMessageReceived += OnChatMessageReceived;

                InitializeMatchConnection();
            }
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
                NavigateToMenu();
            }
        }

        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            _matchEnded = true;
            ClientSession.MarkIntentionalDisconnect();

            try
            {
                DetachChatEventsSafely();
                LeaveMatchSafely();
            }
            finally
            {
                SafeDisposeProxyAndDetachEvents();
                ClientSession.ResetIntentionalDisconnect();
            }
        }

        private void DetachChatEventsSafely()
        {
            try
            {
                LobbySession.Manager.ChatMessageReceived -= OnChatMessageReceived;
            }
            catch
            {
                Debug.WriteLine("[MatchRoom.DetachChatEventsSafely] Failed to detach ChatMessageReceived");
            }
        }

        private void LeaveMatchSafely()
        {
            try
            {
                if (_proxy == null)
                {
                    return;
                }

                if (_proxy.State != CommunicationState.Opened)
                {
                    return;
                }

                _proxy.LeaveMatch(_lobbyCode, _myUsername);
            }
            catch (EndpointNotFoundException)
            {
                Debug.WriteLine("[MatchRoom.LeaveMatchSafely] EndpointNotFound");
            }
            catch (CommunicationException)
            {
                Debug.WriteLine("[MatchRoom.LeaveMatchSafely] CommunicationException");
            }
            catch (TimeoutException)
            {
                Debug.WriteLine("[MatchRoom.LeaveMatchSafely] TimeoutException");
            }
            catch
            {
                Debug.WriteLine("[MatchRoom.LeaveMatchSafely] Unknown error");
            }
        }

        private void SafeDisposeProxyAndDetachEvents()
        {
            MatchServiceClient proxy = _proxy;
            _proxy = null;

            try
            {
                if (proxy == null)
                {
                    return;
                }

                DetachProxyEventsSafely(proxy);
                CloseOrAbortProxySafely(proxy);
            }
            catch
            {
                Debug.WriteLine("[MatchRoom.SafeDisposeProxyAndDetachEvents] Unexpected error");
            }
        }

        private void InitializeMatchConnection()
        {
            _callbackHandler = new MatchCallbackHandler(this);
            var context = new InstanceContext(_callbackHandler);

            _proxy = new MatchServiceClient(context);
            AttachProxyEventsSafely(_proxy);

            try
            {
                var result = _proxy.ConnectToMatch(_lobbyCode, _myUsername);

                if (!result.Success)
                {
                    MessageHelper.ShowFromResult(result);
                    return;
                }

                var state = _proxy.GetMatchState(_lobbyCode);
                if (state != null)
                {
                    UpdateGameState(state);
                }
            }
            catch (EndpointNotFoundException ex)
            {
                Debug.WriteLine($"[MatchRoom.InitializeMatchConnection] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[MatchRoom.InitializeMatchConnection] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"[MatchRoom.InitializeMatchConnection] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
            }
        }

        private void OnConnectionLost(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_matchEnded || _isReconnecting)
                {
                    return;
                }

                if (ClientSession.IsIntentionalDisconnect)
                {
                    return;
                }

                _isReconnecting = true;
                StartReconnectFlow();
            }));
        }

        private async void StartReconnectFlow()
        {
            bool reconnected = false;

            try
            {
                await Dispatcher.InvokeAsync(ShowReconnectingOverlay);

                _reconnectDeadlineUtc = DateTime.UtcNow.AddSeconds(ReconnectWindowSeconds);

                while (DateTime.UtcNow < _reconnectDeadlineUtc && !_wasRemovedByServer)
                {
                    UpdateReconnectCountdownUI();

                    reconnected = await TryReconnectOnce().ConfigureAwait(false);
                    if (reconnected)
                    {
                        return;
                    }

                    await Task.Delay(1000).ConfigureAwait(false);
                }
            }
            catch (EndpointNotFoundException ex)
            {
                Debug.WriteLine($"[MatchRoom.StartReconnectFlow] {ex.Message}");
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine($"[MatchRoom.StartReconnectFlow] {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine($"[MatchRoom.StartReconnectFlow] {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[MatchRoom.StartReconnectFlow] {ex.Message}");
            }
            finally
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    HideReconnectingOverlay();
                    _isReconnecting = false;
                });
            }

            await Dispatcher.InvokeAsync(() =>
            {
                if (_wasRemovedByServer)
                {
                    MessageHelper.ShowPopup(
                        _removedReasonKey ?? MessageKeys.MatchRemovedAfterDisconnect,
                        PopupType.Warning);
                }
                else if (!reconnected)
                {
                    MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
                }

                CleanupAndExitAfterReconnectFail();
                NavigateToMenu();
            });
        }



        private void UpdateReconnectCountdownUI()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                int remaining = (int)Math.Ceiling((_reconnectDeadlineUtc - DateTime.UtcNow).TotalSeconds);
                if (remaining < 0)
                {
                    remaining = 0;
                }

                try
                {
                    if (txtReconnectCountdown != null)
                    {
                        txtReconnectCountdown.Text = remaining.ToString();
                    }
                }
                catch
                {
                    Debug.WriteLine("[MatchRoom.UpdateReconnectCountdownUI] txtReconnectCountdown not available");
                }
            }));
        }

        private void ShowReconnectingOverlay()
        {
            ReconnectingOverlay.Visibility = Visibility.Visible;
        }

        private void HideReconnectingOverlay()
        {
            ReconnectingOverlay.Visibility = Visibility.Collapsed;
        }

        private async Task<bool> TryReconnectOnce()
        {
            if (_wasRemovedByServer)
            {
                return false;
            }

            MatchServiceClient newProxy = null;

            try
            {
                var oldProxy = _proxy;
                _proxy = null;

                CloseAndDetachProxySafely(oldProxy);

                _callbackHandler = new MatchCallbackHandler(this);
                var context = new InstanceContext(_callbackHandler);

                newProxy = new MatchServiceClient(context);
                AttachProxyEventsSafely(newProxy);

                var result = await Task.Run(() => newProxy.ConnectToMatch(_lobbyCode, _myUsername))
                                      .ConfigureAwait(false);

                if (result == null || !result.Success || _wasRemovedByServer)
                {
                    AbortProxySafely(newProxy);
                    return false;
                }

                var state = await Task.Run(() => newProxy.GetMatchState(_lobbyCode))
                                      .ConfigureAwait(false);

                if (state != null)
                {
                    await Dispatcher.InvokeAsync(() => UpdateGameState(state));
                }

                _proxy = newProxy;

                await Dispatcher.InvokeAsync(HideReconnectingOverlay);

                return true;
            }
            catch
            {
                AbortProxySafely(newProxy);
                return false;
            }
        }


        private void AttachProxyEventsSafely(MatchServiceClient proxy)
        {
            try
            {
                if (proxy == null)
                {
                    return;
                }

                proxy.InnerChannel.Faulted += OnConnectionLost;
                proxy.InnerChannel.Closed += OnConnectionLost;
            }
            catch
            {
                Debug.WriteLine("[MatchRoom.AttachProxyEventsSafely] Failed");
            }
        }

        private void DetachProxyEventsSafely(MatchServiceClient proxy)
        {
            try
            {
                if (proxy == null)
                {
                    return;
                }

                proxy.InnerChannel.Faulted -= OnConnectionLost;
                proxy.InnerChannel.Closed -= OnConnectionLost;
            }
            catch
            {
                Debug.WriteLine("[MatchRoom.DetachProxyEventsSafely] Failed");
            }
        }

        private void CloseAndDetachProxySafely(MatchServiceClient proxy)
        {
            if (proxy == null)
            {
                return;
            }

            DetachProxyEventsSafely(proxy);
            CloseOrAbortProxySafely(proxy);
        }

        private static void CloseOrAbortProxySafely(MatchServiceClient proxy)
        {
            try
            {
                if (proxy == null)
                {
                    return;
                }

                if (proxy.State == CommunicationState.Faulted)
                {
                    proxy.Abort();
                    return;
                }

                proxy.Close();
            }
            catch
            {
                AbortProxySafely(proxy);
            }
        }

        private static void AbortProxySafely(MatchServiceClient proxy)
        {
            try
            {
                proxy?.Abort();
            }
            catch
            {
                Debug.WriteLine("[MatchRoom.AbortProxySafely] Abort failed");
            }
        }

        private void CleanupAndExitAfterReconnectFail()
        {
            try
            {
                LobbySession.Manager.ChatMessageReceived -= OnChatMessageReceived;
            }
            catch
            {
                Debug.WriteLine("[MatchRoom.CleanupAndExitAfterReconnectFail] Failed to detach chat");
            }

            try
            {
                LobbySession.Reset();
            }
            catch
            {
                Debug.WriteLine("[MatchRoom.CleanupAndExitAfterReconnectFail] LobbySession.Reset failed");
            }

            try
            {
                _proxy?.Abort();
            }
            catch
            {
                Debug.WriteLine("[MatchRoom.CleanupAndExitAfterReconnectFail] Abort proxy failed");
            }
        }

        private void SetupPlayersMetadata()
        {
            if (_lobbySnapshot?.Members == null)
            {
                return;
            }

            var sortedMembers = _lobbySnapshot.Members
                .OrderBy(m => m.Username)
                .ToList();

            for (int i = 0; i < sortedMembers.Count; i++)
            {
                AddPlayer(sortedMembers[i], i);
            }
        }

        private void AddPlayer(LobbyMemberDto member, int index)
        {
            Brush color = GetPlayerColor(index);

            _userColors[member.Username] = color;

            Players.Add(new PlayerViewModel
            {
                Username = member.Username,
                DisplayName = BuildDisplayName(member),
                ColorBrush = color,
                AvatarPath = PathProvider.LoadAvatar(member.AvatarFile),
                IsTurnVisible = Visibility.Collapsed,
                StatusText = MessageTranslator.GetLocalizedMessage(MessageKeys.StatusWaiting),
                ReportVisibility = CanReportPlayer(member.Username)
                    ? Visibility.Visible
                    : Visibility.Collapsed
            });
        }

        private Brush GetPlayerColor(int index)
        {
            return _availableColors[index % _availableColors.Count];
        }

        private static string BuildDisplayName(LobbyMemberDto member)
        {
            return member.IsHost
                ? $"★ {member.Username}"
                : member.Username;
        }

        private bool CanReportPlayer(string username)
        {
            if (ClientSession.IsGuest)
            {
                return false;
            }

            if (string.Equals(username, _myUsername, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (ClientSession.IsGuestUsername(username))
            {
                return false;
            }

            return true;
        }

        private static List<(int X, int Y, int Z)> GenerateBoardCubeCoordinates()
        {
            var cells = new List<(int X, int Y, int Z)>();

            int centerRadius = BoardRadius;
            int maxCoord = centerRadius * 2;

            for (int x = -maxCoord; x <= maxCoord; x++)
            {
                for (int y = -maxCoord; y <= maxCoord; y++)
                {
                    int z = -x - y;

                    if (x + y + z != 0)
                    {
                        continue;
                    }

                    int ax = Math.Abs(x);
                    int ay = Math.Abs(y);
                    int az = Math.Abs(z);
                    int max = Math.Max(ax, Math.Max(ay, az));

                    if (max <= centerRadius)
                    {
                        cells.Add((x, y, z));
                        continue;
                    }

                    int[] sorted = { ax, ay, az };
                    Array.Sort(sorted);

                    bool isArmCell =
                        max > centerRadius &&
                        max <= centerRadius * 2 &&
                        sorted[1] <= centerRadius;

                    if (isArmCell)
                    {
                        cells.Add((x, y, z));
                    }
                }
            }

            return cells;
        }

        private void DrawBoardBackground()
        {
            BoardCanvas.Children.Clear();
            _holesVisuals.Clear();
            _marblesVisuals.Clear();

            var cubeCoords = GenerateBoardCubeCoordinates();

            foreach (var c in cubeCoords)
            {
                int q = c.X;
                int r = c.Z;
                DrawHole(q, r);
            }
        }

        private static void DrawPlayerLabels()
        {
            Debug.WriteLine("[MatchRoom.DrawPlayerLabels] Not implemented");
        }

        private static Point HexToPixel(int q, int r)
        {
            double x = HexSize * (Math.Sqrt(3) * q + (Math.Sqrt(3) / 2.0) * r);
            double y = HexSize * (1.5 * r);
            return new Point(CenterX + x, CenterY + y);
        }

        private Point GetMarbleTopLeft(Point boardCoord, double width, double height)
        {
            Point center = HexToPixel((int)boardCoord.X, (int)boardCoord.Y);
            return new Point(center.X - (width / 2), center.Y - (height / 2));
        }

        private static void EnsureTransforms(Ellipse marble)
        {
            if (marble == null)
            {
                return;
            }

            if (marble.RenderTransform is TransformGroup)
            {
                return;
            }

            var group = new TransformGroup();
            group.Children.Add(new ScaleTransform(1, 1));
            group.Children.Add(new TranslateTransform(0, 0));

            marble.RenderTransform = group;
            marble.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        private void AnimateMarbleMove(Ellipse marble, Point toCoord, Action onCompleted)
        {
            if (marble == null)
            {
                return;
            }

            EnsureTransforms(marble);

            double fromLeft = Canvas.GetLeft(marble);
            double fromTop = Canvas.GetTop(marble);

            Point toTopLeft = GetMarbleTopLeft(toCoord, marble.Width, marble.Height);

            var duration = new Duration(TimeSpan.FromMilliseconds(220));

            var animLeft = new DoubleAnimation
            {
                From = fromLeft,
                To = toTopLeft.X,
                Duration = duration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var animTop = new DoubleAnimation
            {
                From = fromTop,
                To = toTopLeft.Y,
                Duration = duration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var pop = new DoubleAnimationUsingKeyFrames();
            pop.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))));
            pop.KeyFrames.Add(new EasingDoubleKeyFrame(1.15,KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(120)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            });

            pop.KeyFrames.Add(new EasingDoubleKeyFrame(
                1.0,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(220)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            });


            var group = (TransformGroup)marble.RenderTransform;
            var scale = (ScaleTransform)group.Children[0];

            int completedCount = 0;
            EventHandler whenDone = (s, e) =>
            {
                completedCount++;
                if (completedCount < 2)
                {
                    return;
                }

                onCompleted?.Invoke();
            };

            animLeft.Completed += whenDone;
            animTop.Completed += whenDone;

            marble.BeginAnimation(Canvas.LeftProperty, animLeft);
            marble.BeginAnimation(Canvas.TopProperty, animTop);

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, pop);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, pop);
        }

        private void DrawHole(int q, int r)
        {
            Point px = HexToPixel(q, r);

            var hole = new Ellipse
            {
                Width = HexSize * 0.8,
                Height = HexSize * 0.8,
                Fill = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
                Stroke = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)),
                StrokeThickness = 1,
                Tag = new Point(q, r)
            };

            hole.MouseLeftButtonDown += OnBoardClick;

            Canvas.SetLeft(hole, px.X - (hole.Width / 2));
            Canvas.SetTop(hole, px.Y - (hole.Height / 2));
            BoardCanvas.Children.Add(hole);

            _holesVisuals[new Point(q, r)] = hole;
        }

        private void PlaceMarble(int q, int r, Brush color)
        {
            var point = new Point(q, r);

            if (!_holesVisuals.TryGetValue(point, out _))
            {
                Debug.WriteLine("[MatchRoom.PlaceMarble] hole not found");
                return;
            }

            if (_marblesVisuals.ContainsKey(point))
            {
                BoardCanvas.Children.Remove(_marblesVisuals[point]);
                _marblesVisuals.Remove(point);
            }

            Point px = HexToPixel(q, r);

            var marble = new Ellipse
            {
                Width = HexSize * 0.7,
                Height = HexSize * 0.7,
                Fill = color,
                Stroke = Brushes.White,
                StrokeThickness = 1,
                Tag = point,
                IsHitTestVisible = false,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 5,
                    ShadowDepth = 2,
                    Opacity = 0.8
                }
            };

            Canvas.SetLeft(marble, px.X - (marble.Width / 2));
            Canvas.SetTop(marble, px.Y - (marble.Height / 2));
            BoardCanvas.Children.Add(marble);

            _marblesVisuals[point] = marble;
        }

        private void UpdateGameState(MatchStateDto state)
        {
            foreach (var m in _marblesVisuals.Values)
            {
                BoardCanvas.Children.Remove(m);
            }

            _marblesVisuals.Clear();

            foreach (var entry in state.BoardPieces)
            {
                string user = entry.Key;

                if (!_userColors.TryGetValue(user, out var color))
                {
                    continue;
                }

                foreach (var coord in entry.Value)
                {
                    PlaceMarble(coord.X, coord.Z, color);
                }
            }

            _currentPlayerTurn = state.CurrentTurnPlayer;
            UpdatePlayerTurnUI();
        }

        private void UpdatePlayerTurnUI()
        {
            string turnText = MessageTranslator.GetLocalizedMessage(MessageKeys.StatusTurn);
            string waitText = MessageTranslator.GetLocalizedMessage(MessageKeys.StatusWaiting);

            foreach (var player in Players)
            {
                bool isTurn = player.Username == _currentPlayerTurn;
                player.IsTurnVisible = isTurn ? Visibility.Visible : Visibility.Collapsed;
                player.StatusText = isTurn ? turnText : waitText;
            }

            icPlayers.Items.Refresh();
        }

        public void HandlePlayerMoved(TurnChangeDto turn)
        {
            if (_matchEnded)
            {
                return;
            }

            if (turn.BoardState != null)
            {
                UpdateGameState(turn.BoardState);
                return;
            }

            if (turn.MoveOrigin == null || turn.MoveDestination == null)
            {
                return;
            }

            var from = new Point(turn.MoveOrigin.X, turn.MoveOrigin.Z);
            var to = new Point(turn.MoveDestination.X, turn.MoveDestination.Z);

            if (_marblesVisuals.TryGetValue(from, out var marble))
            {
                SoundManager.PlayMoveEffect();

                _marblesVisuals.Remove(from);

                AnimateMarbleMove(marble, to, () =>
                {
                    try
                    {
                        marble.Tag = to;
                        _marblesVisuals[to] = marble;
                    }
                    catch (CommunicationException ex)
                    {
                        Debug.WriteLine($"[MatchRoom.HandlePlayerMoved] {ex.Message}");
                    }
                });
            }

            _currentPlayerTurn = turn.NextPlayer;
            UpdatePlayerTurnUI();
        }

        public void HandleMatchEnded(string winner)
        {
            if (_matchEnded)
            {
                return;
            }

            _matchEnded = true;

            VictoryOverlay.Visibility = Visibility.Visible;

            txtFinalTitle.Text = MessageTranslator.GetLocalizedMessage(MessageKeys.GameFinishedTitle);

            string winnerLabel = MessageTranslator.GetLocalizedMessage(MessageKeys.GameWinnerLabel);
            txtWinnerName.Text = $"{winnerLabel}: {winner}";
        }

        public void HandlePlayerLeft(string username)
        {
            if (_matchEnded)
            {
                return;
            }

            string msg = MessageTranslator.GetLocalizedMessage(MessageKeys.PlayerLeftMatch);
            AddChatMessage("Sistema", $"{username} {msg}");

            var p = Players.FirstOrDefault(x => x.Username == username);
            if (p != null)
            {
                p.StatusText = MessageTranslator.GetLocalizedMessage(MessageKeys.StatusDisconnected);
            }

            icPlayers.Items.Refresh();
        }

        public void HandleError(string serverMsg)
        {
            if (string.IsNullOrWhiteSpace(serverMsg))
            {
                return;
            }

            if (!TryParseServerEvent(serverMsg, out string evt, out string user))
            {
                MessageHelper.ShowPopup(serverMsg, PopupType.Warning);
                return;
            }

            if (evt == "DISCONNECTED")
            {
                HandleDisconnected(user);
                return;
            }

            if (evt == "RECONNECTED")
            {
                HandleReconnected(user);
                return;
            }

            if (evt == "REMOVED")
            {
                HandleRemoved(user);
                return;
            }

            MessageHelper.ShowPopup(serverMsg, PopupType.Warning);
        }

        private static bool TryParseServerEvent(string msg, out string evt, out string user)
        {
            evt = null;
            user = null;

            int idx = msg.IndexOf("::", StringComparison.Ordinal);
            if (idx <= 0)
            {
                return false;
            }

            evt = msg.Substring(0, idx).Trim();
            user = msg.Substring(idx + 2).Trim();

            if (string.IsNullOrWhiteSpace(evt) || string.IsNullOrWhiteSpace(user))
            {
                return false;
            }

            return true;
        }

        private void HandleDisconnected(string user)
        {
            var p = Players.FirstOrDefault(x =>
                string.Equals(x.Username, user, StringComparison.OrdinalIgnoreCase));

            if (p != null)
            {
                p.StatusText = MessageTranslator.GetLocalizedMessage(MessageKeys.StatusDisconnected);
                icPlayers.Items.Refresh();
            }

            AddChatMessage("Sistema",
                $"{user} {MessageTranslator.GetLocalizedMessage(MessageKeys.StatusDisconnected)}");
        }

        private void HandleReconnected(string user)
        {
            var p = Players.FirstOrDefault(x =>
                string.Equals(x.Username, user, StringComparison.OrdinalIgnoreCase));

            if (p != null)
            {
                p.StatusText = MessageTranslator.GetLocalizedMessage(MessageKeys.StatusWaiting);
                icPlayers.Items.Refresh();
            }

            AddChatMessage("Sistema",
                $"{user} {MessageTranslator.GetLocalizedMessage(MessageKeys.PlayerReconnected)}");
        }

        private void HandleRemoved(string user)
        {
            AddChatMessage("Sistema",
                $"{user} {MessageTranslator.GetLocalizedMessage(MessageKeys.PlayerLeftMatch)}");

            if (string.Equals(user, _myUsername, StringComparison.OrdinalIgnoreCase))
            {
                _wasRemovedByServer = true;
                _removedReasonKey = MessageKeys.MatchRemovedAfterDisconnect;
            }
        }

        private void OnBoardClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Ellipse hole))
            {
                return;
            }

            if (_matchEnded)
            {
                return;
            }

            Point clickedCoord = (Point)hole.Tag;

            if (_currentPlayerTurn != _myUsername)
            {
                return;
            }

            if (_marblesVisuals.ContainsKey(clickedCoord))
            {
                var marble = _marblesVisuals[clickedCoord];

                if (marble.Fill == _userColors[_myUsername])
                {
                    SelectPiece(clickedCoord, hole);
                }

                return;
            }

            if (_selectedCoord.HasValue)
            {
                _ = SendMove(_selectedCoord.Value, clickedCoord);
            }
        }

        private void SelectPiece(Point coord, Ellipse hole)
        {
            DeselectPiece();

            _selectedCoord = coord;
            hole.Stroke = Brushes.Yellow;
            hole.StrokeThickness = 3;
        }

        private void DeselectPiece()
        {
            if (_selectedCoord.HasValue &&
                _holesVisuals.TryGetValue(_selectedCoord.Value, out var hole))
            {
                hole.Stroke = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
                hole.StrokeThickness = 1;
            }

            _selectedCoord = null;
        }

        private MoveRequestDto CreateMoveRequest(Point origin, Point dest)
        {
            int ox = (int)origin.X;
            int oz = (int)origin.Y;
            int dx = (int)dest.X;
            int dz = (int)dest.Y;

            return new MoveRequestDto
            {
                LobbyCode = _lobbyCode,
                Username = _myUsername,
                Origin = new HexCoordinateDto
                {
                    X = ox,
                    Z = oz,
                    Y = -(ox + oz)
                },
                Destination = new HexCoordinateDto
                {
                    X = dx,
                    Z = dz,
                    Y = -(dx + dz)
                }
            };
        }

        private async Task SendMove(Point origin, Point dest)
        {
            if (_proxy == null || _proxy.State != CommunicationState.Opened)
            {
                return;
            }

            var req = CreateMoveRequest(origin, dest);

            try
            {
                var result = await Task.Run(() => _proxy.MovePiece(req));

                if (!result.Success)
                {
                    MessageHelper.ShowFromResult(result);
                }

                DeselectPiece();
            }
            catch (EndpointNotFoundException ex)
            {
                MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
                Debug.WriteLine($"[MatchRoom.SendMove] {ex.Message}");
                NavigateToMenu();
            }
            catch (CommunicationException ex)
            {
                MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
                Debug.WriteLine($"[MatchRoom.SendMove] {ex.Message}");
                NavigateToMenu();
            }
            catch (TimeoutException ex)
            {
                MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
                Debug.WriteLine($"[MatchRoom.SendMove] {ex.Message}");
                NavigateToMenu();
            }
        }

        private void OnSendMessageClick(object sender, RoutedEventArgs e)
        {
            if (ClientSession.IsGuest)
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage(MessageKeys.GuestFeatureOnly),
                    PopupType.Info);

                txtChatInput.Clear();
                return;
            }

            string msg = txtChatInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(msg))
            {
                return;
            }

            LobbySession.Manager.SendChatMessage(msg);
            txtChatInput.Clear();
        }

        public void AddChatMessage(string user, string msg)
        {
            OnChatMessageReceived(user, msg, null);
        }

        private void OnChatMessageReceived(string user, string msg, string time)
        {
            string localTime = DateTime.Now.ToString("HH:mm");

            Dispatcher.BeginInvoke(new Action(() =>
            {
                var tb = new TextBlock
                {
                    Text = $"[{localTime}] {user}: {msg}",
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 2, 0, 2)
                };

                ChatContainer.Children.Add(tb);
                ChatScroll.ScrollToEnd();
            }));
        }

        private void OnLeaveMatchClick(object sender, RoutedEventArgs e)
        {
            if (MessageHelper.ShowConfirm("confirmExitLobby"))
            {
                try
                {
                    _proxy?.LeaveMatch(_lobbyCode, _myUsername);
                }
                catch (EndpointNotFoundException)
                {
                    MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
                }
                catch (CommunicationException)
                {
                    MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
                }
                catch (TimeoutException)
                {
                    MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
                }

                NavigateToMenu();
            }
        }

        private void OnBackToMenuClick(object sender, RoutedEventArgs e)
        {
            NavigateToMenu();
        }

        private void OnSoundClick(object sender, RoutedEventArgs e)
        {
            var main = Application.Current.MainWindow;

            var window = new Window
            {
                Owner = main,
                Content = new ConfiSound(),
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Width = main?.ActualWidth ?? 1280,
                Height = main?.ActualHeight ?? 720,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                WindowState = main?.WindowState ?? WindowState.Normal
            };

            window.ShowDialog();
        }

        private void OnLanguageClick(object sender, RoutedEventArgs e)
        {
            var main = Application.Current.MainWindow;

            var window = new Window
            {
                Owner = main,
                Content = new SelectLanguage(),
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Width = main?.ActualWidth ?? 1280,
                Height = main?.ActualHeight ?? 720,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                WindowState = main?.WindowState ?? WindowState.Normal
            };

            window.ShowDialog();
        }

        private void NavigateToMenu()
        {
            if (ClientSession.IsGuest)
            {
                var target = new MenuGuest();
                if (NavigationService != null)
                {
                    NavigationService.Navigate(target);
                }
                else
                {
                    Application.Current.MainWindow.Content = target;
                }

                return;
            }

            var current = ClientSession.CurrentProfile;

            var menuProfile = new AccountProxy.PublicProfile
            {
                IdUser = current.IdUser,
                Username = current.Username,
                AvatarFile = current.AvatarFile
            };

            var targetPage = new MenuRegisteredPlayer(menuProfile);

            if (NavigationService != null)
            {
                NavigationService.Navigate(targetPage);
            }
            else
            {
                Application.Current.MainWindow.Content = targetPage;
            }
        }

        public void HandleBanStatusUpdated(BanInfoDto banInfo)
        {
            if (banInfo == null || !banInfo.IsBanned)
            {
                return;
            }

            try
            {
                PendingBanNotificationStore.Save(banInfo);

                _matchEnded = true;

                string msg = PendingBanNotificationStore.BuildBanMessage(banInfo);
                MessageHelper.ShowPopup(msg, PopupType.Error);

                PendingBanNotificationStore.Clear();
            }
            catch
            {
                try
                {
                    PendingBanNotificationStore.Save(banInfo);
                }
                catch
                {
                    Debug.WriteLine("[MatchRoom.HandleBanStatusUpdated] Save failed");
                }
            }
            finally
            {
                try
                {
                    _proxy?.Abort();
                }
                catch (EndpointNotFoundException ex)
                {
                    Debug.WriteLine($"[MatchRoom.HandleBanStatusUpdated] {ex.Message}");
                    MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
                }
                catch (CommunicationException ex)
                {
                    Debug.WriteLine($"[MatchRoom.HandleBanStatusUpdated] {ex.Message}");
                    MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
                }
                catch (TimeoutException ex)
                {
                    Debug.WriteLine($"[MatchRoom.HandleBanStatusUpdated] {ex.Message}");
                    MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
                }

                NavigateToMenu();
            }
        }

        private void OnReportPlayerInMatchClick(object sender, RoutedEventArgs e)
        {
            if (!TryBuildReportRequestFromMatch(sender, out ReportPlayerRequest req))
            {
                return;
            }

            try
            {
                LobbySession.Manager.ReportPlayer(req);
                MessageHelper.ShowPopup(MessageKeys.PlayerReported, PopupType.Success);
            }
            catch
            {
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

        private bool TryBuildReportRequestFromMatch(object sender, out ReportPlayerRequest request)
        {
            request = null;

            if (ClientSession.IsGuest)
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage(MessageKeys.GuestFeatureOnly),
                    PopupType.Info);

                return false;
            }

            if (!(sender is Button btn) || !(btn.Tag is PlayerViewModel vm))
            {
                return false;
            }

            if (ClientSession.IsGuestUsername(vm.Username) ||
                string.Equals(vm.Username, _myUsername, StringComparison.OrdinalIgnoreCase))
            {
                MessageHelper.ShowPopup(
                    MessageTranslator.GetLocalizedMessage(MessageKeys.GuestFeatureOnly),
                    PopupType.Info);

                return false;
            }

            string reasonKey = ShowReportReasonDialogAndGetKey();
            if (string.IsNullOrWhiteSpace(reasonKey))
            {
                return false;
            }

            request = new ReportPlayerRequest
            {
                CodigoLobby = _lobbyCode,
                IdPartida = null,
                ReporterUsername = _myUsername,
                ReportedUsername = vm.Username,
                Reason = reasonKey
            };

            return true;
        }

        private string ShowReportReasonDialogAndGetKey()
        {
            var main = Application.Current.MainWindow;

            var popup = new DamasChinas_Client.UI.Popups.ReportReasonPopup();

            var window = new Window
            {
                Owner = main,
                Content = popup,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Width = 520,
                Height = 360,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                Background = Brushes.Transparent,
                AllowsTransparency = true
            };

            popup.RequestClose += () =>
            {
                try
                {
                    window.DialogResult = popup.IsConfirmed;
                }
                catch
                {
                    MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
                }

                window.Close();
            };

            bool? dialog = window.ShowDialog();

            if (dialog != true || !popup.IsConfirmed)
            {
                return null;
            }

            return popup.SelectedReasonKey;
        }

        public class PlayerViewModel
        {
            public string Username { get; set; }
            public string DisplayName { get; set; }
            public string StatusText { get; set; }
            public ImageSource AvatarPath { get; set; }
            public Brush ColorBrush { get; set; }
            public Visibility IsTurnVisible { get; set; }
            public Visibility ReportVisibility { get; set; }
        }
    }
}
