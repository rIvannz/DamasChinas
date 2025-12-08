using System;
using System.Collections.Generic;
using System.ServiceModel;
using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Logic;

namespace DamasChinas_Server.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class LobbyService : ILobbyService
    {
        private readonly RepositoryUsers _userRepository;
        private readonly LobbyManager _lobbyManager;
        private readonly ILogService _log;

        public LobbyService() : this(new RepositoryUsers(), LobbyManager.Instance, LogFactory.Create<LobbyService>()) { }

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

                // IMPORTANT: Register callback BEFORE logic
                LobbySessionManager.Add(hostUsername, callback);

                _lobbyManager.CreateLobby(hostUsername, profile, request, callback);
                return OperationResult.Ok();
            }
            catch (Exception ex) { return HandleException(ex, "CreateLobby"); }
        }

        public OperationResult JoinLobby(JoinLobbyRequest request)
        {
            try
            {
                var profile = GetProfile(request.Username);
                var callback = GetLobbyCallback();

                // IMPORTANT: Register callback BEFORE logic to enable updates
                LobbySessionManager.Add(request.Username, callback);

                _lobbyManager.JoinLobby(request, profile, callback);
                return OperationResult.Ok();
            }
            catch (Exception ex) { return HandleException(ex, "JoinLobby"); }
        }

        public OperationResult LeaveLobby(string username)
        {
            try
            {
                _lobbyManager.LeaveLobby(username);
                LobbySessionManager.Remove(username);
                return OperationResult.Ok();
            }
            catch (Exception ex) { return HandleException(ex, "LeaveLobby"); }
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

        public OperationResult InviteFriend(string hostUsername, string friendUsername, int lobbyCode)
        {
            return ExecuteOperation(() =>
                _lobbyManager.InviteFriend(hostUsername, friendUsername, lobbyCode, ResolveCallbackForUser),
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

        // --- Helpers ---

        private PublicProfile GetProfile(string username)
        {
            int id = _userRepository.GetUserIdByUsername(username);
            return _userRepository.GetPublicProfile(id);
        }

        private static ILobbyCallback GetLobbyCallback()
        {
            return OperationContext.Current.GetCallbackChannel<ILobbyCallback>();
        }

        private ILobbyCallback ResolveCallbackForUser(string username)
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
                return OperationResult.Fail(valEx.Message, valEx.Code);

            _log.Error($"[{context}] Critical Error: {ex.Message}", ex);
            return OperationResult.Fail(ex.Message, MessageCode.UnknownError);
        }
    }
}