using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace DamasChinas_Server
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class FriendService : IFriendService
    {
        private readonly FriendRepository _repo;
        private readonly ILogService _log;

    
        private const string OperationGetFriends = nameof(GetFriends);
        private const string OperationGetFriendRequests = nameof(GetFriendRequests);
        private const string OperationSendFriendRequest = nameof(SendFriendRequest);
        private const string OperationDeleteFriend = nameof(DeleteFriend);
        private const string OperationUpdateBlockStatus = nameof(UpdateBlockStatus);
        private const string OperationUpdateFriendRequestStatus = nameof(UpdateFriendRequestStatus);
        private const string OperationDeleteFriendAndBlock = nameof(DeleteFriendAndBlock);


        public FriendService()
            : this(new FriendRepository(), LogFactory.Create<FriendService>())
        {
        }

        internal FriendService(FriendRepository repo, ILogService log)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public List<FriendDto> GetFriends(string username)
        {
            return ExecuteOperation(
                () => _repo.GetFriends(username),
                OperationGetFriends,
                faultOnValidation: true
            );
        }

        public List<FriendDto> GetFriendRequests(string username)
        {
            return ExecuteOperation(
                () => _repo.GetFriendRequests(username),
                OperationGetFriendRequests,
                faultOnValidation: true
            );
        }

        public OperationResult SendFriendRequest(string senderUsername, string receiverUsername)
        {
            return ExecuteOperation(
                () =>
                {
                    bool ok = _repo.SendFriendRequest(senderUsername, receiverUsername);
                    return ok
                        ? OperationResult.Ok()
                        : OperationResult.Fail("Friend request failed.", MessageCode.UnknownError);
                },
                OperationSendFriendRequest
            );
        }

        public OperationResult DeleteFriend(string username, string friendUsername)
        {
            return ExecuteOperation(
                () =>
                {
                    bool ok = _repo.DeleteFriend(username, friendUsername);
                    return ok
                        ? OperationResult.Ok()
                        : OperationResult.Fail("DeleteFriend returned false.", MessageCode.UnknownError);
                },
                OperationDeleteFriend
            );
        }

        public OperationResult UpdateBlockStatus(string blockerUsername, string blockedUsername, bool block)
        {
            return ExecuteOperation(
                () =>
                {
                    bool ok = _repo.UpdateBlockStatus(blockerUsername, blockedUsername, block);
                    return ok
                        ? OperationResult.Ok()
                        : OperationResult.Fail("UpdateBlockStatus returned false.", MessageCode.UnknownError);
                },
                OperationUpdateBlockStatus
            );
        }

        public OperationResult UpdateFriendRequestStatus(string receiverUsername, string senderUsername, bool accept)
        {
            return ExecuteOperation(
                () =>
                {
                    bool ok = _repo.UpdateFriendRequestStatus(receiverUsername, senderUsername, accept);
                    return ok
                        ? OperationResult.Ok()
                        : OperationResult.Fail("UpdateFriendRequestStatus returned false.", MessageCode.UnknownError);
                },
                OperationUpdateFriendRequestStatus
            );
        }

        public OperationResult DeleteFriendAndBlock(string blockerUsername, string blockedUsername)
        {
            return ExecuteOperation(
                () =>
                {
                    bool ok = _repo.DeleteFriendAndBlock(blockerUsername, blockedUsername);
                    return ok
                        ? OperationResult.Ok()
                        : OperationResult.Fail("DeleteFriendAndBlock returned false.", MessageCode.UnknownError);
                },
                OperationDeleteFriendAndBlock
            );
        }


        private T ExecuteOperation<T>(
            Func<T> func,
            string context,
            bool faultOnValidation = false)
        {
            try
            {
                _log.Info($"[{context}] START");
                T result = func();
                _log.Info($"[{context}] SUCCESS");
                return result;
            }
            catch (RepositoryValidationException ex)
            {
                _log.Warn($"[{context}]  validation failed: {ex.Code}");

                if (faultOnValidation)
                {
                    throw new FaultException<MessageCode>(ex.Code, ex.Code.ToString());
                }

                return (T)(object)OperationResult.Fail(ex.Code.ToString(), ex.Code);
            }
            catch (FaultException<MessageCode>)
            {
                throw;
            }
            catch (Exception ex)
            {
                _log.Error($"[{context}] Unexpected exception: {ex.Message}", ex);

                if (faultOnValidation)
                {
                    throw new FaultException<MessageCode>(MessageCode.UnknownError, MessageCode.UnknownError.ToString());
                }

                return (T)(object)OperationResult.Fail(ex.Message, MessageCode.UnknownError);
            }
        }
    }
}
