using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using System.ServiceModel;

namespace DamasChinas_Server.Interfaces
{
    [ServiceContract(CallbackContract = typeof(IMatchCallback), SessionMode = SessionMode.Required)]
    public interface IMatchService
    {
        /// <summary>
        /// Conecta al jugador a la partida activa.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        OperationResult ConnectToMatch(int lobbyCode, string username);

        /// <summary>
        /// Intenta realizar un movimiento.
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        OperationResult MovePiece(MoveRequestDto move);

        /// <summary>
        /// Abandona la partida (Rendirse).
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        void LeaveMatch(int lobbyCode, string username);

        // Dentro de la interfaz IMatchService
        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        MatchStateDto GetMatchState(int lobbyCode);
    }
}