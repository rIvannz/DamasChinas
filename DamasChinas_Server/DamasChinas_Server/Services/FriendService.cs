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
        
                throw new FaultException<MessageCode>(MessageCode.UnknownError, MessageCode.UnknownError.ToString());
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
                throw new FaultException<MessageCode>(MessageCode.UnknownError, MessageCode.UnknownError.ToString());
            }
        }


        public OperationResult SendFriendRequest(string senderUsername, string receiverUsername)
        {
            try
            {
                bool success = _repo.SendFriendRequest(senderUsername, receiverUsername);

                return success
                    ? OperationResult.Ok()
                    : OperationResult.Fail("Friend request failed.", MessageCode.UnknownError);
            }
            catch (RepositoryValidationException ex)
            {
            
                return OperationResult.Fail(ex.Code.ToString(), ex.Code);
            }
            catch (Exception ex)
            {
         
                return OperationResult.Fail(ex.Message, MessageCode.UnknownError);
            }
        }


        public OperationResult DeleteFriend(string username, string friendUsername)
        {
            try
            {
                bool success = _repo.DeleteFriend(username, friendUsername);

                return success
                    ? OperationResult.Ok()
                    : OperationResult.Fail("DeleteFriend returned false.", MessageCode.UnknownError);
            }
            catch (RepositoryValidationException ex)
            {
                return OperationResult.Fail(ex.Code.ToString(), ex.Code);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(ex.Message, MessageCode.UnknownError);
            }
        }

        public OperationResult UpdateBlockStatus(string blockerUsername, string blockedUsername, bool block)
        {
            try
            {
                bool success = _repo.UpdateBlockStatus(blockerUsername, blockedUsername, block);

                return success
                    ? OperationResult.Ok()
                    : OperationResult.Fail("UpdateBlockStatus returned false.", MessageCode.UnknownError);
            }
            catch (RepositoryValidationException ex)
            {
                return OperationResult.Fail(ex.Code.ToString(), ex.Code);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(ex.Message, MessageCode.UnknownError);
            }
        }


        public OperationResult UpdateFriendRequestStatus(string receiverUsername, string senderUsername, bool accept)
        {
            try
            {
                bool success = _repo.UpdateFriendRequestStatus(receiverUsername, senderUsername, accept);

                return success
                    ? OperationResult.Ok()
                    : OperationResult.Fail("UpdateFriendRequestStatus returned false.", MessageCode.UnknownError);
            }
            catch (RepositoryValidationException ex)
            {
                return OperationResult.Fail(ex.Code.ToString(), ex.Code);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(ex.Message, MessageCode.UnknownError);
            }
        }


        public OperationResult DeleteFriendAndBlock(string blockerUsername, string blockedUsername)
        {
            try
            {
                bool success = _repo.DeleteFriendAndBlock(blockerUsername, blockedUsername);

                return success
                    ? OperationResult.Ok()
                    : OperationResult.Fail("DeleteFriendAndBlock returned false.", MessageCode.UnknownError);
            }
            catch (RepositoryValidationException ex)
            {
                return OperationResult.Fail(ex.Code.ToString(), ex.Code);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(ex.Message, MessageCode.UnknownError);
            }
        }
    }
}
