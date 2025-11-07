using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.ServiceModel;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;

namespace DamasChinas_Server.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
                     ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class LobbyService : ILobbyService
    {
        private readonly ConcurrentDictionary<string, Lobby> _lobbies = new ConcurrentDictionary<string, Lobby>();
        private readonly ConcurrentDictionary<int, ILobbyCallback> _connections = new ConcurrentDictionary<int, ILobbyCallback>();

        private ILobbyCallback CurrentCallback =>
            OperationContext.Current.GetCallbackChannel<ILobbyCallback>();

        private static string NewCode()
            => Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

        // =========================
        // CREAR LOBBY
        // =========================
        public Lobby CreateLobby(int hostUserId, string hostUsername, bool isPrivate)
        {
            var code = NewCode();
            var lobby = new Lobby
            {
                Code = code,
                HostUserId = hostUserId,
                IsPrivate = isPrivate
            };

            lobby.Members.Add(new LobbyMember
            {
                UserId = hostUserId,
                Username = hostUsername,
                IsHost = true
            });

            _lobbies[code] = lobby;
            _connections[hostUserId] = CurrentCallback;

            return lobby;
        }

        // =========================
        // UNIRSE A UN LOBBY
        // =========================
        public Lobby JoinLobby(string code, int userId, string username)
        {
            if (!_lobbies.TryGetValue(code, out var lobby))
                throw new FaultException("Lobby not found.");

            // Validar que el host siga activo
            if (!_connections.ContainsKey(lobby.HostUserId))
            {
                _lobbies.TryRemove(code, out _);
                throw new FaultException("Lobby is no longer active.");
            }

            // Evitar unión de baneados
            if (lobby.BannedUsers.Contains(userId))
                throw new FaultException("You are banned from this lobby.");

            // Registrar miembro si no existe
            if (!lobby.Members.Any(m => m.UserId == userId))
            {
                var member = new LobbyMember
                {
                    UserId = userId,
                    Username = username,
                    IsHost = false
                };
                lobby.Members.Add(member);
            }

            _connections[userId] = CurrentCallback;

            // Notificar a los miembros activos (de forma asíncrona)
            foreach (var m in lobby.Members.ToList())
            {
                if (_connections.TryGetValue(m.UserId, out var cb))
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            cb.OnMemberJoined(new LobbyMember
                            {
                                UserId = userId,
                                Username = username,
                                IsHost = (lobby.HostUserId == userId)
                            });
                        }
                        catch
                        {
                            _connections.TryRemove(m.UserId, out _);
                        }
                    });
                }
            }

            return lobby;
        }

        // =========================
        // OBTENER LOBBIES PÚBLICOS ACTIVOS
        // =========================
        public List<Lobby> GetPublicLobbies()
        {
            // Eliminar lobbies donde el host ya no tiene conexión activa
            var inactiveCodes = _lobbies.Values
                .Where(l => !_connections.ContainsKey(l.HostUserId))
                .Select(l => l.Code)
                .ToList();

            foreach (var code in inactiveCodes)
                _lobbies.TryRemove(code, out _);

            // Devolver solo las partidas activas y públicas
            var activeLobbies = _lobbies.Values
                .Where(l => !l.IsPrivate && _connections.ContainsKey(l.HostUserId))
                .ToList();

            return activeLobbies;
        }

        // =========================
        // SALIR DE UN LOBBY
        // =========================
        public bool LeaveLobby(string code, int userId)
        {
            if (!_lobbies.TryGetValue(code, out var lobby)) return false;

            var member = lobby.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null) return false;

            lobby.Members.Remove(member);
            _connections.TryRemove(userId, out _);

            foreach (var m in lobby.Members)
                if (_connections.TryGetValue(m.UserId, out var cb))
                    cb.OnMemberLeft(userId);

            // Si se va el host o queda vacío, cerrar lobby
            if (userId == lobby.HostUserId || lobby.Members.Count == 0)
            {
                foreach (var m in lobby.Members)
                    if (_connections.TryGetValue(m.UserId, out var cb))
                        cb.OnLobbyClosed("Host left or lobby empty.");

                _lobbies.TryRemove(code, out _);
            }

            return true;
        }

        // =========================
        // ENVIAR MENSAJE AL LOBBY
        // =========================
        public void SendLobbyMessage(string code, int userId, string username, string message)
        {
            if (!_lobbies.TryGetValue(code, out var lobby)) return;

            var utc = DateTime.UtcNow.ToString("o");
            foreach (var m in lobby.Members)
                if (_connections.TryGetValue(m.UserId, out var cb))
                    cb.OnMessageReceived(userId, username, message, utc);
        }

        // =========================
        // OBTENER LOBBY POR CÓDIGO
        // =========================
        public Lobby GetLobby(string code)
            => _lobbies.TryGetValue(code, out var lobby) ? lobby : null;

        // =========================
        // EXPULSAR JUGADOR
        // =========================
        public bool KickMember(string code, int targetUserId)
        {
            if (!_lobbies.TryGetValue(code, out var lobby)) return false;
            var target = lobby.Members.FirstOrDefault(m => m.UserId == targetUserId);
            if (target == null) return false;

            lobby.Members.Remove(target);
            _connections.TryRemove(targetUserId, out _);

            foreach (var m in lobby.Members)
                if (_connections.TryGetValue(m.UserId, out var cb))
                    cb.OnMemberLeft(targetUserId);

            return true;
        }

        // =========================
        // BANEAR JUGADOR
        // =========================
        public bool BanMember(string code, int targetUserId)
        {
            if (!_lobbies.TryGetValue(code, out var lobby))
                return false;

            if (lobby.BannedUsers.Contains(targetUserId))
                return false;

            var target = lobby.Members.FirstOrDefault(m => m.UserId == targetUserId);
            if (target == null)
                return false;

            lobby.Members.Remove(target);
            lobby.BannedUsers.Add(targetUserId);
            _connections.TryRemove(targetUserId, out _);

            foreach (var m in lobby.Members)
                if (_connections.TryGetValue(m.UserId, out var cb))
                    cb.OnMemberLeft(targetUserId);

            return true;
        }

        // =========================
        // INICIAR PARTIDA
        // =========================
        public bool StartGame(string code)
        {
            if (!_lobbies.TryGetValue(code, out var lobby)) return false;

            foreach (var m in lobby.Members)
                if (_connections.TryGetValue(m.UserId, out var cb))
                    cb.OnGameStarted(code);

            // Si quieres mantener el lobby hasta que acabe la partida, comenta esta línea
            //_lobbies.TryRemove(code, out _);

            return true;
        }
    }
}
