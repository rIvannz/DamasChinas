using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Game;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Services;
using DamasChinas_Server.logic;

namespace DamasChinas_Server.Logic
{
    public sealed class MatchManager
    {
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

        // ================================================
        // CREAR MATCH
        // ================================================
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
            string host = players.First();

            _matches[lobbyCode] = new ActiveMatch(game, playerColorMap, host);

            _log.Info($"Match created for Lobby {lobbyCode}. Host={host}");
        }

        private void BroadcastBoardState(int lobbyCode, ActiveMatch match)
        {
            var state = GetMatchState(lobbyCode);

            foreach (var cb in match.Callbacks.Values)
            {
                try
                {
                    cb.OnPlayerMoved(new TurnChangeDto { BoardState = state });
                }
                catch
                {
                    // Ignoramos fallas individuales de callback
                }
            }
        }

        // ================================================
        // REGISTRO DE SESIÓN
        // ================================================
        public void RegisterPlayerSession(int lobbyCode, string username, IMatchCallback callback)
        {
            if (!_matches.TryGetValue(lobbyCode, out var match))
            {
                throw new RepositoryValidationException(MessageCode.LobbyNotFound);
            }

            if (!match.UserColorMap.ContainsKey(username))
            {
                throw new RepositoryValidationException(MessageCode.UserNotFound);
            }

            match.Callbacks[username] = callback;
        }

        // ================================================
        // APLICAR MOVIMIENTO
        // ================================================
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
                throw new Exception(result.ErrorMessage);
            }

            BroadcastMove(req.LobbyCode, req.Username, match, origin, dest);

            // ================================================
            // FIN DE PARTIDA
            // ================================================
            if (result.Winner.HasValue)
            {
                string winner = match.UserColorMap
                    .First(x => x.Value == result.Winner.Value).Key;

                BroadcastGameOver(winner, match);
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
            catch (Exception ex)
            {
                _log.Error("Error saving match result: " + ex.Message, ex);
            }
        }

        // ================================================
        // SALIDA DE JUGADOR / DESCONEXIÓN
        // ================================================
        public void HandlePlayerDisconnect(int lobbyCode, string username)
        {
            // Wrapper para centralizar la lógica de salida
            RemovePlayer(lobbyCode, username);
        }

        public void RemovePlayer(int lobbyCode, string username)
        {
            if (!_matches.TryGetValue(lobbyCode, out var match))
                return;

            if (!match.UserColorMap.TryGetValue(username, out var color))
                return;

            bool wasHost = string.Equals(match.HostUsername, username, StringComparison.OrdinalIgnoreCase);

            // === Caso especial: solo dos jugadores ===
            if (match.UserColorMap.Count == 2)
            {
                string winner = match.UserColorMap.Keys
                    .First(u => !u.Equals(username, StringComparison.OrdinalIgnoreCase));

                BroadcastGameOver(winner, match);
                SaveMatchResult(match, winner);

                _matches.TryRemove(lobbyCode, out _);
                return;
            }

            // === Caso general: jugador sale (incluye host) ===
           // match.Game.RemovePlayer(color);
            match.UserColorMap.Remove(username);
            match.Callbacks.TryRemove(username, out _);

            foreach (var cb in match.Callbacks.Values)
            {
                try { cb.OnPlayerLeftMatch(username); }
                catch { }
            }

            // Si el host salió, elegir un nuevo host para consistencia interna
            if (wasHost && match.UserColorMap.Count > 0)
            {
                match.HostUsername = match.UserColorMap.Keys.First();
                _log.Info($"[MatchManager] Host changed to {match.HostUsername}");
            }

            // Enviar estado nuevo del tablero
            BroadcastBoardState(lobbyCode, match);
        }



        // ================================================
        // ESTADO TABLERO
        // ================================================
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

        // ================================================
        // BROADCASTS
        // ================================================
        private void BroadcastMove(int code, string player, ActiveMatch match,
            HexCoordinate from, HexCoordinate to)
        {
            var next = match.UserColorMap
                .First(x => x.Value == match.Game.CurrentTurn).Key;

            var dto = new TurnChangeDto
            {
                PreviousPlayer = player,
                NextPlayer = next,
                MoveOrigin = new HexCoordinateDto { X = from.X, Y = from.Y, Z = from.Z },
                MoveDestination = new HexCoordinateDto { X = to.X, Y = to.Y, Z = to.Z }
            };

            foreach (var cb in match.Callbacks.Values)
            {
                try
                {
                    cb.OnPlayerMoved(dto);
                }
                catch
                {
                    // Ignoramos errores individuales
                }
            }
        }

        private void BroadcastGameOver(string winner, ActiveMatch match)
        {
            foreach (var cb in match.Callbacks.Values)
            {
                try
                {
                    cb.OnMatchEnded(winner);
                }
                catch
                {
                    // Ignoramos errores individuales
                }
            }
        }

        // ================================================
        // CLASE INTERNA
        // ================================================
        private class ActiveMatch
        {
            public ChineseCheckersGame Game { get; }
            public Dictionary<string, PlayerColor> UserColorMap { get; }
            public ConcurrentDictionary<string, IMatchCallback> Callbacks { get; }
            public string HostUsername { get; set; }

            public ActiveMatch(ChineseCheckersGame game, Dictionary<string, PlayerColor> map, string host)
            {
                Game = game;
                UserColorMap = map;
                HostUsername = host;
                Callbacks = new ConcurrentDictionary<string, IMatchCallback>();
            }
        }
    }
}
