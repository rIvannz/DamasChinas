using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace DamasChinas_Server
{
    public class FriendService : IFriendService
    {
        private readonly FriendRepository _repo = new FriendRepository();


        public List<FriendDto> GetFriends(string username)
        {
            try
            {
                return _repo.GetFriends(username);
            }
            catch (RepositoryValidationException ex)
            {
                throw new FaultException<MessageCode>(ex.Code, ex.Code.ToString());
            }
            catch (Exception)
            {
                throw new FaultException<MessageCode>(MessageCode.UnknownError, "UnknownError");
            }
        }


        public List<FriendDto> GetFriendRequests(string username)
        {
            try
            {
                return _repo.GetFriendRequests(username);
            }
            catch (RepositoryValidationException ex)
            {
                throw new FaultException<MessageCode>(ex.Code, ex.Code.ToString());
            }
            catch (Exception)
            {
                throw new FaultException<MessageCode>(MessageCode.UnknownError, "UnknownError");
            }
        }


        public OperationResult SendFriendRequest(string senderUsername, string receiverUsername)
        {
            try
            {
                bool success = _repo.SendFriendRequest(senderUsername, receiverUsername);

                return success
                    ? OperationResult.Ok()
                    : OperationResult.Fail("Unknown fail.", MessageCode.UnknownError);
            }
            catch (RepositoryValidationException ex)
            {
                return OperationResult.Fail(
                    ex.Code.ToString(),
                    ex.Code
                );
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(
                    ex.Message,
                    MessageCode.UnknownError
                );
            }
        }



        public bool DeleteFriend(string username, string friendUsername)
        {
            try
            {
                return _repo.DeleteFriend(username, friendUsername);
            }
            catch (RepositoryValidationException ex)
            {
                throw new FaultException<MessageCode>(ex.Code, ex.Code.ToString());
            }
            catch (Exception)
            {
                throw new FaultException<MessageCode>(MessageCode.UnknownError, "UnknownError");
            }
        }


        public bool UpdateBlockStatus(string blockerUsername, string blockedUsername, bool block)
        {
            try
            {
                return _repo.UpdateBlockStatus(blockerUsername, blockedUsername, block);
            }
            catch (RepositoryValidationException ex)
            {
                throw new FaultException<MessageCode>(ex.Code, ex.Code.ToString());
            }
            catch (Exception)
            {
                throw new FaultException<MessageCode>(MessageCode.UnknownError, "UnknownError");
            }
        }

        public bool UpdateFriendRequestStatus(string receiverUsername, string senderUsername, bool accept)
        {
            try
            {
                return _repo.UpdateFriendRequestStatus(receiverUsername, senderUsername, accept);
            }
            catch (RepositoryValidationException ex)
            {
                throw new FaultException<MessageCode>(ex.Code, ex.Code.ToString());
            }
            catch (Exception)
            {
                throw new FaultException<MessageCode>(MessageCode.UnknownError, "UnknownError");
            }
        }

        public bool DeleteFriendAndBlock(string blockerUsername, string blockedUsername)
        {
            try
            {
                return _repo.DeleteFriendAndBlock(blockerUsername, blockedUsername);
            }
            catch (RepositoryValidationException ex)
            {
                throw new FaultException<MessageCode>(ex.Code, ex.Code.ToString());
            }
            catch (Exception)
            {
                throw new FaultException<MessageCode>(MessageCode.UnknownError, "UnknownError");
            }
        }
    }
}
