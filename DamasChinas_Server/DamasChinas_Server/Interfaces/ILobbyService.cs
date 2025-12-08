using System.Collections.Generic;
using System.ServiceModel;
using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;

namespace DamasChinas_Server.Interfaces
{
    [ServiceContract(CallbackContract = typeof(ILobbyCallback), SessionMode = SessionMode.Required)]
    public interface ILobbyService
    {
        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        List<LobbySummaryDto> GetPublicLobbies();

        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        LobbySnapshotDto GetCurrentLobby(string username);

        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        OperationResult CreateLobby(string hostUsername, CreateLobbyRequest request);

        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        OperationResult JoinLobby(JoinLobbyRequest request);

        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        OperationResult LeaveLobby(string username);

        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        OperationResult StartGame(string hostUsername);

        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        OperationResult KickPlayer(string hostUsername, int lobbyCode, string targetUsername);

        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        OperationResult ReportPlayer(ReportPlayerRequest request);

        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        BanInfoDto GetBanInfo(string username);

        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        OperationResult InviteFriend(string hostUsername, string friendUsername, int lobbyCode);

        [OperationContract(IsOneWay = true)]
        void SendLobbyMessage(string sender, int lobbyCode, string message);
    }
}