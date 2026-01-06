using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.GameRepositories;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Services;
using DamasChinas_Server.Utilities;
using DamasChinas_Shared.Contracts.Dtos;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;

namespace DamasChinas_Server.Logic
{
    public sealed class LobbyManager
    {
        private const int MinPlayersToStart = 2;

        private static readonly Lazy<LobbyManager> _instance =
            new Lazy<LobbyManager>(() => new LobbyManager());

        private readonly ConcurrentDictionary<int, LobbyState> _lobbies;

        private readonly object _codeLock = new object();
        private static readonly RandomNumberGenerator _randomGenerator = RandomNumberGenerator.Create();

        private static readonly ILogService _log = LogFactory.Create(typeof(LobbyManager));

        private LobbyManager()
        {
            _lobbies = new ConcurrentDictionary<int, LobbyState>();
        }

        public static LobbyManager Instance => _instance.Value;

        private static void SafeInvokeCallback(string context, string username, Action<ILobbyCallback> action)
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
            catch (CommunicationException ex)
            {
                _log.Warn(
                    $"[{context}] Callback FAILED → user disconnected: {username}. Error: {ex.Message}");

                LobbySessionManager.Remove(username);


            }
        }


        public void HandleUnexpectedDisconnect(string username)
        {
            var lobby = FindLobbyByUser(username);
            if (lobby == null)
            {
                return;
            }

            bool wasHost = lobby.IsHost(username);

            lobby.RemoveMember(username);

            _log.Warn($"[LobbyManager] User disconnected unexpectedly: {username}");

            lobby.BroadcastMessage(
                "Server",
                $"{username} has been disconnected.");

            if (!lobby.IsGameStarted && (lobby.IsEmpty || wasHost))
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
            if (string.IsNullOrWhiteSpace(username) || IsGuest(username))
            {
                return new BanInfoDto { IsBanned = false, TotalReports = 0 };
            }

            try
            {
                var usersRepo = new RepositoryUsers();
                int userId = usersRepo.GetUserIdByUsername(username);

                var reportsRepo = new RepositoryReports();
                int total = reportsRepo.CountReportsForUser(userId);

                var sanctionsRepo = new RepositorySanctions();
                BanInfoDto ban = sanctionsRepo.GetActiveBanInfo(userId);

                ban.TotalReports = total;
                return ban;
            }
            catch
            {
                return new BanInfoDto { IsBanned = false, TotalReports = 0 };
            }
        }

        public LobbySnapshotDto CreateLobby(
            string hostUsername,
            PublicProfile hostProfile,
            CreateLobbyRequest request,
            ILobbyCallback callback)
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

            lobby.ThrowIfKicked(profile.Username);

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

          
            if (!lobby.IsGameStarted && (lobby.IsEmpty || wasHost))
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

