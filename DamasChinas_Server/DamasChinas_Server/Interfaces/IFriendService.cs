using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

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
        [FaultContract(typeof(MessageCode))]
        bool SendFriendRequest(string senderUsername, string receiverUsername);

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