using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Game;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.logic;
using DamasChinas_Server.Services;
using DamasChinas_Shared.Contracts.Dtos;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;

namespace DamasChinas_Server.Logic
{
    public sealed class MatchManager
    {
        private const int DisconnectGraceSeconds = 20;

        private static readonly Lazy<MatchManager> _instance =
            new Lazy<MatchManager>(() => new MatchManager());

        public static MatchManager Instance => _instance.Value;

        private readonly ConcurrentDictionary<int, ActiveMatch> _matches =
            new ConcurrentDictionary<int, ActiveMatch>();

        private readonly ILogService _log;
        private readonly IRepositoryMatches _repoMatches;

        private MatchManager()
        {
            _log = LogFactory.Create(typeof(MatchManager));
            _repoMatches = new RepositoryMatches();
        }

        public void CreateMatchFromLobby(int lobbyCode, List<string> players)
        {
            if (_matches.ContainsKey(lobbyCode))
            {
                return;
            }

            var colorList = Enum.GetValues(typeof(PlayerColor)).Cast<PlayerColor>().ToList();
            var playerColorMap = new Dictionary<string, PlayerColor>();
            var gamePlayers = new List<Player>();

            for (int i = 0; i < players.Count; i++)
            {
                var color = colorList[i];
                playerColorMap[players[i]] = color;
                gamePlayers.Add(new Player(players[i], players[i], color));
            }

            var game = new ChineseCheckersGame(gamePlayers);
            string host = players[0];

            _matches[lobbyCode] = new ActiveMatch(game, playerColorMap, host);

            _log.Info($"Match created for Lobby {lobbyCode}. Host={host}");
        }

        public void RegisterPlayerSession(int lobbyCode, string username, IMatchCallback callback)
        {
            if (!_matches.TryGetValue(lobbyCode, out var match))
                throw new RepositoryValidationException(MessageCode.LobbyNotFound);

            if (!match.UserColorMap.ContainsKey(username))
                throw new RepositoryValidationException(MessageCode.UserNotFound);

           
            bool hadCallback = match.Callbacks.ContainsKey(username);

     
            bool wasDisconnected = match.DisconnectedUsers.TryRemove(username, out _);

            match.Callbacks[username] = callback;

           
            bool hadPendingRemoval = match.PendingRemovals.ContainsKey(username);
            CancelPendingRemoval(match, username);

            bool isReconnect = wasDisconnected || hadCallback || hadPendingRemoval;

            if (isReconnect)
            {
                _log.Info($"[MatchManager] {MessageCode.PlayerReconnected} user={username} lobby={lobbyCode}");
                BroadcastPlayerReconnectedSafe(username, match);
            }
            
        }


        public void ApplyMove(MoveRequestDto req)
        {
            if (!_matches.TryGetValue(req.LobbyCode, out var match))
            {
                throw new RepositoryValidationException(MessageCode.LobbyNotFound);
            }

            var color = match.UserColorMap[req.Username];

            var origin = new HexCoordinate(req.Origin.X, req.Origin.Y, req.Origin.Z);
            var dest = new HexCoordinate(req.Destination.X, req.Destination.Y, req.Destination.Z);

            var move = new PlayerMove(color, new List<HexCoordinate> { origin, dest });

            var result = match.Game.TryApplyMove(move);

            if (!result.Succeeded)
            {
                throw new RepositoryValidationException(MessageCode.InvalidMove);
            }

            BroadcastMoveSafe(req.LobbyCode, req.Username, match, origin, dest);

            if (result.Winner.HasValue)
            {
                string winner = match.UserColorMap
                    .First(x => x.Value == result.Winner.Value).Key;

                BroadcastGameOverSafe(winner, match);
                SaveMatchResult(match, winner);

                _matches.TryRemove(req.LobbyCode, out _);
            }
        }

