using System;
using System.Collections.Generic;
using System.ServiceModel;
using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Logic;
using DamasChinas_Shared.Contracts.Dtos;

namespace DamasChinas_Server.Services
{
    [DbGuardBehavior]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class LobbyService : ILobbyService
    {
        private readonly RepositoryUsers _userRepository;
        private readonly LobbyManager _lobbyManager;
        private readonly ILogService _log;

        private string _username;
        private bool _hasLeft;

        public LobbyService()
            : this(new RepositoryUsers(), LobbyManager.Instance, LogFactory.Create<LobbyService>())
        {
        }

        internal LobbyService(RepositoryUsers userRepository, LobbyManager lobbyManager, ILogService log)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _lobbyManager = lobbyManager ?? throw new ArgumentNullException(nameof(lobbyManager));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public List<LobbySummaryDto> GetPublicLobbies()
        {
            return ExecuteRequest(() => _lobbyManager.GetPublicLobbies(), "GetPublicLobbies");
        }

        public LobbySnapshotDto GetCurrentLobby(string username)
        {
            return ExecuteRequest(() => _lobbyManager.GetLobbyForUser(username), "GetCurrentLobby");
        }

        public OperationResult CreateLobby(string hostUsername, CreateLobbyRequest request)
        {
            try
            {
                var profile = GetProfile(hostUsername);
                var callback = GetLobbyCallback();

                BindChannelToUser(hostUsername);

                LobbySessionManager.Add(hostUsername, callback);

                _lobbyManager.CreateLobby(hostUsername, profile, request, callback);
                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                return HandleException(ex, "CreateLobby");
            }
        }

        public OperationResult JoinLobby(JoinLobbyRequest request)
        {
            try
            {
                var profile = GetProfile(request.Username);
                var callback = GetLobbyCallback();

                BindChannelToUser(request.Username);

                LobbySessionManager.Add(request.Username, callback);

                _lobbyManager.JoinLobby(request, profile, callback);
                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                return HandleException(ex, "JoinLobby");
            }
        }

        public OperationResult LeaveLobby(string username)
        {
            try
            {
                _hasLeft = true;

                _lobbyManager.LeaveLobby(username);
                LobbySessionManager.Remove(username);
                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                return HandleException(ex, "LeaveLobby");
            }
        }

        public OperationResult StartGame(string hostUsername)
        {
            return ExecuteOperation(() => _lobbyManager.StartGame(hostUsername), "StartGame");
        }

        public OperationResult KickPlayer(string hostUsername, int lobbyCode, string targetUsername)
        {
            return ExecuteOperation(() => _lobbyManager.KickPlayer(hostUsername, lobbyCode, targetUsername), "KickPlayer");
        }

        public OperationResult ReportPlayer(ReportPlayerRequest request)
        {
            return ExecuteOperation(() => _lobbyManager.ReportPlayer(request), "ReportPlayer");
        }

        public BanInfoDto GetBanInfo(string username)
        {
            return ExecuteRequest(() => _lobbyManager.GetBanInfo(username), "GetBanInfo");
        }

        public OperationResult InviteFriend(string hostUsername, string friendUsername, int lobbyCode, string languageCode)
        {
            return ExecuteOperation(() =>
                _lobbyManager.InviteFriend(hostUsername, friendUsername, lobbyCode, languageCode, ResolveCallbackForUser),
                "InviteFriend");
        }


        public void SendLobbyMessage(string sender, int lobbyCode, string message)
        {
            try
            {
                _lobbyManager.BroadcastMessage(lobbyCode, sender, message);
            }
            catch (Exception ex)
            {
                _log.Error($"Error sending message: {ex.Message}", ex);
            }
        }

        private void BindChannelToUser(string username)
        {
            _username = username;
            _hasLeft = false;

            var channel = OperationContext.Current?.Channel;
            if (channel == null)
            {
                return;
            }

            channel.Closed -= OnChannelClosedOrFaulted;
            channel.Faulted -= OnChannelClosedOrFaulted;

            channel.Closed += OnChannelClosedOrFaulted;
            channel.Faulted += OnChannelClosedOrFaulted;
        }

        private void OnChannelClosedOrFaulted(object sender, EventArgs e)
        {
            try
            {
                if (_hasLeft)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(_username))
                {
                    return;
                }

                _hasLeft = true;

                _log.Warn($"[LobbyService] Channel closed/faulted for user={_username}");

                try
                {
                    _lobbyManager.HandleUnexpectedDisconnect(_username);
                }
                catch (Exception ex)
                {
                    _log.Error($"[LobbyService] HandleUnexpectedDisconnect error: {ex.Message}", ex);
                }
                finally
                {
                    LobbySessionManager.Remove(_username);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"[LobbyService.OnChannelClosedOrFaulted] {ex.Message}", ex);
            }
        }

