using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.ServiceModel;
using Damas_Chinas_Server.Dtos;
using Damas_Chinas_Server.Interfaces;

namespace Damas_Chinas_Server.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
                     ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class LobbyService : ILobbyService
    {

        private readonly ConcurrentDictionary<string, Lobby> _lobbies = new ConcurrentDictionary<string, Lobby>();
        private readonly ConcurrentDictionary<int, ILobbyCallback> _connections = new ConcurrentDictionary<int, ILobbyCallback>();

        // userId -> callback (por sesión activa)

        private ILobbyCallback CurrentCallback =>
            OperationContext.Current.GetCallbackChannel<ILobbyCallback>();

        private static string NewCode()
            => Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

        public Lobby CreateLobby(int hostUserId, string hostUsername, bool isPrivate)
        {
            var code = NewCode();
            var lobby = new Lobby
            {
                Code = code,
                HostUserId = hostUserId,
                IsPrivate = isPrivate
            };
            lobby.Members.Add(new LobbyMember { UserId = hostUserId, Username = hostUsername, IsHost = true });

            _lobbies[code] = lobby;
            _connections[hostUserId] = CurrentCallback;

            return lobby;
        }

        public Lobby JoinLobby(string code, int userId, string username)
        {
            if (!_lobbies.TryGetValue(code, out var lobby))
                throw new FaultException("Lobby not found.");

            if (!lobby.Members.Any(m => m.UserId == userId))
            {
                var member = new LobbyMember { UserId = userId, Username = username, IsHost = false };
                lobby.Members.Add(member);
            }

            _connections[userId] = CurrentCallback;

            // Notificar a todos
            foreach (var m in lobby.Members)
                if (_connections.TryGetValue(m.UserId, out var cb))
                    cb.OnMemberJoined(new LobbyMember { UserId = userId, Username = username, IsHost = (lobby.HostUserId == userId) });

            return lobby;
        }

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

        public void SendLobbyMessage(string code, int userId, string username, string message)
        {
            if (!_lobbies.TryGetValue(code, out var lobby)) return;

            var utc = DateTime.UtcNow.ToString("o");
            foreach (var m in lobby.Members)
                if (_connections.TryGetValue(m.UserId, out var cb))
                    cb.OnMessageReceived(userId, username, message, utc);
        }

        public Lobby GetLobby(string code)
            => _lobbies.TryGetValue(code, out var lobby) ? lobby : null;

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

        public bool StartGame(string code)
        {
            if (!_lobbies.TryGetValue(code, out var lobby)) return false;

            foreach (var m in lobby.Members)
                if (_connections.TryGetValue(m.UserId, out var cb))
                    cb.OnGameStarted(code);

            // opcional: mantener lobby hasta terminar partida, o cerrarlo:
            //_lobbies.TryRemove(code, out _);
            return true;
        }
    }
}
