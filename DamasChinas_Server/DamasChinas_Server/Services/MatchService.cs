using System;
using System.ServiceModel;
using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Logic;

namespace DamasChinas_Server.Services
{
    [DbGuardBehavior]
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class MatchService : IMatchService
    {
        private readonly MatchManager _manager;
        private readonly ILogService _log;


        private int _lobbyCode;
        private string _username;
        private bool _hasLeft;

        public MatchService()
            : this(MatchManager.Instance, LogFactory.Create<MatchService>())
        {
        }

        internal MatchService(MatchManager manager, ILogService log)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public OperationResult ConnectToMatch(int lobbyCode, string username)
        {
            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<IMatchCallback>();


                _lobbyCode = lobbyCode;
                _username = username;
                _hasLeft = false;


                var channel = OperationContext.Current.Channel;
                channel.Closed += OnChannelClosedOrFaulted;
                channel.Faulted += OnChannelClosedOrFaulted;

                _manager.RegisterPlayerSession(lobbyCode, username, callback);
                return OperationResult.Ok();
            }
            catch (RepositoryValidationException ex)
            {
                return OperationResult.Fail(ex.Message, ex.Code);
            }
            catch (CommunicationException ex)
            {
                _log.Error($"Error connecting to match {lobbyCode}: {ex.Message}", ex);
                return OperationResult.Fail("Connection error.", MessageCode.UnknownError);
            }
        }

        public MatchStateDto GetMatchState(int lobbyCode)
        {
            try
            {
                return _manager.GetMatchState(lobbyCode);
            }
            catch
            {
                throw new FaultException<MessageCode>(MessageCode.UnknownError);
            }
        }

        public OperationResult MovePiece(MoveRequestDto req)
        {
            try
            {
                MatchManager.Instance.ApplyMove(req);
                return OperationResult.Ok();
            }
            catch (RepositoryValidationException ex)
            {
                return OperationResult.Fail(ex.Code.ToString(), ex.Code);
            }
        }

        public void LeaveMatch(int lobbyCode, string username)
        {
            try
            {
                _hasLeft = true;
                _manager.RemovePlayer(lobbyCode, username);
            }
            catch (CommunicationException ex)
            {
                _log.Error($"LeaveMatch error: {ex.Message}", ex);
            }
        }


        private void OnChannelClosedOrFaulted(object sender, EventArgs e)
        {
            try
            {
                if (_hasLeft)
                {
                    return;
                }

                if (_lobbyCode <= 0 || string.IsNullOrWhiteSpace(_username))
                {
                    return;
                }

                _log.Warn($"[MatchService] Channel closed/faulted for user={_username}, lobby={_lobbyCode}");

                _manager.HandlePlayerDisconnect(_lobbyCode, _username);
            }
            catch (CommunicationException ex)
            {
                _log.Error($"[MatchService] {MessageCode.ServerUnavailable} on channel closed/faulted user={_username} lobby={_lobbyCode}", ex);
            }
            catch (TimeoutException ex)
            {
                _log.Error($"[MatchService] {MessageCode.NetworkLatency} on channel closed/faulted user={_username} lobby={_lobbyCode}", ex);
            }
            catch
            {
                _log.Warn($"[MatchService] {MessageCode.UnknownError} on channel closed/faulted user={_username} lobby={_lobbyCode}");
            }
        }


    }
}

