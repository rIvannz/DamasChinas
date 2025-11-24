using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;

namespace DamasChinas_Server.Services
{
    [ServiceBehavior(
    InstanceContextMode = InstanceContextMode.Single,
    ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class LobbyService : ILobbyService
    {
        
        protected LobbyService()
        {
        }

        private readonly ConcurrentDictionary<string, Lobby> _lobbies =
            new ConcurrentDictionary<string, Lobby>();

        private readonly ConcurrentDictionary<int, ILobbyCallback> _connections =
            new ConcurrentDictionary<int, ILobbyCallback>();

      
        private ILobbyCallback CurrentCallback =>
            OperationContext.Current.GetCallbackChannel<ILobbyCallback>();


        private static string NewCode() =>
            Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();



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
                throw new FaultException(MessageCode.LobbyNotFound.ToString());

            if (!_connections.ContainsKey(lobby.HostUserId))
            {
                _lobbies.TryRemove(code, out _);
                throw new FaultException(MessageCode.LobbyInactive.ToString());
            }

            if (lobby.BannedUsers.Contains(userId))
                throw new FaultException(MessageCode.LobbyUserBanned.ToString());

            if (!lobby.Members.Any(m => m.UserId == userId))
            {
                lobby.Members.Add(new LobbyMember
                {
                    UserId = userId,
                    Username = username,
                    IsHost = false
                });
            }

            _connections[userId] = CurrentCallback;

         
            var recipients = lobby.Members
                .Where(m => _connections.ContainsKey(m.UserId))
                .Select(m => m.UserId)
                .ToList();

            foreach (var memberId in recipients)
            {
                if (_connections.TryGetValue(memberId, out var cb))
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
                            _connections.TryRemove(memberId, out _);
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

            return _lobbies.Values
                .Where(l => !l.IsPrivate && _connections.ContainsKey(l.HostUserId))
                .ToList();
        }



        public bool LeaveLobby(string code, int userId)
        {
            if (!_lobbies.TryGetValue(code, out var lobby))
                return false;

            if (!TryHandleMemberLeaving(lobby, userId))
                return false;

            NotifyMembersLeft(lobby, userId);

            if (userId == lobby.HostUserId || lobby.Members.Count == 0)
                CloseLobby(lobby);

            CleanupInactiveLobbies();

            return true;
        }

        private bool TryHandleMemberLeaving(Lobby lobby, int userId)
        {
            var member = lobby.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null)
                return false;

            lobby.Members.Remove(member);
            _connections.TryRemove(userId, out _);

            return true;
        }

        private void NotifyMembersLeft(Lobby lobby, int userId)
        {
            var recipients = lobby.Members
                .Where(m => _connections.ContainsKey(m.UserId))
                .Select(m => m.UserId)
                .ToList();

            foreach (var id in recipients)
            {
                if (_connections.TryGetValue(id, out var cb))
                {
                    try
                    {
                        cb.OnMemberLeft(userId);
                    }
                    catch
                    {
                        _connections.TryRemove(id, out _);
                    }
                }
            }
        }

        private void CloseLobby(Lobby lobby)
        {
            var recipients = lobby.Members
                .Where(m => _connections.ContainsKey(m.UserId))
                .Select(m => m.UserId)
                .ToList();

            foreach (var id in recipients)
            {
                if (_connections.TryGetValue(id, out var cb))
                {
                    try
                    {
                        cb.OnLobbyClosed(MessageCode.LobbyClosed.ToString());
                    }
                    catch
                    {
                        _connections.TryRemove(id, out _);
                    }
                }
            }

            _lobbies.TryRemove(lobby.Code, out _);
        }

        private void CleanupInactiveLobbies()
        {
            var inactiveCodes = _lobbies.Values
                .Where(l => !_connections.ContainsKey(l.HostUserId))
                .Select(l => l.Code)
                .ToList();

            foreach (var code in inactiveCodes)
                _lobbies.TryRemove(code, out _);
        }



        public void SendLobbyMessage(string code, int userId, string username, string message)
        {
            if (!_lobbies.TryGetValue(code, out var lobby))
                return;

            var utc = DateTime.UtcNow.ToString("o");

            var recipients = lobby.Members
                .Select(m => m.UserId)
                .ToList();

            foreach (var id in recipients)
            {
                if (_connections.TryGetValue(id, out var cb))
                {
                    cb.OnMessageReceived(userId, username, message, utc);
                }
            }
        }



        public Lobby GetLobby(string code) =>
            _lobbies.TryGetValue(code, out var lobby) ? lobby : null;

        public bool KickMember(string code, int targetUserId)
        {
            if (!_lobbies.TryGetValue(code, out var lobby))
                return false;

            var target = lobby.Members.FirstOrDefault(m => m.UserId == targetUserId);
            if (target == null)
                return false;

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
            if (!_lobbies.TryGetValue(code, out var lobby))
                return false;

            foreach (var m in lobby.Members)
                if (_connections.TryGetValue(m.UserId, out var cb))
                    cb.OnGameStarted(code);

            return true;
        }
    }
}
