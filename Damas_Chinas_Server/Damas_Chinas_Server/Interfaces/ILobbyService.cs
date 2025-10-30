using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Damas_Chinas_Server.Dtos;

namespace Damas_Chinas_Server.Interfaces
{
    public interface ILobbyCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnMemberJoined(LobbyMember member);

        [OperationContract(IsOneWay = true)]
        void OnMemberLeft(int userId);

        [OperationContract(IsOneWay = true)]
        void OnMessageReceived(int userId, string username, string message, string utcIso);

        [OperationContract(IsOneWay = true)]
        void OnLobbyClosed(string reason);

        [OperationContract(IsOneWay = true)]
        void OnGameStarted(string code);
    }

    [ServiceContract(
        SessionMode = SessionMode.Required,
        CallbackContract = typeof(ILobbyCallback))]
    public interface ILobbyService
    {
        [OperationContract]
        Lobby CreateLobby(int hostUserId, string hostUsername, bool isPrivate);

        [OperationContract]
        Lobby JoinLobby(string code, int userId, string username);

        [OperationContract]
        bool LeaveLobby(string code, int userId);

        [OperationContract(IsOneWay = true)]
        void SendLobbyMessage(string code, int userId, string username, string message);

        [OperationContract]
        Lobby GetLobby(string code);

        [OperationContract]
        bool KickMember(string code, int targetUserId);

        [OperationContract]
        bool StartGame(string code); // Notifica vía callback
    }
}

