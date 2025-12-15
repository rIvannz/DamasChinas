using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Services;
using DamasChinas_Server.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace DamasChinas_Server.Logic
{
    public sealed class LobbyManager
    {
        private const int MinPlayersToStart = 2;
        private const int MaxPlayersPerLobby = 6;
        private const int ReportsFirstBan = 3;
        private const int ReportsSecondBan = 6;
        private const int ReportsPermanentBan = 10;

        private static readonly Lazy<LobbyManager> _instance =
            new Lazy<LobbyManager>(() => new LobbyManager());

        private readonly ConcurrentDictionary<int, LobbyState> _lobbies;
        private readonly ConcurrentDictionary<string, BanState> _bans;

        private readonly object _codeLock = new object();
        private readonly Random _random = new Random();
        private static readonly ILogService _log = LogFactory.Create(typeof(LobbyManager));

        private LobbyManager()
        {
            _lobbies = new ConcurrentDictionary<int, LobbyState>();
            _bans = new ConcurrentDictionary<string, BanState>(StringComparer.OrdinalIgnoreCase);
        }

        public static LobbyManager Instance => _instance.Value;


        private static void SafeInvokeCallback(
            string context,
            string username,
            Action<ILobbyCallback> action)
        {
            ILobbyCallback callback = LobbySessionManager.Get(username);

            if (callback == null)
            {
                return;
            }

            try
            {
                action(callback);
            }
            catch (Exception ex)
            {
                _log.Warn($"[{context}] Callback failed for user={username}. Error: {ex.Message}");
            }
        }


        public List<LobbySummaryDto> GetPublicLobbies()
        {
            return _lobbies.Values
                .Where(l => l.Visibility == LobbyVisibility.Public && !l.IsGameStarted)
                .Select(l => new LobbySummaryDto
                {
                    LobbyCode = l.LobbyCode,
                    HostUsername = l.HostUsername,
                    CurrentPlayers = l.GetMemberCount(),
                    MaxPlayers = l.MaxPlayers,
                    Visibility = l.Visibility,
                    IsActive = !l.IsGameStarted
                })
                .ToList();
        }

        public LobbySnapshotDto GetLobbyForUser(string username)
        {
            var lobby = FindLobbyByUser(username);

            if (lobby == null)
            {
                return null;
            }

            return lobby.ToSnapshot();
        }

        public BanInfoDto GetBanInfo(string username)
        {
            if (_bans.TryGetValue(username, out var ban))
            {
                if (ban.IsBanned && !ban.IsPermanent && ban.BanUntilUtc.HasValue && ban.BanUntilUtc.Value <= DateTime.UtcNow)
                {
                    ban.IsBanned = false;
                }
                return ban.ToDto();
            }
            return new BanInfoDto { IsBanned = false, TotalReports = 0 };
        }


        public LobbySnapshotDto CreateLobby(string hostUsername, PublicProfile hostProfile, CreateLobbyRequest request, ILobbyCallback callback)
        {
            ValidateCreateRequest(request);
            EnsureNotBanned(hostUsername);

            int lobbyCode = GenerateUniqueCode();

            var lobby = new LobbyState(lobbyCode, request.Visibility, request.MaxPlayers, hostUsername);

            LobbySessionManager.Add(hostUsername, callback);

            lobby.AddOrUpdateMember(LobbyMemberDtoFromProfile(hostProfile, isHost: true));

            if (!_lobbies.TryAdd(lobbyCode, lobby))
            {
                throw new RepositoryValidationException(MessageCode.MatchCreationFailed);
            }

            return lobby.ToSnapshot();
        }

        public LobbySnapshotDto JoinLobby(JoinLobbyRequest request, PublicProfile profile, ILobbyCallback callback)
        {
            ValidateJoinRequest(request);
            EnsureNotBanned(profile.Username);

            var lobby = GetLobbyByCode(request.LobbyCode);
            lobby.ThrowIfGameStarted();
            lobby.ThrowIfFull();

            LobbySessionManager.Add(profile.Username, callback);

            lobby.AddOrUpdateMember(LobbyMemberDtoFromProfile(profile, isHost: lobby.IsHost(profile.Username)));

            BroadcastSnapshot(lobby);
            return lobby.ToSnapshot();
        }

        public void LeaveLobby(string username)
        {
            var lobby = FindLobbyByUser(username);
            if (lobby == null)
            {
                return;
            }

            bool wasHost = lobby.IsHost(username);

            lobby.RemoveMember(username);
            LobbySessionManager.Remove(username);

            if (lobby.IsEmpty || wasHost)
            {
                CloseLobbyInternal(lobby, MessageCode.LobbyClosed);
                return;
            }

        
            if (wasHost)
            {
                lobby.AssignNewHostIfNeeded();
            }

            BroadcastSnapshot(lobby);
        }

        public void KickPlayer(string hostUsername, int lobbyCode, string targetUsername)
        {
            var lobby = GetLobbyByCode(lobbyCode);
            lobby.EnsureHost(hostUsername);

            if (hostUsername == targetUsername)
            {
                return;
            }

            lobby.RemoveMember(targetUsername);

            SafeInvokeCallback("KickPlayer", targetUsername, cb => cb.OnKickedFromLobby(MessageCode.LobbyKicked));
            LobbySessionManager.Remove(targetUsername);

            BroadcastSnapshot(lobby);
        }

        public void StartGame(string hostUsername)
        {
            var lobby = FindLobbyByUser(hostUsername);
            if (lobby == null)
                throw new RepositoryValidationException(MessageCode.LobbyNotFound);

            lobby.EnsureHost(hostUsername);
            lobby.EnsureCanStartGame(MinPlayersToStart);
            lobby.MarkGameStarted();

   
            var members = lobby.GetMembers().ToList();
            _log.Info($"[StartGame] Miembros detectados: {string.Join(", ", members.Select(m => m.Username))}");

            if (members.Count < 2)
            {
                _log.Error("[StartGame] ERROR: La partida NO puede iniciarse, miembros insuficientes.");
                throw new RepositoryValidationException(MessageCode.LobbyMinPlayersNotReached);
            }

      
            var playerUsernames = members.Select(m => m.Username).ToList();
            MatchManager.Instance.CreateMatchFromLobby(lobby.LobbyCode, playerUsernames);
            _log.Info($"[StartGame] Partida creada correctamente con jugadores: {string.Join(", ", playerUsernames)}");

       
            foreach (var member in members)
            {
                SafeInvokeCallback("StartGame", member.Username, cb => cb.OnGameStarting());
            }
        }



        public void BroadcastMessage(int lobbyCode, string sender, string message)
        {
            if (_lobbies.TryGetValue(lobbyCode, out var lobby))
            {
                if (!lobby.ContainsPlayer(sender))
                {
                    return;
                }

                lobby.BroadcastMessage(sender, message);
            }
        }
        public void InviteFriend(
        string hostUsername,
        string friendUsername,
        int lobbyCode,
        Func<string, ILobbyCallback> callbackResolver)
        {
            var lobby = GetLobbyByCode(lobbyCode);
            lobby.EnsureHost(hostUsername);
            EnsureNotBanned(friendUsername);

            var callback = callbackResolver(friendUsername);

            if (callback == null)
            {
                throw new RepositoryValidationException(
                    MessageCode.LobbyInvitationTargetNotOnline);
            }

            callback.OnInvitationReceived(new LobbyInvitationDto
            {
                LobbyCode = lobbyCode,
                HostUsername = hostUsername,
                MaxPlayers = lobby.MaxPlayers
            });

            SendLobbyInvitationEmail(hostUsername, friendUsername, lobbyCode);
        }

        private void SendLobbyInvitationEmail(
        string hostUsername,
        string friendUsername,
        int lobbyCode)
        {
            var usersRepo = new RepositoryUsers();

            string friendEmail = usersRepo.GetEmailByUsername(friendUsername);

            EmailSender.SendInvitationGameEmail(
                friendEmail,
                friendUsername,
                hostUsername,
                lobbyCode
            );
        }




        public void ReportPlayer(ReportPlayerRequest request)
        {
            if (request == null)
            {
                return;
            }

            var banState = _bans.GetOrAdd(request.ReportedUsername, new BanState { TotalReports = 0, IsBanned = false });

            lock (banState)
            {
                banState.TotalReports++;
                ApplyBanLogic(banState);
            }
        }

        public void ApplyReportsOnMatchEnd(string username)
        {
            var banState = _bans.GetOrAdd(username, new BanState());
        }

        private void ApplyBanLogic(BanState state)
        {
            if (state.TotalReports >= ReportsPermanentBan)
            {
                state.IsBanned = true;
                state.IsPermanent = true;
            }
            else if (state.TotalReports >= ReportsSecondBan)
            {
                state.IsBanned = true;
                state.BanUntilUtc = DateTime.UtcNow.AddDays(7);
            }
            else if (state.TotalReports >= ReportsFirstBan)
            {
                state.IsBanned = true;
                state.BanUntilUtc = DateTime.UtcNow.AddHours(24);
            }
        }



        private void BroadcastSnapshot(LobbyState lobby)
        {
            var snapshot = lobby.ToSnapshot();

            foreach (var member in lobby.GetMembers())
            {
                SafeInvokeCallback("Snapshot", member.Username, cb => cb.OnLobbySnapshot(snapshot));
            }
        }

        private void CloseLobbyInternal(LobbyState lobby, MessageCode reason)
        {
            _lobbies.TryRemove(lobby.LobbyCode, out _);

            foreach (var member in lobby.GetMembers())
            {
                SafeInvokeCallback("CloseLobby", member.Username, cb => cb.OnLobbyClosed(reason));
                LobbySessionManager.Remove(member.Username);
            }
        }

        private LobbyState GetLobbyByCode(int lobbyCode)
        {
            if (!_lobbies.TryGetValue(lobbyCode, out var lobby))
            {
                throw new RepositoryValidationException(MessageCode.LobbyNotFound);
            }
            return lobby;
        }

        private LobbyState FindLobbyByUser(string username)
        {
            return _lobbies.Values.FirstOrDefault(l => l.ContainsPlayer(username));
        }

        private void EnsureNotBanned(string username)
        {
            if (_bans.TryGetValue(username, out var ban))
            {
                if (ban.IsBanned && (!ban.BanUntilUtc.HasValue || ban.BanUntilUtc > DateTime.UtcNow))
                {
                    throw new RepositoryValidationException(MessageCode.LobbyUserBanned);
                }
            }
        }

        private int GenerateUniqueCode()
        {
            lock (_codeLock)
            {
                int code;
                do
                {
                    code = _random.Next(100000, 999999);
                }
                while (_lobbies.ContainsKey(code));

                return code;
            }
        }

        private void ValidateCreateRequest(CreateLobbyRequest req)
        {
            if (req == null)
            {
                throw new RepositoryValidationException(MessageCode.MatchCreationFailed);
            }

            if (req.MaxPlayers != 2 && req.MaxPlayers != 4 && req.MaxPlayers != 6)
            {
                throw new RepositoryValidationException(MessageCode.LobbyInvalidMaxPlayers);
            }
        }

        private void ValidateJoinRequest(JoinLobbyRequest req)
        {
            if (req == null || req.LobbyCode <= 0)
            {
                throw new RepositoryValidationException(MessageCode.LobbyNotFound);
            }

            if (string.IsNullOrWhiteSpace(req.Username))
            {
                throw new RepositoryValidationException(MessageCode.UsernameEmpty);
            }
        }

        private static LobbyMemberDto LobbyMemberDtoFromProfile(PublicProfile p, bool isHost)
        {
            return new LobbyMemberDto
            {
                UserId = p.IdUser,
                Username = p.Username,
                AvatarFile = p.AvatarFile,
                IsHost = isHost
            };
        }

        private sealed class LobbyState
        {
            private readonly ConcurrentDictionary<string, LobbyMember> _members = new ConcurrentDictionary<string, LobbyMember>();

            public int LobbyCode { get; }
            public LobbyVisibility Visibility { get; }
            public int MaxPlayers { get; }
            public string HostUsername { get; private set; }
            public bool IsGameStarted { get; private set; }
            public bool IsEmpty => _members.IsEmpty;

            public LobbyState(int code, LobbyVisibility vis, int max, string host)
            {
                LobbyCode = code;
                Visibility = vis;
                MaxPlayers = max;
                HostUsername = host;
            }

            public void AddOrUpdateMember(LobbyMemberDto dto)
            {
                _members[dto.Username] = new LobbyMember(dto);
            }

            public void RemoveMember(string username)
            {
                _members.TryRemove(username, out _);
            }

            public bool ContainsPlayer(string username)
            {
                return _members.ContainsKey(username);
            }

            public int GetMemberCount()
            {
                return _members.Count;
            }

            public bool IsHost(string username)
            {
                return string.Equals(HostUsername, username, StringComparison.OrdinalIgnoreCase);
            }

            public void AssignNewHostIfNeeded()
            {
                if (_members.IsEmpty)
                {
                    HostUsername = null;
                    return;
                }

                HostUsername = _members.Values.OrderBy(m => m.JoinedAtUtc).First().Username;
            }

            public void ThrowIfFull()
            {
                if (_members.Count >= MaxPlayers)
                {
                    throw new RepositoryValidationException(MessageCode.LobbyFull);
                }
            }

            public void ThrowIfGameStarted()
            {
                if (IsGameStarted)
                {
                    throw new RepositoryValidationException(MessageCode.LobbyGameAlreadyStarted);
                }
            }

            public void EnsureCanStartGame(int min)
            {
                int count = _members.Count;

                if (count < min)
                {
                    throw new RepositoryValidationException(MessageCode.LobbyMinPlayersNotReached);
                }

                if (count != 2 && count != 4 && count != 6)
                {
                    throw new RepositoryValidationException(MessageCode.LobbyInvalidMaxPlayers);
                }
            }

            public void EnsureHost(string u)
            {
                if (!IsHost(u))
                {
                    throw new RepositoryValidationException(MessageCode.LobbyNotHost);
                }
            }

            public void MarkGameStarted()
            {
                IsGameStarted = true;
            }

            public IEnumerable<LobbyMember> GetMembers()
            {
                return _members.Values;
            }

            public void BroadcastMessage(string sender, string msg)
            {
                string time = DateTime.UtcNow.ToString("O"); // Formato ISO 8601 estándar

                foreach (var m in _members.Values)
                {
                    SafeInvokeCallback("Chat", m.Username, cb => cb.OnChatMessageReceived(sender, msg, time));
                }
            }

            public LobbySnapshotDto ToSnapshot()
            {
                return new LobbySnapshotDto
                {
                    LobbyCode = LobbyCode,
                    Visibility = Visibility,
                    MaxPlayers = MaxPlayers,
                    IsGameStarted = IsGameStarted,
                    Members = _members.Values.Select(m => m.ToDto(IsHost(m.Username)))
                                           .OrderByDescending(m => m.IsHost).ToArray()
                };
            }
        }

        private sealed class LobbyMember
        {
            public string Username { get; }
            public string AvatarFile { get; }
            public int UserId { get; }
            public DateTime JoinedAtUtc { get; }

            public LobbyMember(LobbyMemberDto dto)
            {
                Username = dto.Username;
                AvatarFile = dto.AvatarFile;
                UserId = dto.UserId;
                JoinedAtUtc = DateTime.UtcNow;
            }

            public LobbyMemberDto ToDto(bool isHost)
            {
                return new LobbyMemberDto
                {
                    Username = Username,
                    AvatarFile = AvatarFile,
                    UserId = UserId,
                    IsHost = isHost
                };
            }
        }

        private sealed class BanState
        {
            public bool IsBanned { get; set; }
            public bool IsPermanent { get; set; }
            public DateTime? BanUntilUtc { get; set; }
            public int TotalReports { get; set; }

            public BanInfoDto ToDto()
            {
                return new BanInfoDto
                {
                    IsBanned = IsBanned,
                    IsPermanent = IsPermanent,
                    BanUntilUtc = BanUntilUtc,
                    TotalReports = TotalReports
                };
            }
        }
    }
}