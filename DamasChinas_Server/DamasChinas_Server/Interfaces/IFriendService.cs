using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using System.Collections.Generic;
using System.ServiceModel;

namespace DamasChinas_Server.Interfaces
{
    [ServiceContract(
        CallbackContract = typeof(IFriendCallback),
        SessionMode = SessionMode.Required)]
    public interface IFriendService
    {

        [OperationContract(IsOneWay = true)]
        void SubscribeFriendEvents(string username);

        [OperationContract(IsOneWay = true)]
        void UnsubscribeFriendEvents(string username);

        
        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        List<FriendDto> GetFriends(string username);

        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        List<FriendDto> GetFriendRequests(string username);

        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        PublicFriendProfile GetFriendPublicProfile(string friendUsername);

        [OperationContract]
        OperationResult SendFriendRequest(string senderUsername, string receiverUsername);

        [OperationContract]
        OperationResult DeleteFriend(string username, string friendUsername);

        [OperationContract]
        OperationResult UpdateBlockStatus(string blockerUsername, string blockedUsername, bool block);

        [OperationContract]
        OperationResult UpdateFriendRequestStatus(string receiverUsername, string senderUsername, bool accept);

        [OperationContract]
        OperationResult DeleteFriendAndBlock(string blockerUsername, string blockedUsername);
    }
}
