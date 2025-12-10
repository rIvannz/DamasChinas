using System;
using System.Collections.Concurrent;
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

        // ============================================================
        // ADD CALLBACK
        // ============================================================
        public static void Add(string username, IFriendCallback callback)
        {
            string key = Normalize(username);
            if (string.IsNullOrWhiteSpace(key) || callback == null)
                return;

            try
            {
                ActiveFriendCallbacks[key] = callback;
                _log.Info($"[Add] Callback agregado: {key}");
            }
            catch (Exception ex)
            {
                _log.Error($"[Add] Error al agregar callback: {key}", ex);
            }
        }

        // ============================================================
        // REMOVE
        // ============================================================
        public static void Remove(string username)
        {
            string key = Normalize(username);
            if (string.IsNullOrWhiteSpace(key))
                return;

            try
            {
                ActiveFriendCallbacks.TryRemove(key, out _);
                _log.Info($"[Remove] Callback removido: {key}");
            }
            catch (Exception ex)
            {
                _log.Error($"[Remove] Error al remover callback: {key}", ex);
            }
        }

        // ============================================================
        // GET
        // ============================================================
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
            catch (Exception ex)
            {
                _log.Error($"[Get] Error obteniendo callback: {key}", ex);
                return null;
            }
        }

        // ============================================================
        // NOTIFICATIONS
        // ============================================================
        public static void NotifyFriendRemoved(string targetUsername, string removedFriendUsername)
        {
            string key = Normalize(targetUsername);
            if (string.IsNullOrWhiteSpace(key)) return;

            try
            {
                if (ActiveFriendCallbacks.TryGetValue(key, out var callback))
                    callback.FriendRemoved(removedFriendUsername);
            }
            catch (Exception ex)
            {
                _log.Error($"[NotifyFriendRemoved] Error notificando a {key}", ex);
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
            catch (Exception ex)
            {
                _log.Error($"[NotifyUserBlocked] Error notificando bloqueo a {key}", ex);
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
            catch (Exception ex)
            {
                _log.Error($"[NotifyFriendRequestReceived] Error notificando a {key}", ex);
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

        // ========================== FIX DEFINITIVO ==========================
        public static void NotifyFriendListUpdated(string username)
        {
            string key = Normalize(username);
            if (string.IsNullOrWhiteSpace(key)) return;

            try
            {
                if (!ActiveFriendCallbacks.TryGetValue(key, out var callback))
                {
                    _log.Warn($"[NotifyFriendListUpdated] No existe callback para {key}");
                    return;
                }

                callback.FriendListUpdated();
                _log.Info($"[NotifyFriendListUpdated] Enviado a: {key}");
            }
            catch (Exception ex)
            {
                _log.Error($"[NotifyFriendListUpdated] Error notificando a {key}", ex);
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
            catch (Exception ex)
            {
                _log.Error($"[NotifyUserUnblocked] Error notificando desbloqueo a {key}", ex);
                ActiveFriendCallbacks.TryRemove(key, out _);
            }
        }
    }
}
