using System;
using System.Collections.Concurrent;
using DamasChinas_Server.Common;
using DamasChinas_Server.Interfaces;

namespace DamasChinas_Server.Services
{
    public static class SessionManager
    {
        private static readonly ConcurrentDictionary<string, ISessionCallback> ActiveSessions =
            new ConcurrentDictionary<string, ISessionCallback>();

        private static readonly ILogService _log = LogFactory.Create(typeof(SessionManager));

    

        private const string OperationAddSession = nameof(AddSession);
        private const string OperationRemoveSession = nameof(RemoveSession);
        private const string OperationGetSession = nameof(GetSession);
        private const string OperationIsOnline = nameof(IsOnline);
        private const string OperationUpdateSessionUsername = nameof(UpdateSessionUsername);

      

        public static void AddSession(string username, ISessionCallback callback)
        {
            ExecuteOperation(() =>
            {
                ActiveSessions[username] = callback;
                _log.Info($"[{OperationAddSession}] Sesión agregada: {username}");
            }, OperationAddSession);
        }

        public static void RemoveSession(string nickname)
        {
            ExecuteOperation(() =>
            {
                ActiveSessions.TryRemove(nickname, out _);
                _log.Info($"[{OperationRemoveSession}] Sesión removida: {nickname}");
            }, OperationRemoveSession);
        }

        public static ISessionCallback GetSession(string nickname)
        {
            return ExecuteOperation(
                () =>
                {
                    ActiveSessions.TryGetValue(nickname, out var callback);
                    _log.Info($"[{OperationGetSession}] Obtener sesión de: {nickname}");
                    return callback;
                },
                OperationGetSession,
                default(ISessionCallback)
            );
        }

        public static bool IsOnline(string nickname)
        {
            return ExecuteOperation(
                () =>
                {
                    bool online = ActiveSessions.ContainsKey(nickname);
         
                    return online;
                },
                OperationIsOnline,
                false
            );
        }

        public static void UpdateSessionUsername(string currentUsername, string newUsername)
        {
            ExecuteOperation(() =>
            {
                if (string.IsNullOrWhiteSpace(currentUsername) ||
                    string.IsNullOrWhiteSpace(newUsername) ||
                    currentUsername.Equals(newUsername))
                {
                    return;
                }

                if (ActiveSessions.TryRemove(currentUsername, out var callback))
                {
                    ActiveSessions[newUsername] = callback;
                    _log.Info($"[{OperationUpdateSessionUsername}] {currentUsername} → {newUsername}");
                }
            }, OperationUpdateSessionUsername);
        }

     
        private static void ExecuteOperation(Action action, string context)
        {
            try
            {
                _log.Info($"[{context}] START");
                action();
                _log.Info($"[{context}] SUCCESS");
            }
            catch (Exception ex)
            {
                _log.Error($"[{context}] Unexpected exception: {ex.Message}", ex);
            }
        }

        private static T ExecuteOperation<T>(Func<T> action, string context, T defaultValue)
        {
            try
            {
                _log.Info($"[{context}] START");
                var result = action();
                _log.Info($"[{context}] SUCCESS");
                return result;
            }
            catch (Exception ex)
            {
                _log.Error($"[{context}] Unexpected exception: {ex.Message}", ex);
                return defaultValue;
            }
        }
    }
}
