using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Logic;
using DamasChinas_Server.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.ServiceModel;

namespace DamasChinas_Server
{
    [DbGuardBehavior]
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


        private T ExecuteOperation<T>( Func<T> func, string context, bool faultOnValidation = false)
        {
            T ReturnFail(MessageCode code, string message = null)
            {
                if (faultOnValidation)
                    throw new FaultException<MessageCode>(code, code.ToString());

                return (T)(object)OperationResult.Fail(message ?? code.ToString(), code);
            }

            void LogSqlOrEntity(Exception ex)
            {
                if (ex is SqlException sqlEx)
                    _log.Error($"[{context}] SQL ERROR {sqlEx.Number}", sqlEx);
                else if (ex is EntityException entityEx && entityEx.InnerException is SqlException innerSql)
                    _log.Error($"[{context}] SQL ERROR {innerSql.Number}", innerSql);
            }

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
                return ReturnFail(ex.Code, ex.Code.ToString());
            }
            catch (FaultException<MessageCode>)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogSqlOrEntity(ex);

                if (ex is SqlException || ex is EntityException)
                    return ReturnFail(MessageCode.ServerUnavailable);

                return ReturnFail(MessageCode.UnknownError, ex.Message);
            }
        }
    }
}