        private void SaveMatchResult(ActiveMatch match, string winner)
        {
            try
            {
                _repoMatches.SaveMatchResult(match.UserColorMap, winner);
                _log.Info($"Match result saved. Winner={winner}");
            }
            catch (InvalidOperationException ex)
            {
                _log.Error("Error saving match result: " + ex.Message, ex);
            }
            catch (CommunicationException ex)
            {
                _log.Error("Error saving match result: " + ex.Message, ex);
            }
        }

        public void HandlePlayerDisconnect(int lobbyCode, string username)
        {
            if (!_matches.TryGetValue(lobbyCode, out var match))
            {
                return;
            }

            if (!match.UserColorMap.ContainsKey(username))
            {
                return;
            }

            match.DisconnectedUsers[username] = true;

            _log.Warn($"[MatchManager] {MessageCode.PlayerTemporarilyDisconnected} user={username} lobby={lobbyCode}");
            BroadcastPlayerTemporarilyDisconnectedSafe(username, match);

            if (match.PendingRemovals.ContainsKey(username))
            {
                return;
            }

            var cts = new CancellationTokenSource();
            if (!match.PendingRemovals.TryAdd(username, cts))
            {
                return;
            }

            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    await System.Threading.Tasks.Task
                        .Delay(TimeSpan.FromSeconds(DisconnectGraceSeconds), cts.Token)
                        .ConfigureAwait(false);

                    RemovePlayer(lobbyCode, username);

                    _log.Info($"[MatchManager] {MessageCode.PlayerRemovedAfterDisconnect} user={username} lobby={lobbyCode}");
                    BroadcastPlayerRemovedAfterTimeoutSafe(username, match);
                }
                catch (OperationCanceledException)
                {
                    _log.Info($"[MatchManager] disconnect timer canceled (reconnected) user={username} lobby={lobbyCode}");
                }
                catch (ObjectDisposedException)
                {
                    _log.Info($"[MatchManager] disconnect timer disposed user={username} lobby={lobbyCode}");
                }
                catch (ArgumentOutOfRangeException)
                {
                    _log.Error($"[MatchManager] {MessageCode.UnknownError} invalid delay for disconnect timer user={username} lobby={lobbyCode}");
                }
                finally
                {
                    if (match.PendingRemovals.TryRemove(username, out var removedCts))
                    {
                        try { removedCts.Dispose(); } catch { }
                    }

                    match.DisconnectedUsers.TryRemove(username, out _);
                }
            });
        }

        public void RemovePlayer(int lobbyCode, string username)
        {
            if (!_matches.TryGetValue(lobbyCode, out var match))
            {
                return;
            }

            CleanupDisconnectTracking(match, username);

            if (!match.UserColorMap.TryGetValue(username, out var color))
            {
                return;
            }

            if (match.UserColorMap.Count == 2)
            {
                RemovePlayerFromTwoPlayerMatch(lobbyCode, username, match);
                return;
            }

            RemovePlayerFromMultiPlayerMatch(lobbyCode, username, match, color);
        }

        private static void CleanupDisconnectTracking(ActiveMatch match, string username)
        {
            CancelPendingRemoval(match, username);
            match.DisconnectedUsers.TryRemove(username, out _);
        }

        private static void CancelPendingRemoval(ActiveMatch match, string username)
        {
            if (match.PendingRemovals.TryRemove(username, out var cts))
            {
                try { cts.Cancel(); } catch { }
                try { cts.Dispose(); } catch { }
            }
        }

        private void RemovePlayerFromTwoPlayerMatch(int lobbyCode, string username, ActiveMatch match)
        {
            match.Callbacks.TryRemove(username, out _);
            match.UserColorMap.Remove(username);

            string winner = match.UserColorMap.Keys.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(winner))
            {
                _matches.TryRemove(lobbyCode, out _);
                return;
            }

            BroadcastGameOverSafe(winner, match);
            SaveMatchResult(match, winner);

            _matches.TryRemove(lobbyCode, out _);
        }

        private void RemovePlayerFromMultiPlayerMatch(int lobbyCode, string username, ActiveMatch match, PlayerColor color)
        {
            bool wasHost = string.Equals(match.HostUsername, username, StringComparison.OrdinalIgnoreCase);

            match.Game.RemovePlayer(color);

            match.UserColorMap.Remove(username);
            match.Callbacks.TryRemove(username, out _);

            BroadcastPlayerLeftSafe(username, match);

            UpdateHostIfNeeded(match, wasHost);

            BroadcastBoardStateSafe(lobbyCode, match);
        }

        private void UpdateHostIfNeeded(ActiveMatch match, bool wasHost)
        {
            if (!wasHost)
            {
                return;
            }

            if (match.UserColorMap.Count <= 0)
            {
                return;
            }

            match.HostUsername = match.UserColorMap.Keys.First();
            _log.Info($"[MatchManager] Host changed to {match.HostUsername}");
        }

        private void BroadcastPlayerTemporarilyDisconnectedSafe(string username, ActiveMatch match)
        {
            foreach (var entry in match.Callbacks.ToArray())
            {
                try
                {
                    entry.Value.OnErrorOccurred($"DISCONNECTED::{username}");
                }
                catch
                {
                    match.Callbacks.TryRemove(entry.Key, out _);
                    _log.Info($"[MatchManager] Removed dead callback on DISCONNECTED user={entry.Key}");
                }
            }
        }

        private void BroadcastPlayerReconnectedSafe(string username, ActiveMatch match)
        {
            foreach (var entry in match.Callbacks.ToArray())
            {
                try
                {
                    entry.Value.OnErrorOccurred($"RECONNECTED::{username}");
                }
                catch
                {
                    match.Callbacks.TryRemove(entry.Key, out _);
                    _log.Info($"[MatchManager] Removed dead callback on RECONNECTED user={entry.Key}");
                }
            }
        }

        private void BroadcastPlayerRemovedAfterTimeoutSafe(string username, ActiveMatch match)
        {
            foreach (var entry in match.Callbacks.ToArray())
            {
                try
                {
                    entry.Value.OnErrorOccurred($"REMOVED::{username}");
                }
                catch
                {
                    match.Callbacks.TryRemove(entry.Key, out _);
                    _log.Info($"[MatchManager] Removed dead callback on REMOVED user={entry.Key}");
                }
            }
        }

        private void BroadcastBoardStateSafe(int lobbyCode, ActiveMatch match)
        {
            var state = GetMatchState(lobbyCode);

            foreach (var entry in match.Callbacks.ToArray())
            {
                try
                {
                    entry.Value.OnPlayerMoved(new TurnChangeDto { BoardState = state });
                }
                catch
                {
                    match.Callbacks.TryRemove(entry.Key, out _);
                    _log.Info($"[MatchManager] Removed dead callback on BroadcastBoardState user={entry.Key} lobby={lobbyCode}");
                }
            }
        }

        private void BroadcastPlayerLeftSafe(string usernameLeft, ActiveMatch match)
        {
            foreach (var entry in match.Callbacks.ToArray())
            {
                try
                {
                    entry.Value.OnPlayerLeftMatch(usernameLeft);
                }
                catch
                {
                    match.Callbacks.TryRemove(entry.Key, out _);
                    _log.Info($"[MatchManager] Removed dead callback on OnPlayerLeftMatch user={entry.Key}");
                }
            }
        }

        private void BroadcastMoveSafe(int lobbyCode, string player, ActiveMatch match, HexCoordinate from, HexCoordinate to)
        {
            string next;
            try
            {
                next = match.UserColorMap.First(x => x.Value == match.Game.CurrentTurn).Key;
            }
            catch
            {
                BroadcastBoardStateSafe(lobbyCode, match);
                return;
            }

            var dto = new TurnChangeDto
            {
                PreviousPlayer = player,
                NextPlayer = next,
                MoveOrigin = new HexCoordinateDto { X = from.X, Y = from.Y, Z = from.Z },
                MoveDestination = new HexCoordinateDto { X = to.X, Y = to.Y, Z = to.Z }
            };

            foreach (var entry in match.Callbacks.ToArray())
            {
                try
                {
                    entry.Value.OnPlayerMoved(dto);
                }
                catch
                {
                    match.Callbacks.TryRemove(entry.Key, out _);
                    _log.Info($"[MatchManager] Removed dead callback on OnPlayerMoved user={entry.Key} lobby={lobbyCode}");
                }
            }
        }

        private void BroadcastGameOverSafe(string winner, ActiveMatch match)
        {
            foreach (var entry in match.Callbacks.ToArray())
            {
                try
                {
                    entry.Value.OnMatchEnded(winner);
                }
                catch
                {
                    match.Callbacks.TryRemove(entry.Key, out _);
                    _log.Info($"[MatchManager] Removed dead callback on OnMatchEnded user={entry.Key}");
                }
            }
        }

        public MatchStateDto GetMatchState(int lobbyCode)
        {
            if (!_matches.TryGetValue(lobbyCode, out var match))
            {
                return null;
            }

            var board = new Dictionary<string, HexCoordinateDto[]>();

            foreach (var entry in match.UserColorMap)
            {
                var coords = match.Game.Board.Cells
                    .Where(c => c.IsOccupied && c.Piece.Color == entry.Value)
                    .Select(c => new HexCoordinateDto
                    {
                        X = c.Coordinate.X,
                        Y = c.Coordinate.Y,
                        Z = c.Coordinate.Z
                    })
                    .ToArray();

                board[entry.Key] = coords;
            }

            return new MatchStateDto
            {
                IsActive = true,
                CurrentTurnPlayer = match.UserColorMap
                    .First(x => x.Value == match.Game.CurrentTurn).Key,
                BoardPieces = board
            };
        }

        public void NotifyBanAndKickIfInMatch(string username, BanInfoDto banInfo)
        {
            if (string.IsNullOrWhiteSpace(username) || banInfo == null || !banInfo.IsBanned)
            {
                return;
            }

            int lobbyCode = FindLobbyCodeByUser(username);
            if (lobbyCode <= 0)
            {
                return;
            }

            if (!_matches.TryGetValue(lobbyCode, out var match))
            {
                return;
            }

            if (match.Callbacks.TryGetValue(username, out var cb))
            {
                try
                {
                    cb.OnBanStatusUpdated(banInfo);
                }
                catch
                {
                    match.Callbacks.TryRemove(username, out _);
                    _log.Info($"[MatchManager] Removed dead callback on OnBanStatusUpdated user={username}");
                }
            }

            RemovePlayer(lobbyCode, username);
        }

        private int FindLobbyCodeByUser(string username)
        {
            foreach (var entry in _matches)
            {
                var match = entry.Value;
                if (match == null)
                {
                    continue;
                }

                if (match.Callbacks.ContainsKey(username) || match.UserColorMap.ContainsKey(username))
                {
                    return entry.Key;
                }
            }

            return -1;
        }

        private sealed class ActiveMatch
        {
            public ChineseCheckersGame Game { get; }
            public Dictionary<string, PlayerColor> UserColorMap { get; }
            public ConcurrentDictionary<string, IMatchCallback> Callbacks { get; }
            public string HostUsername { get; set; }

            public ConcurrentDictionary<string, CancellationTokenSource> PendingRemovals { get; }
            public ConcurrentDictionary<string, bool> DisconnectedUsers { get; }

            public ActiveMatch(ChineseCheckersGame game, Dictionary<string, PlayerColor> map, string host)
            {
                Game = game;
                UserColorMap = map;
                HostUsername = host;
                Callbacks = new ConcurrentDictionary<string, IMatchCallback>();

                PendingRemovals = new ConcurrentDictionary<string, CancellationTokenSource>();
                DisconnectedUsers = new ConcurrentDictionary<string, bool>();
            }
        }
    }
}