            if (string.Equals(hostUsername, targetUsername, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            lobby.AddKickedUser(targetUsername);

            lobby.RemoveMember(targetUsername);

            SafeInvokeCallback("KickPlayer", targetUsername, cb => cb.OnKickedFromLobby(MessageCode.LobbyKicked));
            LobbySessionManager.Remove(targetUsername);

            BroadcastSnapshot(lobby);
        }

        public void StartGame(string hostUsername)
        {
            var lobby = FindLobbyByUser(hostUsername);
            if (lobby == null)
            {
                throw new RepositoryValidationException(MessageCode.LobbyNotFound);
            }

            lobby.EnsureHost(hostUsername);

            var members = lobby.GetMembers()
                .Where(m => LobbySessionManager.IsOnline(m.Username))
                .ToList();

            _log.Info($"[StartGame] Miembros ONLINE detectados: {string.Join(", ", members.Select(m => m.Username))}");

            if (members.Count < MinPlayersToStart ||
                (members.Count != 2 && members.Count != 4 && members.Count != 6))
            {
                _log.Warn(
                    $"[StartGame] Partida NO iniciada. Jugadores online: {members.Count}");

                throw new RepositoryValidationException(MessageCode.LobbyMinPlayersNotReached);
            }

            lobby.MarkGameStarted();

            var playerUsernames = members
                .Select(m => m.Username)
                .ToList();

            MatchManager.Instance.CreateMatchFromLobby(
                lobby.LobbyCode,
                playerUsernames);

            _log.Info(
                $"[StartGame] Partida creada correctamente con jugadores: {string.Join(", ", playerUsernames)}");

            foreach (var member in members)
            {
                SafeInvokeCallback(
                    "StartGame",
                    member.Username,
                    cb => cb.OnGameStarting());
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

        public void InviteFriend( string hostUsername,string friendUsername, int lobbyCode,string languageCode, Func<string, ILobbyCallback> callbackResolver)
        {
            var lobby = GetLobbyByCode(lobbyCode);
            lobby.EnsureHost(hostUsername);
            EnsureNotBanned(friendUsername);

            SendLobbyInvitationEmail(hostUsername, friendUsername, lobbyCode, languageCode);
        }


        private static void SendLobbyInvitationEmail(string hostUsername, string friendUsername,int lobbyCode, string languageCode)
        {
            var usersRepo = new RepositoryUsers();
            string friendEmail = usersRepo.GetEmailByUsername(friendUsername);

            EmailSender.SendInvitationGameEmail(
                friendEmail,
                friendUsername,
                hostUsername,
                lobbyCode,
                languageCode
            );
        }


        public void ReportPlayer(ReportPlayerRequest request)
        {
            if (!IsValidReportRequest(request))
            {
                return;
            }

            try
            {
                BanInfoDto banInfo = ProcessReport(request);

                NotifyBanStatus(request.ReportedUsername, banInfo);

                if (banInfo.IsBanned)
                {
                    HandleBannedPlayer(request.ReportedUsername, banInfo);
                }
            }
            catch (RepositoryValidationException)
            {
                _log.Error($"[LobbyManager.ReportPlayer] Error");
                throw new RepositoryValidationException(MessageCode.UnknownError);
            }
        }

        private static bool IsValidReportRequest(ReportPlayerRequest request)
        {
            if (request == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.ReporterUsername) ||
                string.IsNullOrWhiteSpace(request.ReportedUsername))
            {
                return false;
            }

            if (IsGuest(request.ReporterUsername) || IsGuest(request.ReportedUsername))
            {
                return false;
            }

            if (string.Equals(
                request.ReporterUsername,
                request.ReportedUsername,
                StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private static BanInfoDto ProcessReport(ReportPlayerRequest request)
        {
            var usersRepo = new RepositoryUsers();
            int reporterId = usersRepo.GetUserIdByUsername(request.ReporterUsername);
            int reportedId = usersRepo.GetUserIdByUsername(request.ReportedUsername);

            string motivo = request.Reason ?? string.Empty;

            var reportsRepo = new RepositoryReports();

            int totalReports = reportsRepo.AddReportAndGetTotal(
                reporterId,
                reportedId,
                request.IdPartida,
                request.CodigoLobby,
                motivo);

            var sanctionsRepo = new RepositorySanctions();

            BanInfoDto banInfo = sanctionsRepo.ApplyBanFromReports(
                reportedId,
                totalReports,
                motivo);

        
            banInfo.TotalReports = totalReports;
            banInfo.LastReportReason = motivo;

            return banInfo;
        }


        private static void NotifyBanStatus(string username, BanInfoDto banInfo)
        {
            var sessionCb = SessionManager.GetSession(username);
            if (sessionCb != null)
            {
                try
                {
                    sessionCb.OnBanStatusUpdated(banInfo);
                }
                catch (CommunicationException ex)
                {
                    _log.Warn($"[LobbyManager.ReportPlayer] Session callback FAILED for {username}: {ex.Message}");
                    SessionManager.RemoveSession(username);
                }
            }

            SafeInvokeCallback(
                "BanStatusUpdated",
                username,
                cb => cb.OnBanStatusUpdated(banInfo));
        }

        private void HandleBannedPlayer(string username, BanInfoDto banInfo)
        {
            MatchManager.Instance.NotifyBanAndKickIfInMatch(username, banInfo);

            var lobby = FindLobbyByUser(username);
            if (lobby == null)
            {
                return;
            }

            bool wasHost = lobby.IsHost(username);

            lobby.RemoveMember(username);

            SafeInvokeCallback(
                "KickedFromLobby",
                username,
                cb => cb.OnKickedFromLobby(MessageCode.LobbyUserBanned));

            LobbySessionManager.Remove(username);

            if (wasHost)
            {
                CloseLobbyInternal(lobby, MessageCode.LobbyClosed);
                return;
            }

            BroadcastSnapshot(lobby);
        }

        private static void BroadcastSnapshot(LobbyState lobby)
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

            foreach (var username in lobby.GetMembers().Select(member => member.Username))
            {
                SafeInvokeCallback("CloseLobby", username, cb => cb.OnLobbyClosed(reason));
                LobbySessionManager.Remove(username);
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

        private static void EnsureNotBanned(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            if (IsGuest(username))
            {
                return;
            }

            var usersRepo = new RepositoryUsers();
            int userId = usersRepo.GetUserIdByUsername(username);

            var sanctionsRepo = new RepositorySanctions();
            if (sanctionsRepo.HasActiveBan(userId))
            {
                throw new RepositoryValidationException(MessageCode.LobbyUserBanned);
            }
        }

        private static int NextSecureInt(int minInclusive, int maxExclusive)
        {
            if (minInclusive >= maxExclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            }

            long diff = (long)maxExclusive - minInclusive;
            byte[] buffer = new byte[4];

            while (true)
            {
                _randomGenerator.GetBytes(buffer);
                uint rand = BitConverter.ToUInt32(buffer, 0);

                const long max = 1L + uint.MaxValue;
                long remainder = max % diff;

                if (rand < max - remainder)
                {
                    return (int)(minInclusive + (rand % diff));
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
                    code = NextSecureInt(100000, 1000000);
                }
                while (_lobbies.ContainsKey(code));

                return code;
            }
        }

        private static void ValidateCreateRequest(CreateLobbyRequest req)
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

        private static void ValidateJoinRequest(JoinLobbyRequest req)
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
            private readonly ConcurrentDictionary<string, LobbyMember> _members =
                new ConcurrentDictionary<string, LobbyMember>(StringComparer.OrdinalIgnoreCase);

            private readonly ConcurrentDictionary<string, byte> _kickedUsers =
                new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

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

            public void AddKickedUser(string username)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return;
                }

                _kickedUsers[username] = 0;
            }

            public void ThrowIfKicked(string username)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return;
                }

                if (_kickedUsers.ContainsKey(username))
                {
                    throw new RepositoryValidationException(MessageCode.LobbyKicked);
                }
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
                string time = DateTime.UtcNow.ToString("O");

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
                    Members = _members.Values
                        .Select(m => m.ToDto(IsHost(m.Username)))
                        .OrderByDescending(m => m.IsHost)
                        .ToArray()
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

        private static bool IsGuest(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            if (!username.StartsWith("Guest-", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string tail = username.Substring("Guest-".Length);
            return tail.Length == 4 && int.TryParse(tail, out _);
        }
    }
}
