using System;
using System.ServiceModel;
using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Logic;

namespace DamasChinas_Server.Services
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class MatchService : IMatchService
    {
        private readonly MatchManager _manager;
        private readonly ILogService _log;

        // Contexto de esta sesión
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

                // Guardamos contexto de esta sesión
                _lobbyCode = lobbyCode;
                _username = username;
                _hasLeft = false;

                // Suscribimos a cierre / fallo del canal
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
            catch (Exception ex)
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

        public OperationResult MovePiece(MoveRequestDto move)
        {
            try
            {
                _manager.ApplyMove(move);
                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                _log.Warn($"Invalid move by {move.Username}: {ex.Message}");
                return OperationResult.Fail(ex.Message, MessageCode.InvalidMove);
            }
        }

        public void LeaveMatch(int lobbyCode, string username)
        {
            try
            {
                _hasLeft = true; // salida voluntaria, para no duplicar en Closed/Faulted
                _manager.RemovePlayer(lobbyCode, username);
            }
            catch (Exception ex)
            {
                _log.Error($"LeaveMatch error: {ex.Message}", ex);
            }
        }

        // =========================================================
        //  DETECCIÓN DE CIERRE / FALLO DEL CANAL WCF
        // =========================================================
        private void OnChannelClosedOrFaulted(object sender, EventArgs e)
        {
            try
            {
                if (_hasLeft)
                {
                    // Ya procesamos la salida por LeaveMatch
                    return;
                }

                if (_lobbyCode <= 0 || string.IsNullOrWhiteSpace(_username))
                {
                    return;
                }

                _hasLeft = true;
                _log.Warn($"[MatchService] Channel closed/faulted for user={_username}, lobby={_lobbyCode}");

                _manager.HandlePlayerDisconnect(_lobbyCode, _username);
            }
            catch (Exception ex)
            {
                _log.Error($"[MatchService.OnChannelClosedOrFaulted] {ex.Message}", ex);
            }
        }
    }
}
