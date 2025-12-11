using DamasChinas_Client.UI.LobbyServiceProxy;
using DamasChinas_Client.UI.MatchServiceProxy;
using DamasChinas_Client.UI.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using AccountProxy = DamasChinas_Client.UI.AccountManagerServiceProxy;

namespace DamasChinas_Client.UI.Pages
{
    // =========================================================
    //  CALLBACK HANDLER (POR SESIÓN)
    // =========================================================
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public class MatchCallbackHandler : IMatchServiceCallback
    {
        private readonly MatchRoom _page;

        public MatchCallbackHandler(MatchRoom page)
        {
            _page = page ?? throw new ArgumentNullException(nameof(page));
        }

        public void OnPlayerMoved(TurnChangeDto turnInfo)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _page.HandlePlayerMoved(turnInfo);
            });
        }

        public void OnMatchEnded(string winnerUsername)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _page.HandleMatchEnded(winnerUsername);
            });
        }

        public void OnPlayerLeftMatch(string username)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _page.HandlePlayerLeft(username);
            });
        }

        public void OnErrorOccurred(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _page.HandleError(message);
            });
        }
    }

    // =========================================================
    //  MATCH ROOM PAGE
    // =========================================================
    public partial class MatchRoom : Page
    {
        // =========================================================
        //  CONSTANTES TABLERO (SINCRONIZADAS CON SERVIDOR)
        // =========================================================

        private const int BoardRadius = 4;
        private const int MaxCubeRadius = BoardRadius * 2;

        private const double HexSize = 32.0;
        private const double CenterX = 600.0;
        private const double CenterY = 600.0;

        // =========================================================
        //  ESTADO GENERAL
        // =========================================================

        private readonly int _lobbyCode;
        private readonly string _myUsername;

        private MatchServiceClient _proxy;
        private MatchCallbackHandler _callbackHandler;
        private LobbySnapshotDto _lobbySnapshot;

        private Dictionary<Point, Ellipse> _holesVisuals;
        private Dictionary<Point, Ellipse> _marblesVisuals;
        private Point? _selectedCoord;
        private string _currentPlayerTurn;

        private bool _matchEnded;

        // Jugadores
        public ObservableCollection<PlayerViewModel> Players { get; set; }
        private readonly Dictionary<string, Brush> _userColors;
        private readonly List<Brush> _availableColors;

        // =========================================================
        //  CONSTRUCTOR
        // =========================================================

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

        // =========================================================
        //  CICLO DE VIDA PÁGINA
        // =========================================================

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _matchEnded = false;

                // 1. Snapshot de lobby
                _lobbySnapshot = LobbySession.Manager.GetCurrentLobby(_myUsername);
                SetupPlayersMetadata();

                // 2. Tablero
                DrawBoardBackground();
                DrawPlayerLabels();

                // 3. Chat del lobby
                LobbySession.Manager.ChatMessageReceived += OnChatMessageReceived;

                // 4. Conexión a servicio de partida
                InitializeMatchConnection();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MatchRoom.OnPageLoaded] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
                NavigateToMenu();
            }
        }

        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            LobbySession.Manager.ChatMessageReceived -= OnChatMessageReceived;
            _matchEnded = true;

            try
            {
                if (_proxy != null && _proxy.State == CommunicationState.Opened)
                {
                    _proxy.Close();
                }
            }
            catch
            {
                _proxy?.Abort();
            }
        }

        // =========================================================
        //  CONFIGURACIÓN DE JUGADORES
        // =========================================================

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
                var member = sortedMembers[i];
                Brush color = _availableColors[i % _availableColors.Count];

                _userColors[member.Username] = color;

                string displayName = member.IsHost
                    ? $"★ {member.Username}"
                    : member.Username;

                Players.Add(new PlayerViewModel
                {
                    Username = member.Username,
                    DisplayName = displayName,
                    ColorBrush = color,
                    AvatarPath = PathProvider.LoadAvatar(member.AvatarFile),
                    IsTurnVisible = Visibility.Collapsed,
                    StatusText = MessageTranslator.GetLocalizedMessage(MessageKeys.StatusWaiting)
                });
            }
        }

        // =========================================================
        //  CONEXIÓN A MATCH SERVICE
        // =========================================================

        private void InitializeMatchConnection()
        {
            _callbackHandler = new MatchCallbackHandler(this);
            var context = new InstanceContext(_callbackHandler);
            _proxy = new MatchServiceClient(context);

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
            catch (Exception ex) when (
                ex is EndpointNotFoundException ||
                ex is CommunicationException ||
                ex is TimeoutException)
            {
                System.Diagnostics.Debug.WriteLine($"[MatchRoom.InitializeMatchConnection] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MatchRoom.InitializeMatchConnection] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

        // =========================================================
        //  GEOMETRÍA DEL TABLERO
        // =========================================================

        private static readonly (int X, int Y, int Z)[] ZoneDirections =
        {
            (1, -1, 0),
            (-1, 1, 0),
            (0, -1, 1),
            (0, 1, -1),
            (1, 0, -1),
            (-1, 0, 1)
        };

        private List<(int X, int Y, int Z)> GenerateBoardCubeCoordinates()
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

        // =========================================================
        //  DIBUJO DEL TABLERO
        // =========================================================

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

        private void DrawPlayerLabels()
        {
            // Panel lateral ya indica claramente jugadores y colores
        }

        private Point HexToPixel(int q, int r)
        {
            double x = HexSize * (Math.Sqrt(3) * q + (Math.Sqrt(3) / 2.0) * r);
            double y = HexSize * (1.5 * r);
            return new Point(CenterX + x, CenterY + y);
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

            if (!_holesVisuals.TryGetValue(point, out var hole))
            {
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

        // =========================================================
        //  ACTUALIZACIÓN DE ESTADO
        // =========================================================

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
                Brush color = marble.Fill;

                BoardCanvas.Children.Remove(marble);
                _marblesVisuals.Remove(from);

                PlaceMarble((int)to.X, (int)to.Y, color);
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

        public void HandleError(string msgKey)
        {
            MessageHelper.ShowPopup(msgKey, PopupType.Warning);
        }



        // =========================================================
        //  INTERACCIÓN CON EL TABLERO
        // =========================================================

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
                SendMove(_selectedCoord.Value, clickedCoord);
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

        // =========================================================
        //  CREACIÓN DEL REQUEST DE MOVIMIENTO
        // =========================================================
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

        private void SendMove(Point origin, Point dest)
        {
            if (_proxy == null || _proxy.State != CommunicationState.Opened)
            {
                return;
            }

            var req = CreateMoveRequest(origin, dest);

            try
            {
                var result = _proxy.MovePiece(req);

                if (!result.Success)
                {
                    MessageHelper.ShowFromResult(result);
                }

                DeselectPiece();
            }
            catch (Exception ex) when (
                ex is EndpointNotFoundException ||
                ex is CommunicationException ||
                ex is TimeoutException)
            {
                System.Diagnostics.Debug.WriteLine($"[MatchRoom.SendMove] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.ServerUnavailable, PopupType.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MatchRoom.SendMove] {ex.Message}");
                MessageHelper.ShowPopup(MessageKeys.UnknownError, PopupType.Error);
            }
        }

        // =========================================================
        //  CHAT
        // =========================================================

        private void OnSendMessageClick(object sender, RoutedEventArgs e)
        {
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

            Dispatcher.Invoke(() =>
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
            });
        }

        // =========================================================
        //  NAVEGACIÓN / BOTONES
        // =========================================================

        private void OnLeaveMatchClick(object sender, RoutedEventArgs e)
        {
            if (MessageHelper.ShowConfirm("confirmExitLobby"))
            {
                try
                {
                    _proxy?.LeaveMatch(_lobbyCode, _myUsername);
                }
                catch
                {
                    // Si falla el leave, de todos modos nos regresamos al menú
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


        // =========================================================
        //  VIEWMODEL JUGADOR
        // =========================================================

        public class PlayerViewModel
        {
            public string Username { get; set; }
            public string DisplayName { get; set; }
            public string StatusText { get; set; }
            public ImageSource AvatarPath { get; set; }
            public Brush ColorBrush { get; set; }
            public Visibility IsTurnVisible { get; set; }
        }
    }
}
