using System.ServiceModel;
using DamasChinas_Server.Dtos;
using DamasChinas_Shared.Contracts.Dtos;


namespace DamasChinas_Server.Interfaces
{
    [ServiceContract]
    public interface IMatchCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnPlayerMoved(TurnChangeDto turnInfo);

        [OperationContract(IsOneWay = true)]
        void OnMatchEnded(string winnerUsername);

        [OperationContract(IsOneWay = true)]
        void OnPlayerLeftMatch(string username);

        [OperationContract(IsOneWay = true)]
        void OnErrorOccurred(string message);

        [OperationContract(IsOneWay = true)]
        void OnBanStatusUpdated(BanInfoDto banInfo);
    }
}
