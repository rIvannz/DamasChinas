using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using System.ServiceModel;

namespace DamasChinas_Server.Interfaces
{
    [ServiceContract(CallbackContract = typeof(IMatchCallback), SessionMode = SessionMode.Required)]
    public interface IMatchService
    {

        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        OperationResult ConnectToMatch(int lobbyCode, string username);

 
        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        OperationResult MovePiece(MoveRequestDto move);


        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        void LeaveMatch(int lobbyCode, string username);


        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        MatchStateDto GetMatchState(int lobbyCode);
    }
}