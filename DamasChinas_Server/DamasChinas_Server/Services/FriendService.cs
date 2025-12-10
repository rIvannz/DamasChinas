using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Services;
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
        private const string OperationSubscribeFriendEvents = nameof(SubscribeFriendEvents);
        private const string OperationUnsubscribeFriendEvents = nameof(UnsubscribeFriendEvents);

        public FriendService()
            : this(new FriendRepository(), LogFactory.Create<FriendService>())
        {
        }

        internal FriendService(FriendRepository repo, ILogService log)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        // =========================================================
        //  SUBSCRIPCIÓN A EVENTOS DE AMIGOS
        // =========================================================
        public void SubscribeFriendEvents(string username)
        {
            try
            {
                _log.Info($"[{OperationSubscribeFriendEvents}] START ({username})");

                var callback = OperationContext.Current.GetCallbackChannel<IFriendCallback>();
                FriendCallbackManager.Add(username, callback);

                _log.Info($"[{OperationSubscribeFriendEvents}] SUCCESS ({username})");
            }
            catch (Exception ex)
            {
                _log.Error($"[{OperationSubscribeFriendEvents}] Unexpected exception: {ex.Message}", ex);
            }
        }

        public void UnsubscribeFriendEvents(string username)
        {
            try
            {
                _log.Info($"[{OperationUnsubscribeFriendEvents}] START ({username})");

                FriendCallbackManager.Remove(username);

                _log.Info($"[{OperationUnsubscribeFriendEvents}] SUCCESS ({username})");
            }
            catch (Exception ex)
            {
                _log.Error($"[{OperationUnsubscribeFriendEvents}] Unexpected exception: {ex.Message}", ex);
            }
        }

        // =========================================================
        //  MÉTODOS EXISTENTES
        // =========================================================

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

        public PublicFriendProfile GetFriendPublicProfile(string friendUsername)
        {
            return ExecuteOperation(
                () => _repo.GetFriendPublicProfile(friendUsername),
                "GetFriendPublicProfile",
                faultOnValidation: true
            );
        }

        public OperationResult SendFriendRequest(string senderUsername, string receiverUsername)
        {
            return ExecuteOperation(
                () =>
                {
                    bool ok = _repo.SendFriendRequest(senderUsername, receiverUsername);

                    if (ok)
                    {
                        FriendCallbackManager.NotifyFriendRequestReceived(receiverUsername, senderUsername);

                        return OperationResult.Ok();
                    }

                    return OperationResult.Fail("Friend request failed.", MessageCode.UnknownError);
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
                    if (ok)
                    {
                        FriendCallbackManager.NotifyFriendRemoved(username, friendUsername);
                        FriendCallbackManager.NotifyFriendRemoved(friendUsername, username);
                        return OperationResult.Ok();
                    }

                    return OperationResult.Fail("DeleteFriend returned false.", MessageCode.UnknownError);
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
                    if (ok)
                    {
                        if (block)
                        {
                            FriendCallbackManager.NotifyFriendRemoved(blockerUsername, blockedUsername);
                            FriendCallbackManager.NotifyFriendRemoved(blockedUsername, blockerUsername);
                            FriendCallbackManager.NotifyUserBlocked(blockedUsername, blockerUsername);
                        }
                        else
                        {
                            FriendCallbackManager.NotifyUserUnblocked(blockedUsername, blockerUsername);
                        }

                        return OperationResult.Ok();
                    }

                    return OperationResult.Fail("UpdateBlockStatus returned false.", MessageCode.UnknownError);
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

                    if (ok && accept)
                    {
                      

                      
                        FriendCallbackManager.NotifyFriendRequestAccepted(senderUsername);

                  
                        FriendCallbackManager.NotifyFriendListUpdated(receiverUsername);
                        FriendCallbackManager.NotifyFriendListUpdated(senderUsername);
                    }

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
                    if (ok)
                    {
                        FriendCallbackManager.NotifyFriendRemoved(blockerUsername, blockedUsername);
                        FriendCallbackManager.NotifyFriendRemoved(blockedUsername, blockerUsername);
                        FriendCallbackManager.NotifyUserBlocked(blockedUsername, blockerUsername);

                        return OperationResult.Ok();
                    }

                    return OperationResult.Fail("DeleteFriendAndBlock returned false.", MessageCode.UnknownError);
                },
                OperationDeleteFriendAndBlock
            );
        }

        // =========================================================
        //  EJECUTOR GENÉRICO
        // =========================================================
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
                _log.Warn($"[{context}] validation failed: {ex.Code}");

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
