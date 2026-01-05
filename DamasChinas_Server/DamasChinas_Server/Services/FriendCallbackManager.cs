using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using DamasChinas_Server.Common;
using DamasChinas_Server.Interfaces;

namespace DamasChinas_Server.Services
{
    public static class FriendCallbackManager
    {
        private static readonly ConcurrentDictionary<string, IFriendCallback> ActiveFriendCallbacks =
            new ConcurrentDictionary<string, IFriendCallback>();

        private static readonly ILogService _log = LogFactory.Create(typeof(FriendCallbackManager));

        private static string Normalize(string username)
        {
            return username?.Trim().ToLowerInvariant();
        }

        public static void Add(string username, IFriendCallback callback)
        {
            string key = Normalize(username);
            if (string.IsNullOrWhiteSpace(key) || callback == null)
                return;

            try
            {
                ActiveFriendCallbacks[key] = callback;
                _log.Info($"[Add] Callback add: {key}");
            }
            catch (InvalidOperationException ex)
            {
                _log.Error($"[Add] callback error on ad: {key}", ex);
            }
        }

     
        public static void Remove(string username)
        {
            string key = Normalize(username);
            if (string.IsNullOrWhiteSpace(key))
                return;

            try
            {
                ActiveFriendCallbacks.TryRemove(key, out _);
                _log.Info($"[Remove] Callback removed : {key}");
            }
            catch (CommunicationException   ex)
            {
                _log.Error($"[Remove] error on remove callback: {key}", ex);
            }
        }

        public static IFriendCallback Get(string username)
        {
            string key = Normalize(username);
            if (string.IsNullOrWhiteSpace(key))
                return null;

            try
            {
                ActiveFriendCallbacks.TryGetValue(key, out var callback);
                return callback;
            }
            catch (CommunicationException ex)
            {
                _log.Error($"[Get] error on get callback: {key}", ex);
                return null;
            }
        }

    

        public static void NotifyFriendRemoved(string targetUsername, string removedFriendUsername)
        {
            string key = Normalize(targetUsername);
            if (string.IsNullOrWhiteSpace(key)) return;

            try
            {
                if (ActiveFriendCallbacks.TryGetValue(key, out var callback))
                    callback.FriendRemoved(removedFriendUsername);
            }
            catch (CommunicationException ex)
            {
                _log.Error($"[NotifyFriendRemoved] error on notification {key}", ex);
                ActiveFriendCallbacks.TryRemove(key, out _);
            }
        }

        public static void NotifyUserBlocked(string blockedUsername, string blockerUsername)
        {
            string key = Normalize(blockedUsername);
            if (string.IsNullOrWhiteSpace(key)) return;

            try
            {
                if (ActiveFriendCallbacks.TryGetValue(key, out var callback))
                    callback.UserBlockedYou(blockerUsername);
            }
            catch (CommunicationException ex)
            {
                _log.Error($"[NotifyUserBlocked] Error on notify to {key}", ex);
                ActiveFriendCallbacks.TryRemove(key, out _);
            }
        }

        public static void NotifyFriendRequestReceived(string targetUsername, string fromUsername)
        {
            string key = Normalize(targetUsername);
            if (string.IsNullOrWhiteSpace(key)) return;

            try
            {
                if (ActiveFriendCallbacks.TryGetValue(key, out var callback))
                    callback.FriendRequestReceived(fromUsername);
            }
            catch (CommunicationException ex)
            {
                _log.Error($"[NotifyFriendRequestReceived] Error on notify to {key}", ex);
                ActiveFriendCallbacks.TryRemove(key, out _);
            }
        }

        public static void NotifyFriendRequestAccepted(string username)
        {
            string key = Normalize(username);
            if (string.IsNullOrWhiteSpace(key)) return;

            if (ActiveFriendCallbacks.TryGetValue(key, out var callback))
                callback.FriendRequestAccepted(username);
        }

        public static void NotifyFriendListUpdated(string username)
        {
            string key = Normalize(username);
            if (string.IsNullOrWhiteSpace(key)) return;

            try
            {
                if (!ActiveFriendCallbacks.TryGetValue(key, out var callback))
                {
                    _log.Warn($"[NotifyFriendListUpdated] Wasent notify {key}");
                    return;
                }

                callback.FriendListUpdated();
                _log.Info($"[NotifyFriendListUpdated] Send to: {key}");
            }
            catch (CommunicationException ex)
            {
                _log.Error($"[NotifyFriendListUpdated] eroor on notifi to {key}", ex);
                ActiveFriendCallbacks.TryRemove(key, out _);
            }
        }

        public static void NotifyUserUnblocked(string unblockedUsername, string byUsername)
        {
            string key = Normalize(unblockedUsername);
            if (string.IsNullOrWhiteSpace(key)) return;

            try
            {
                if (ActiveFriendCallbacks.TryGetValue(key, out var callback))
                    callback.UserUnblockedYou(byUsername);
            }
            catch (CommunicationException ex)
            {
                _log.Error($"[NotifyUserUnblocked] Error on notify unblock to {key}", ex);
                ActiveFriendCallbacks.TryRemove(key, out _);
            }
        }
    }
}
