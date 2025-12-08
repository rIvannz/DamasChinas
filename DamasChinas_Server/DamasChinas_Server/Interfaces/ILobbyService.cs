using System.Collections.Generic;
using System.ServiceModel;
using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;

namespace DamasChinas_Server.Interfaces
{
    [ServiceContract(
        CallbackContract = typeof(ILobbyCallback),
        SessionMode = SessionMode.Required)]
    public interface ILobbyService
    {
        // ===== CONSULTAS =====
        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        List<LobbySummaryDto> GetPublicLobbies();

        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        LobbySnapshotDto GetCurrentLobby(string username);

        // ===== OPERACIONES BÁSICAS =====
        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        OperationResult CreateLobby(string hostUsername, CreateLobbyRequest request);

        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        OperationResult JoinLobby(JoinLobbyRequest request);

        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        OperationResult LeaveLobby(string username);

        // ===== HOST ONLY =====
        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        OperationResult StartGame(string hostUsername);

        //  FALTABA ESTE OperationContract (causaba errores en el cliente)
        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        OperationResult KickPlayer(string hostUsername, int lobbyCode, string targetUsername);

        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        OperationResult ReportPlayer(ReportPlayerRequest request);

        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        BanInfoDto GetBanInfo(string username);

        // ===== INVITACIONES =====
        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        OperationResult InviteFriend(string hostUsername, string friendUsername, int lobbyCode);
    }
}
