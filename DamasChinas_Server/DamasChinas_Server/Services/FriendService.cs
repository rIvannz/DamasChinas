using DamasChinas_Server.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DamasChinas_Server
{
        public class FriendService : IFriendService
        {
                private readonly FriendRepository repo = new FriendRepository();

                public List<FriendDto> GetFriends(string username)
                {
                        return repo.GetFriends(username);
                }

                public List<FriendDto> GetFriendRequests(string username)
                {
                        return repo.GetFriendRequests(username);
                }

                public bool SendFriendRequest(string senderUsername, string receiverUsername)
                {
                        return repo.SendFriendRequest(senderUsername, receiverUsername);
                }

                public bool DeleteFriend(string username, string friendUsername)
                {
                        return repo.DeleteFriend(username, friendUsername);
                }

                public bool UpdateBlockStatus(string blockerUsername, string blockedUsername, bool block)
                {
                        return repo.UpdateBlockStatus(blockerUsername, blockedUsername, block);
                }

                public bool UpdateFriendRequestStatus(string receiverUsername, string senderUsername, bool accept)
                {
                        return repo.UpdateFriendRequestStatus(receiverUsername, senderUsername, accept);
                }

                public bool DeleteFriendAndBlock(string blockerUsername, string blockedUsername)
                { 
                       return repo.DeleteFriendAndBlock(blockerUsername, blockedUsername);
                }
        }
}
