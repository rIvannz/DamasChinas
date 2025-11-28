using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using System.Collections.Generic;
using System.ServiceModel;

namespace DamasChinas_Server.Interfaces
{
    [ServiceContract]
    public interface IFriendService
    {

        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        List<FriendDto> GetFriends(string username);


        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        List<FriendDto> GetFriendRequests(string username);


        [OperationContract]
        OperationResult SendFriendRequest(string senderUsername, string receiverUsername);



        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        bool DeleteFriend(string username, string friendUsername);


        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        bool UpdateBlockStatus(string blockerUsername, string blockedUsername, bool block);


        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        bool UpdateFriendRequestStatus(string receiverUsername, string senderUsername, bool accept);


        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        bool DeleteFriendAndBlock(string blockerUsername, string blockedUsername);
    }
}
