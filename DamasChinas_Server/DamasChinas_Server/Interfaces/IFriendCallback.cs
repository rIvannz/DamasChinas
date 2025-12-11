using System.ServiceModel;

namespace DamasChinas_Server.Interfaces
{
    public interface IFriendCallback
    {
        [OperationContract(IsOneWay = true)]
        void FriendRequestReceived(string fromUsername);

        [OperationContract(IsOneWay = true)]
        void FriendRequestAccepted(string byUsername);

        [OperationContract(IsOneWay = true)]
        void FriendRemoved(string username);

        [OperationContract(IsOneWay = true)]
        void UserBlockedYou(string username);

        [OperationContract(IsOneWay = true)]
        void UserUnblockedYou(string username);

        [OperationContract(IsOneWay = true)]
        void FriendListUpdated();


    }
}
