using DamasChinas_Server.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DamasChinas_Server
{
    [ServiceContract]
    public interface IFriendService
    {
        [OperationContract]
        [FaultContract(typeof(string))]
        List<FriendDto> GetFriends(string username);

        [OperationContract]
        [FaultContract(typeof(string))]
        List<FriendDto> GetFriendRequests(string username);

        [OperationContract]
        [FaultContract(typeof(string))]
        bool SendFriendRequest(string senderUsername, string receiverUsername);

        [OperationContract]
        [FaultContract(typeof(string))]
        bool DeleteFriend(string username, string friendUsername);

        [OperationContract]
        [FaultContract(typeof(string))]
        bool UpdateBlockStatus(string blockerUsername, string blockedUsername, bool block);

        [OperationContract]
        [FaultContract(typeof(string))]
        bool UpdateFriendRequestStatus(string receiverUsername, string senderUsername, bool accept);

        [OperationContract]
        [FaultContract(typeof(string))]
        bool DeleteFriendAndBlock(string blockerUsername, string blockedUsername);
    }
}