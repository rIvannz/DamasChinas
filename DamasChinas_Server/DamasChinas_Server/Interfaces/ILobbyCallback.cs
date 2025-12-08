using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using System.ServiceModel;

namespace DamasChinas_Server.Interfaces
{
    [ServiceContract]
    public interface ILobbyCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnLobbySnapshot(LobbySnapshotDto snapshot);

        [OperationContract(IsOneWay = true)]
        void OnLobbyClosed(MessageCode reason);

        [OperationContract(IsOneWay = true)]
        void OnGameStarting();

        [OperationContract(IsOneWay = true)]
        void OnInvitationReceived(LobbyInvitationDto invitation);

        [OperationContract(IsOneWay = true)]
        void OnKickedFromLobby(MessageCode reason);

        [OperationContract(IsOneWay = true)]
        void OnBanStatusUpdated(BanInfoDto banInfo);

        [OperationContract(IsOneWay = true)]
        void OnChatMessageReceived(string sender, string message, string timestamp);
    }
}