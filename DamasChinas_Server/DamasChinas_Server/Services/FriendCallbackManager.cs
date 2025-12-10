using System;
using System.Collections.Concurrent;
using DamasChinas_Server.Common;
using DamasChinas_Server.Interfaces;

namespace DamasChinas_Server.Services
{
    public static class FriendCallbackManager
    {
        private static readonly ConcurrentDictionary<string, IFriendCallback> ActiveFriendCallbacks =
            new ConcurrentDictionary<string, IFriendCallback>(StringComparer.OrdinalIgnoreCase);

        private static readonly ILogService _log = LogFactory.Create(typeof(FriendCallbackManager));

        private const string OperationAdd = nameof(Add);
        private const string OperationRemove = nameof(Remove);
        private const string OperationGet = nameof(Get);
        private const string OperationNotifyFriendRemoved = nameof(NotifyFriendRemoved);
        private const string OperationNotifyUserBlocked = nameof(NotifyUserBlocked);
        private const string OperationNotifyUserUnblocked = nameof(NotifyUserUnblocked);

        public static void Add(string username, IFriendCallback callback)
        {
            if (string.IsNullOrWhiteSpace(username) || callback == null)
            {
                return;
            }

            try
            {
                ActiveFriendCallbacks[username] = callback;
                _log.Info($"[{OperationAdd}] Callback de amigos agregado: {username}");
            }
            catch (Exception ex)
            {
                _log.Error($"[{OperationAdd}] Error al agregar callback: {username}", ex);
            }
        }

        public static void Remove(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            try
            {
                ActiveFriendCallbacks.TryRemove(username, out _);
                _log.Info($"[{OperationRemove}] Callback de amigos removido: {username}");
            }
            catch (Exception ex)
            {
                _log.Error($"[{OperationRemove}] Error al remover callback: {username}", ex);
            }
        }

        public static IFriendCallback Get(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            try
            {
                ActiveFriendCallbacks.TryGetValue(username, out var callback);
                _log.Info($"[{OperationGet}] Obtener callback de: {username}");
                return callback;
            }
            catch (Exception ex)
            {
                _log.Error($"[{OperationGet}] Error al obtener callback: {username}", ex);
                return null;
            }
        }

        // ========== NOTIFICACIONES ==========

        public static void NotifyFriendRemoved(string targetUsername, string removedFriendUsername)
        {
            if (string.IsNullOrWhiteSpace(targetUsername) ||
                string.IsNullOrWhiteSpace(removedFriendUsername))
            {
                return;
            }

            try
            {
                if (ActiveFriendCallbacks.TryGetValue(targetUsername, out var callback))
                {
                    callback.FriendRemoved(removedFriendUsername);
                    _log.Info($"[{OperationNotifyFriendRemoved}] Notificado a {targetUsername} que se removió a {removedFriendUsername}");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"[{OperationNotifyFriendRemoved}] Error notificando a {targetUsername}", ex);
                ActiveFriendCallbacks.TryRemove(targetUsername, out _);
            }
        }

        public static void NotifyUserBlocked(string blockedUsername, string blockerUsername)
        {
            if (string.IsNullOrWhiteSpace(blockedUsername) ||
                string.IsNullOrWhiteSpace(blockerUsername))
            {
                return;
            }

            try
            {
                if (ActiveFriendCallbacks.TryGetValue(blockedUsername, out var callback))
                {
                    callback.UserBlockedYou(blockerUsername);
                    _log.Info($"[{OperationNotifyUserBlocked}] Notificado a {blockedUsername} que fue bloqueado por {blockerUsername}");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"[{OperationNotifyUserBlocked}] Error notificando bloqueo a {blockedUsername}", ex);
                ActiveFriendCallbacks.TryRemove(blockedUsername, out _);
            }
        }

        public static void NotifyUserUnblocked(string unblockedUsername, string byUsername)
        {
            if (string.IsNullOrWhiteSpace(unblockedUsername) ||
                string.IsNullOrWhiteSpace(byUsername))
            {
                return;
            }

            try
            {
                if (ActiveFriendCallbacks.TryGetValue(unblockedUsername, out var callback))
                {
                    callback.UserUnblockedYou(byUsername);
                    _log.Info($"[{OperationNotifyUserUnblocked}] Notificado a {unblockedUsername} que fue desbloqueado por {byUsername}");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"[{OperationNotifyUserUnblocked}] Error notificando desbloqueo a {unblockedUsername}", ex);
                ActiveFriendCallbacks.TryRemove(unblockedUsername, out _);
            }
        }
    }
}
