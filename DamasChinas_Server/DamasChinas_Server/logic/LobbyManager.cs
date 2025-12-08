using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Services;

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

        // =========================================================
        //  SAFE CALLBACK INVOCATION
        // =========================================================
        private static void SafeInvokeCallback(
            string context,
            string username,
            Action<ILobbyCallback> action)
        {
            ILobbyCallback callback = LobbySessionManager.Get(username);
            if (callback == null)
                return;

            try
            {
                action(callback);
            }
            catch (CommunicationObjectAbortedException ex)
            {
                _log.Warn($"[{context}] Callback abortado (user={username})", ex);
            }
            catch (ObjectDisposedException ex)
            {
                _log.Warn($"[{context}] Callback disposed (user={username})", ex);
            }
            catch (Exception ex)
            {
                _log.Error($"[{context}] Error inesperado (user={username})", ex);
            }
        }

        // =========================================================
        //  CREAR LOBBY
        // =========================================================
        public LobbySnapshotDto CreateLobby(
            string hostUsername,
            PublicProfile hostProfile,
            CreateLobbyRequest request,
            ILobbyCallback callback)
        {
            ValidateCreateRequest(request);

            int lobbyCode = GenerateUniqueCode();

            var lobby = new LobbyState(
                lobbyCode,
                request.Visibility,
                request.MaxPlayers,
                hostUsername);

            LobbySessionManager.Add(hostUsername, callback);

            lobby.AddOrUpdateMember(
                LobbyMemberDtoFromProfile(hostProfile, isHost: true));

            if (!_lobbies.TryAdd(lobbyCode, lobby))
                throw new RepositoryValidationException(MessageCode.MatchCreationFailed);

            return lobby.ToSnapshot();
        }

        // =========================================================
        //  JOIN LOBBY
        // =========================================================
        public LobbySnapshotDto JoinLobby(
            JoinLobbyRequest request,
            PublicProfile profile,
            ILobbyCallback callback)
        {
            ValidateJoinRequest(request);
            EnsureNotBanned(profile.Username);

            var lobby = GetLobbyByCode(request.LobbyCode);
            lobby.ThrowIfGameStarted();
            lobby.ThrowIfFull();

            LobbySessionManager.Add(profile.Username, callback);

            lobby.AddOrUpdateMember(
                LobbyMemberDtoFromProfile(profile, isHost: lobby.IsHost(profile.Username)));

            BroadcastSnapshot(lobby);

            return lobby.ToSnapshot();
        }

        // =========================================================
        //  LEAVE LOBBY
        // =========================================================
        public void LeaveLobby(string username)
        {
            var lobby = FindLobbyByUser(username);
            if (lobby == null)
                return;

            bool wasHost = lobby.IsHost(username);

            lobby.RemoveMember(username);
            LobbySessionManager.Remove(username);

            if (lobby.IsEmpty)
            {
                CloseLobbyInternal(lobby, MessageCode.LobbyClosed);
                return;
            }

            if (wasHost)
                lobby.AssignNewHostIfNeeded();

            BroadcastSnapshot(lobby);
        }

        // =========================================================
        //  KICK PLAYER
        // =========================================================
        public void KickPlayer(string hostUsername, int lobbyCode, string targetUsername)
        {
            var lobby = GetLobbyByCode(lobbyCode);

            lobby.EnsureHost(hostUsername);
            lobby.RemoveMember(targetUsername);

            SafeInvokeCallback(
                "KickPlayer.OnKickedFromLobby",
                targetUsername,
                cb => cb.OnKickedFromLobby(MessageCode.LobbyKicked)); 

            LobbySessionManager.Remove(targetUsername);

            BroadcastSnapshot(lobby);
        }

        // =========================================================
        //  START GAME
        // =========================================================
        public void StartGame(string hostUsername)
        {
            var lobby = FindLobbyByUser(hostUsername);
            if (lobby == null)
                throw new RepositoryValidationException(MessageCode.LobbyNotFound);

            lobby.EnsureHost(hostUsername);
            lobby.EnsureCanStartGame(MinPlayersToStart);
            lobby.MarkGameStarted();

            foreach (LobbyMember member in lobby.GetMembers())
            {
                SafeInvokeCallback(
                    "StartGame.OnGameStarting",
                    member.Username,
                    cb => cb.OnGameStarting());
            }
        }

        // =========================================================
        //  INVITE FRIEND
        // =========================================================
        public void InviteFriend(
            string hostUsername,
            string friendUsername,
            int lobbyCode,
            Func<string, ILobbyCallback> callbackResolver)
        {
            var lobby = GetLobbyByCode(lobbyCode);

            lobby.EnsureHost(hostUsername);
            EnsureNotBanned(friendUsername);

            ILobbyCallback callback = callbackResolver(friendUsername);
            if (callback == null)
                throw new RepositoryValidationException(MessageCode.LobbyInvitationTargetNotOnline);

            callback.OnInvitationReceived(new LobbyInvitationDto
            {
                LobbyCode = lobbyCode,
                HostUsername = hostUsername,
                MaxPlayers = lobby.MaxPlayers
            });
        }

        // =========================================================
        //  BROADCAST SNAPSHOT
        // =========================================================
        private void BroadcastSnapshot(LobbyState lobby)
        {
            LobbySnapshotDto snapshot = lobby.ToSnapshot();

            foreach (LobbyMember member in lobby.GetMembers())
            {
                SafeInvokeCallback(
                    "BroadcastSnapshot.OnLobbySnapshot",
                    member.Username,
                    cb => cb.OnLobbySnapshot(snapshot));
            }
        }

        // =========================================================
        //  CLOSE LOBBY
        // =========================================================
        private void CloseLobbyInternal(LobbyState lobby, MessageCode reason)
        {
            _lobbies.TryRemove(lobby.LobbyCode, out _);

            foreach (LobbyMember member in lobby.GetMembers())
            {
                SafeInvokeCallback(
                    "CloseLobbyInternal.OnLobbyClosed",
                    member.Username,
                    cb => cb.OnLobbyClosed(reason));

                LobbySessionManager.Remove(member.Username);
            }
        }

        // =========================================================
        //  VALIDATION + HELPERS
        // =========================================================
        private static LobbyMemberDto LobbyMemberDtoFromProfile(
            PublicProfile p, bool isHost)
        {
            return new LobbyMemberDto
            {
                UserId = p.IdUser,
                Username = p.Username,
                AvatarFile = p.AvatarFile,
                IsHost = isHost
            };
        }

        private LobbyState GetLobbyByCode(int lobbyCode)
        {
            if (!_lobbies.TryGetValue(lobbyCode, out var lobby))
                throw new RepositoryValidationException(MessageCode.LobbyNotFound);

            return lobby;
        }

        private LobbyState FindLobbyByUser(string username)
        {
            return _lobbies.Values.FirstOrDefault(l => l.ContainsPlayer(username));
        }

        private int GenerateUniqueCode()
        {
            lock (_codeLock)
            {
                int code;
                do { code = _random.Next(100000, 999999); }
                while (_lobbies.ContainsKey(code));
                return code;
            }
        }

        private void ValidateCreateRequest(CreateLobbyRequest request)
        {
            if (request == null)
                throw new RepositoryValidationException(MessageCode.MatchCreationFailed);

            if (request.MaxPlayers != 2 &&
                request.MaxPlayers != 4 &&
                request.MaxPlayers != 6)
                throw new RepositoryValidationException(MessageCode.LobbyInvalidMaxPlayers);
        }

        private void ValidateJoinRequest(JoinLobbyRequest request)
        {
            if (request == null || request.LobbyCode <= 0)
                throw new RepositoryValidationException(MessageCode.LobbyNotFound);

            if (string.IsNullOrWhiteSpace(request.Username))
                throw new RepositoryValidationException(MessageCode.UsernameEmpty);
        }

        private void EnsureNotBanned(string username)
        {
            if (!_bans.TryGetValue(username, out var ban))
                return;

            if (!ban.IsBanned)
                return;

            if (!ban.IsPermanent && ban.BanUntilUtc <= DateTime.UtcNow)
            {
                ban.IsBanned = false;
                _bans[username] = ban;
                return;
            }

            throw new RepositoryValidationException(MessageCode.LobbyUserBanned);
        }

        // =========================================================
        //  INTERNAL CLASSES
        // =========================================================
        private sealed class LobbyState
        {
            private readonly ConcurrentDictionary<string, LobbyMember> _members;

            public LobbyState(int lobbyCode,
                LobbyVisibility visibility,
                int maxPlayers,
                string hostUsername)
            {
                LobbyCode = lobbyCode;
                Visibility = visibility;
                MaxPlayers = maxPlayers;
                HostUsername = hostUsername;

                _members = new ConcurrentDictionary<string, LobbyMember>();
            }

            public int LobbyCode { get; }
            public LobbyVisibility Visibility { get; }
            public int MaxPlayers { get; }
            public string HostUsername { get; private set; }
            public bool IsGameStarted { get; private set; }
            public bool IsEmpty => _members.IsEmpty;

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

            public bool IsHost(string username)
            {
                return string.Equals(
                    HostUsername,
                    username,
                    StringComparison.OrdinalIgnoreCase);
            }

            public void AssignNewHostIfNeeded()
            {
                if (_members.IsEmpty)
                {
                    HostUsername = null;
                    return;
                }

                HostUsername = _members.Values
                    .OrderBy(m => m.JoinedAtUtc)
                    .First().Username;
            }

            public void ThrowIfFull()
            {
                if (_members.Count >= MaxPlayers)
                    throw new RepositoryValidationException(MessageCode.LobbyFull);
            }

            public void ThrowIfGameStarted()
            {
                if (IsGameStarted)
                    throw new RepositoryValidationException(MessageCode.LobbyGameAlreadyStarted);
            }

            public void EnsureCanStartGame(int minPlayers)
            {
                int count = _members.Count;

                if (count < minPlayers)
                    throw new RepositoryValidationException(MessageCode.LobbyMinPlayersNotReached);

                if (count != 2 && count != 4 && count != 6)
                    throw new RepositoryValidationException(MessageCode.LobbyInvalidMaxPlayers);
            }

            public void EnsureHost(string username)
            {
                if (!IsHost(username))
                    throw new RepositoryValidationException(MessageCode.LobbyNotHost);
            }

            public void MarkGameStarted()
            {
                IsGameStarted = true;
            }

            public LobbySnapshotDto ToSnapshot()
            {
                return new LobbySnapshotDto
                {
                    LobbyCode = LobbyCode,
                    Visibility = Visibility,
                    MaxPlayers = MaxPlayers,
                    Members = _members.Values
                        .Select(m => m.ToDto(IsHost(m.Username)))
                        .OrderByDescending(m => m.IsHost)
                        .ThenBy(m => m.Username)
                        .ToArray(),
                    IsGameStarted = IsGameStarted
                };
            }

            public IEnumerable<LobbyMember> GetMembers()
            {
                return _members.Values;
            }
        }

        private sealed class LobbyMember
        {
            public LobbyMember(LobbyMemberDto dto)
            {
                Username = dto.Username;
                AvatarFile = dto.AvatarFile;
                UserId = dto.UserId;
                JoinedAtUtc = DateTime.UtcNow;
            }

            public string Username { get; }
            public string AvatarFile { get; }
            public int UserId { get; }
            public DateTime JoinedAtUtc { get; }

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
