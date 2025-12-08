using System;
using System.Collections.Generic;
using System.ServiceModel;
using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Logic;
using DamasChinas_Server.Services;   // SessionManager y LobbySessionManager

namespace DamasChinas_Server.Services
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class LobbyService : ILobbyService
    {
        private readonly RepositoryUsers _userRepository;
        private readonly LobbyManager _lobbyManager;
        private readonly ILogService _log;

        private const string ContextCreateLobby = nameof(CreateLobby);
        private const string ContextJoinLobby = nameof(JoinLobby);
        private const string ContextLeaveLobby = nameof(LeaveLobby);
        private const string ContextStartGame = nameof(StartGame);
        private const string ContextKickPlayer = nameof(KickPlayer);
        private const string ContextReportPlayer = nameof(ReportPlayer);
        private const string ContextInviteFriend = nameof(InviteFriend);
        private const string ContextGetPublicLobbies = nameof(GetPublicLobbies);
        private const string ContextGetCurrentLobby = nameof(GetCurrentLobby);
        private const string ContextGetBanInfo = nameof(GetBanInfo);

        public LobbyService()
            : this(new RepositoryUsers(), LobbyManager.Instance, LogFactory.Create<LobbyService>())
        {
        }

        internal LobbyService(
            RepositoryUsers userRepository,
            LobbyManager lobbyManager,
            ILogService log)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _lobbyManager = lobbyManager ?? throw new ArgumentNullException(nameof(lobbyManager));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        // =========================================================
        //  CONSULTAS
        // =========================================================

        public List<LobbySummaryDto> GetPublicLobbies()
        {
            try
            {
                _log.Info($"[{ContextGetPublicLobbies}] Request");
                return _lobbyManager.GetPublicLobbies();
            }
            catch (RepositoryValidationException ex)
            {
                _log.Warn($"[{ContextGetPublicLobbies}] Validation error: {ex.Code}");
                throw new FaultException<MessageCode>(ex.Code);
            }
            catch (Exception ex)
            {
                _log.Error($"[{ContextGetPublicLobbies}] Unexpected exception", ex);
                throw new FaultException<MessageCode>(MessageCode.UnknownError);
            }
        }

        public LobbySnapshotDto GetCurrentLobby(string username)
        {
            try
            {
                _log.Info($"[{ContextGetCurrentLobby}] username={username}");
                return _lobbyManager.GetLobbyForUser(username);
            }
            catch (RepositoryValidationException ex)
            {
                _log.Warn($"[{ContextGetCurrentLobby}] Validation error: {ex.Code}");
                throw new FaultException<MessageCode>(ex.Code);
            }
            catch (Exception ex)
            {
                _log.Error($"[{ContextGetCurrentLobby}] Unexpected exception", ex);
                throw new FaultException<MessageCode>(MessageCode.UnknownError);
            }
        }



        // =========================================================
        //  OPERACIONES
        // =========================================================

        public OperationResult CreateLobby(string hostUsername, CreateLobbyRequest request)
        {
            try
            {
                _log.Info($"[{ContextCreateLobby}] host={hostUsername}, maxPlayers={request?.MaxPlayers}, visibility={request?.Visibility}");

                PublicProfile profile = GetProfile(hostUsername);
                ILobbyCallback callback = GetLobbyCallback();

                // ★★ FIX CRÍTICO: REGISTRO DEL CALLBACK ★★
                LobbySessionManager.Add(hostUsername, callback);

                _lobbyManager.CreateLobby(hostUsername, profile, request, callback);

                return OperationResult.Ok();
            }
            catch (RepositoryValidationException ex)
            {
                _log.Warn($"[{ContextCreateLobby}] Validation error: {ex.Code}");
                return OperationResult.Fail(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _log.Error($"[{ContextCreateLobby}] Unexpected exception", ex);
                return OperationResult.Fail(ex.Message, MessageCode.MatchCreationFailed);
            }
        }


        public OperationResult JoinLobby(JoinLobbyRequest request)
        {
            try
            {
                _log.Info($"[{ContextJoinLobby}] username={request?.Username}, lobby={request?.LobbyCode}");

                PublicProfile profile = GetProfile(request.Username);
                ILobbyCallback callback = GetLobbyCallback();

                // ★★ FIX CRÍTICO: REGISTRO DEL CALLBACK ★★
                LobbySessionManager.Add(request.Username, callback);

                _lobbyManager.JoinLobby(request, profile, callback);

                return OperationResult.Ok();
            }
            catch (RepositoryValidationException ex)
            {
                _log.Warn($"[{ContextJoinLobby}] Validation error: {ex.Code}");
                return OperationResult.Fail(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _log.Error($"[{ContextJoinLobby}] Unexpected exception", ex);
                return OperationResult.Fail(ex.Message, MessageCode.UnknownError);
            }
        }


        public OperationResult LeaveLobby(string username)
        {
            try
            {
                _log.Info($"[{ContextLeaveLobby}] username={username}");

                if (string.IsNullOrWhiteSpace(username))
                    return OperationResult.Fail("Username empty", MessageCode.UserNotFound);

                _lobbyManager.LeaveLobby(username);

               
                LobbySessionManager.Remove(username);

                return OperationResult.Ok();
            }
            catch (RepositoryValidationException ex)
            {
                _log.Warn($"[{ContextLeaveLobby}] Validation error: {ex.Code}");
                return OperationResult.Fail(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                _log.Error($"[{ContextLeaveLobby}] Unexpected exception", ex);
                return OperationResult.Fail(ex.Message, MessageCode.UnknownError);
            }
        }



        public OperationResult StartGame(string hostUsername)
        {
            try
            {
                _log.Info($"[{ContextStartGame}] host={hostUsername}");

                _lobbyManager.StartGame(hostUsername);
                return OperationResult.Ok();
            }
            catch (RepositoryValidationException ex)
            {
                return OperationResult.Fail(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(ex.Message, MessageCode.LobbyStartFailed);
            }
        }

        public OperationResult KickPlayer(string hostUsername, int lobbyCode, string targetUsername)
        {
            try
            {
                _lobbyManager.KickPlayer(hostUsername, lobbyCode, targetUsername);
                return OperationResult.Ok();
            }
            catch (RepositoryValidationException ex)
            {
                return OperationResult.Fail(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(ex.Message, MessageCode.UnknownError);
            }
        }

        public OperationResult ReportPlayer(ReportPlayerRequest request)
        {
            try
            {
                _log.Info($"[ReportPlayer] reporter={request?.ReporterUsername}, reported={request?.ReportedUsername}, lobby={request?.LobbyCode}");

                _lobbyManager.ReportPlayer(request);
                return OperationResult.Ok();
            }
            catch (RepositoryValidationException ex)
            {
                return OperationResult.Fail(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(ex.Message, MessageCode.UnknownError);
            }
        }


        public BanInfoDto GetBanInfo(string username)
        {
            try
            {
                return _lobbyManager.GetBanInfo(username);
            }
            catch (RepositoryValidationException ex)
            {
                throw new FaultException<MessageCode>(ex.Code);
            }
            catch (Exception)
            {
                throw new FaultException<MessageCode>(MessageCode.UnknownError);
            }
        }


        public OperationResult InviteFriend(
            string hostUsername,
            string friendUsername,
            int lobbyCode)
        {
            try
            {
                _lobbyManager.InviteFriend(
                    hostUsername,
                    friendUsername,
                    lobbyCode,
                    ResolveCallbackForUser);

                return OperationResult.Ok();
            }
            catch (RepositoryValidationException ex)
            {
                return OperationResult.Fail(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(ex.Message, MessageCode.LobbyInvitationFailed);
            }
        }



        // =========================================================
        //  HELPERS
        // =========================================================

        private PublicProfile GetProfile(string username)
        {
            int id = _userRepository.GetUserIdByUsername(username);
            return _userRepository.GetPublicProfile(id);
        }

        private static ILobbyCallback GetLobbyCallback()
        {
            var context = OperationContext.Current;
            return context.GetCallbackChannel<ILobbyCallback>();
        }

        private ILobbyCallback ResolveCallbackForUser(string username)
        {
            return LobbySessionManager.Get(username);
        }
    }
}
