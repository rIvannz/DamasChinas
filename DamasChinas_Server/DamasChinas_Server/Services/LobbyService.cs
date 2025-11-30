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

        private static ILobbyCallback CurrentCallback =>
            OperationContext.Current.GetCallbackChannel<ILobbyCallback>();

        private static string NewCode() =>
            Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

        // ============================================
        // CREATE LOBBY
        // ============================================

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

        // ============================================
        // JOIN LOBBY
        // ============================================

        public Lobby JoinLobby(string code, int userId, string username)
        {
            if (!_lobbies.TryGetValue(code, out var lobby))
                throw new FaultException(MessageCode.LobbyNotFound.ToString());

            ValidateLobbyStatus(lobby);

            AddMemberIfNotExists(lobby, userId, username);

            _connections[userId] = CurrentCallback;

            NotifyJoin(lobby, userId, username);

            return lobby;
        }

        private static void ValidateLobbyStatus(Lobby lobby)
        {
            if (lobby == null)
                throw new FaultException(MessageCode.LobbyNotFound.ToString());

            if (lobby.HostUserId <= 0)
                throw new FaultException(MessageCode.UnknownError.ToString());
        }

        private static void AddMemberIfNotExists(Lobby lobby, int userId, string username)
        {
            if (!lobby.Members.Any(m => m.UserId == userId))
            {
                lobby.Members.Add(new LobbyMember
                {
                    UserId = userId,
                    Username = username,
                    IsHost = false
                });
            }
        }

        private void NotifyJoin(Lobby lobby, int newUserId, string username)
        {
            foreach (var member in lobby.Members)
            {
                if (_connections.TryGetValue(member.UserId, out var cb))
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            cb.OnMemberJoined(new LobbyMember
                            {
                                UserId = newUserId,
                                Username = username,
                                IsHost = (lobby.HostUserId == newUserId)
                            });
                        }
                        catch
                        {
                            _connections.TryRemove(member.UserId, out _);
                        }
                    });
                }
            }
        }

        // ============================================
        // PUBLIC LOBBIES
        // ============================================

        public List<Lobby> GetPublicLobbies()
        {
            CleanupInactiveLobbies();

            return _lobbies.Values
                .Where(l => !l.IsPrivate && _connections.ContainsKey(l.HostUserId))
                .ToList();
        }

        // ============================================
        // LEAVE LOBBY
        // ============================================

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
            foreach (var m in lobby.Members)
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
        }

        // ============================================
        // CLOSE LOBBY
        // ============================================

        private void CloseLobby(Lobby lobby)
        {
            foreach (var m in lobby.Members)
            {
                if (_connections.TryGetValue(m.UserId, out var cb))
                {
                    try
                    {
                        cb.OnLobbyClosed(MessageCode.LobbyClosed.ToString());
                    }
                    catch
                    {
                        _connections.TryRemove(m.UserId, out _);
                    }
                }
            }

            _lobbies.TryRemove(lobby.Code, out _);
        }

        private void CleanupInactiveLobbies()
        {
            var inactive = _lobbies.Values
                .Where(l => !_connections.ContainsKey(l.HostUserId))
                .Select(l => l.Code)
                .ToList();

            foreach (var code in inactive)
                _lobbies.TryRemove(code, out _);
        }

        // ============================================
        // CHAT
        // ============================================

        public void SendLobbyMessage(string code, int userId, string username, string message)
        {
            if (!_lobbies.TryGetValue(code, out var lobby))
                return;

            var utc = DateTime.UtcNow.ToString("o");

            foreach (var m in lobby.Members)
            {
                if (_connections.TryGetValue(m.UserId, out var cb))
                {
                    cb.OnMessageReceived(userId, username, message, utc);
                }
            }
        }

        public Lobby GetLobby(string code) =>
            _lobbies.TryGetValue(code, out var lobby) ? lobby : null;

        // ============================================
        // BAN / KICK
        // ============================================

        public bool KickMember(string code, int targetUserId)
        {
            if (!_lobbies.TryGetValue(code, out var lobby))
                return false;

            return RemoveMember(lobby, targetUserId);
        }

        public bool BanMember(string code, int targetUserId)
        {
            if (!_lobbies.TryGetValue(code, out var lobby))
                return false;

            if (!RemoveMember(lobby, targetUserId))
                return false;

            lobby.BannedUsers.Add(targetUserId);
            return true;
        }

        private bool RemoveMember(Lobby lobby, int targetUserId)
        {
            var member = lobby.Members.FirstOrDefault(m => m.UserId == targetUserId);
            if (member == null)
                return false;

            lobby.Members.Remove(member);
            _connections.TryRemove(targetUserId, out _);

            foreach (var m in lobby.Members)
                if (_connections.TryGetValue(m.UserId, out var cb))
                    cb.OnMemberLeft(targetUserId);

            return true;
        }

        // ============================================
        // GAME STARTED ? USUARIOS EN PARTIDA
        // ============================================

        public bool StartGame(string code)
        {
            if (!_lobbies.TryGetValue(code, out var lobby))
                return false;

            foreach (var m in lobby.Members)
            {
           
                SessionManager.ForEachSession(cb =>
                {
                    try
                    {
                        cb.PlayerInGame(m.Username);
                    }
                    catch { }
                });

                if (_connections.TryGetValue(m.UserId, out var cbLobby))
                {
                    cbLobby.OnGameStarted(code);
                }
            }

            return true;
        }
    }
}