        private PublicProfile GetProfile(string username)
        {
            int id = _userRepository.GetUserIdByUsername(username);
            return _userRepository.GetPublicProfile(id);
        }

        private static ILobbyCallback GetLobbyCallback()
        {
            return OperationContext.Current.GetCallbackChannel<ILobbyCallback>();
        }

        private static ILobbyCallback ResolveCallbackForUser(string username)
        {
            return LobbySessionManager.Get(username);
        }

        private T ExecuteRequest<T>(Func<T> action, string context)
        {
            try
            {
                return action();
            }
            catch (RepositoryValidationException ex)
            {
                _log.Warn($"[{context}] Validation Error: {ex.Code}");
                throw new FaultException<MessageCode>(ex.Code);
            }
            catch (Exception ex)
            {
                _log.Error($"[{context}] Error: {ex.Message}", ex);
                throw new FaultException<MessageCode>(MessageCode.UnknownError);
            }
        }

        public OperationResult JoinLobbyGuest(JoinLobbyRequest request)
        {
            try
            {
                if (request == null || request.LobbyCode <= 0)
                {
                    return OperationResult.Fail("Invalid lobby.", MessageCode.LobbyNotFound);
                }

                if (!IsGuestUsername(request.Username))
                {
                    return OperationResult.Fail("Invalid guest username.", MessageCode.UsernameEmpty);
                }

                var callback = GetLobbyCallback();

                BindChannelToUser(request.Username);
                LobbySessionManager.Add(request.Username, callback);

                var guestProfile = BuildGuestProfile(request.Username);

                _lobbyManager.JoinLobby(request, guestProfile, callback);

                return OperationResult.Ok();
            }
            catch (RepositoryValidationException ex)
            {
                return OperationResult.Fail(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "JoinLobbyGuest");
            }
        }

        private static bool IsGuestUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            if (!username.StartsWith("Guest-", StringComparison.OrdinalIgnoreCase))
                return false;

            string tail = username.Substring("Guest-".Length);
            return tail.Length == 4 && int.TryParse(tail, out _);
        }

        private static PublicProfile BuildGuestProfile(string username)
        {
            return new PublicProfile
            {
                IdUser = 0,
                Username = username,
                AvatarFile = "avatarIcon.png",
                Name = string.Empty,
                LastName = string.Empty,
                Email = string.Empty,
                SocialUrl = string.Empty,
                MatchesPlayed = 0,
                Wins = 0,
                Loses = 0
            };
        }


        private OperationResult ExecuteOperation(Action action, string context)
        {
            try
            {
                action();
                return OperationResult.Ok();
            }
            catch (RepositoryValidationException ex)
            {
                return OperationResult.Fail(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _log.Error($"[{context}] Error: {ex.Message}", ex);
                return OperationResult.Fail(ex.Message, MessageCode.UnknownError);
            }
        }

        private OperationResult HandleException(Exception ex, string context)
        {
            if (ex is RepositoryValidationException valEx)
            {
                return OperationResult.Fail(valEx.Message, valEx.Code);
            }

            _log.Error($"[{context}] Critical Error: {ex.Message}", ex);
            return OperationResult.Fail(ex.Message, MessageCode.UnknownError);
        }
    }
}
