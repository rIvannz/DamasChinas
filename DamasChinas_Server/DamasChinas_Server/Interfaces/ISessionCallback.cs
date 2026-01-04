using System.ServiceModel;
using DamasChinas_Shared.Contracts.Dtos;

namespace DamasChinas_Server.Interfaces
{
    [ServiceContract]
    public interface ISessionCallback
    {
        [OperationContract(IsOneWay = true)]
        void SessionExpired();

        [OperationContract(IsOneWay = true)]
        void PlayerDisconnected(string nickname);

        [OperationContract(IsOneWay = true)]
        void PlayerConnected(string nickname);

        [OperationContract(IsOneWay = true)]
        void PlayerInGame(string username);

        [OperationContract(IsOneWay = true)]
        void PlayerLeftGame(string username);

        [OperationContract(IsOneWay = true)]
        void OnBanStatusUpdated(BanInfoDto banInfo);

        [OperationContract(IsOneWay = true)]
        void OnForcedLogout(string code);

    }
}
