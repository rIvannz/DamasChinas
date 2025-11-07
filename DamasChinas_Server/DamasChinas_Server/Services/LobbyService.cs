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

      
        public Lobby JoinLobby(string code, int userId, string username)
        {
            if (!_lobbies.TryGetValue(code, out var lobby))
                throw new FaultException("Lobby not found.");

            
            if (!_connections.ContainsKey(lobby.HostUserId))
            {
                _lobbies.TryRemove(code, out _);
                throw new FaultException("Lobby is no longer active.");
            }

           
            if (lobby.BannedUsers.Contains(userId))
                throw new FaultException("You are banned from this lobby.");

            
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

     
        public List<Lobby> GetPublicLobbies()
        {
           
            var inactiveCodes = _lobbies.Values
                .Where(l => !_connections.ContainsKey(l.HostUserId))
                .Select(l => l.Code)
                .ToList();

            foreach (var code in inactiveCodes)
                _lobbies.TryRemove(code, out _);

          
            var activeLobbies = _lobbies.Values
                .Where(l => !l.IsPrivate && _connections.ContainsKey(l.HostUserId))
                .ToList();

            return activeLobbies;
        }


        public bool LeaveLobby(string code, int userId)
        {
            if (!_lobbies.TryGetValue(code, out var lobby))
                return false;

            var member = lobby.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null)
                return false;

            // Eliminar de la lista de miembros
            lobby.Members.Remove(member);
            _connections.TryRemove(userId, out _);

            // Notificar a los miembros que alguien salió
            foreach (var m in lobby.Members.ToList())
            {
                if (_connections.TryGetValue(m.UserId, out var cb))
                {
                    try
                    {
                        cb.OnMemberLeft(userId);
                    }
                    catch
                    {
                        _connections.TryRemove(m.UserId, out _);
                    }
                }
            }

            // Si se va el host o el lobby queda vacío
            if (userId == lobby.HostUserId || lobby.Members.Count == 0)
            {
                foreach (var m in lobby.Members.ToList())
                {
                    if (_connections.TryGetValue(m.UserId, out var cb))
                    {
                        try
                        {
                            cb.OnLobbyClosed("The host has left the lobby or it became empty.");
                        }
                        catch
                        {
                            _connections.TryRemove(m.UserId, out _);
                        }
                    }
                }

                _lobbies.TryRemove(code, out _);
            }

            // Limpieza extra (por si el host cierra la aplicación sin avisar)
            var inactiveLobbies = _lobbies.Values
                .Where(l => !_connections.ContainsKey(l.HostUserId))
                .Select(l => l.Code)
                .ToList();

            foreach (var inactiveCode in inactiveLobbies)
                _lobbies.TryRemove(inactiveCode, out _);

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
