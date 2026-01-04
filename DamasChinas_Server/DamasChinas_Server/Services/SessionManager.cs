using DamasChinas_Server.Common;
using DamasChinas_Server.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.ServiceModel;

namespace DamasChinas_Server.Services
{
    public static class SessionManager
    {
        private sealed class SessionEntry
        {
            public ISessionCallback Callback { get; }
            public ICommunicationObject Channel { get; }

            public SessionEntry(ISessionCallback callback, ICommunicationObject channel)
            {
                Callback = callback;
                Channel = channel;
            }
        }

        private static readonly ConcurrentDictionary<string, SessionEntry> ActiveSessions =
            new ConcurrentDictionary<string, SessionEntry>();


        private static readonly ILogService _log = LogFactory.Create(typeof(SessionManager));

        private const string OperationAddSession = nameof(AddSession);
        private const string OperationRemoveSession = nameof(RemoveSession);
        private const string OperationGetSession = nameof(GetSession);
        private const string OperationIsOnline = nameof(IsOnline);
        private const string OperationUpdateSessionUsername = nameof(UpdateSessionUsername);
        private const string OperationForEachSession = nameof(ForEachSession);



        public static void AddSession(string username, ISessionCallback callback)
        {
            ExecuteOperation(() =>
            {
                if (string.IsNullOrWhiteSpace(username) || callback == null)
                {
                    return;
                }

                var channel = OperationContext.Current?.Channel;
                if (channel == null)
                {
                    return;
                }

                ActiveSessions[username] = new SessionEntry(callback, channel);

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
                    if (string.IsNullOrWhiteSpace(nickname))
                    {
                        return null;
                    }

                    ActiveSessions.TryGetValue(nickname, out var entry);
                    _log.Info($"[{OperationGetSession}] Obtener sesión de: {nickname}");
                    return entry?.Callback;
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

        public static void ForEachSession(Action<ISessionCallback> action)
        {
            if (action == null)
            {
                return;
            }

            ExecuteOperation(() =>
            {
                foreach (var entry in ActiveSessions.ToArray())
                {
                    try
                    {
                        action(entry.Value.Callback);
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"[{OperationForEachSession}] Callback falló, limpiando sesión zombi.", ex);
                        ActiveSessions.TryRemove(entry.Key, out _);
                    }
                }
            }, OperationForEachSession);
        }


        public static void UpdateSessionUsername(string currentUsername, string newUsername)
        {
            ExecuteOperation(() =>
            {
                if (string.IsNullOrWhiteSpace(currentUsername) ||
                    string.IsNullOrWhiteSpace(newUsername) ||
                    currentUsername.Equals(newUsername, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (ActiveSessions.TryRemove(currentUsername, out var entry))
                {
                    ActiveSessions[newUsername] = entry;
                    _log.Info($"[{OperationUpdateSessionUsername}] {currentUsername} to {newUsername}");
                }
            }, OperationUpdateSessionUsername);
        }



        public static void ForEachSession(Action<string, ISessionCallback> action)
        {
            if (action == null)
            {
                return;
            }

            ExecuteOperation(() =>
            {
                foreach (var kvp in ActiveSessions.ToArray())
                {
                    try
                    {
                        action(kvp.Key, kvp.Value.Callback);
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"[{OperationForEachSession}] Callback falló, limpiando sesión zombi.", ex);
                        ActiveSessions.TryRemove(kvp.Key, out _);
                    }
                }
            }, OperationForEachSession);
        }



        private static void ExecuteOperation(Action action, string context)
        {
            try
            {
                _log.Info($"[{context}] START");

                action();

                _log.Info($"[{context}] SUCCESS");
            }
            catch (SqlException ex)
            {
                _log.Error($"[{context}] SQL ERROR {ex.Number}");
            }
            catch (EntityException ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    _log.Error($"[{context}] SQL ERROR {sqlEx.Number}", sqlEx);
                }
                else
                {
                    _log.Error($"[{context}] ENTITY ERROR: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"[{context}] Unexpected exception: {ex.Message}");
            }
        }

        public static void ForceDisconnectAll(MessageCode code)
        {
            ExecuteOperation(() =>
            {
                string key = ToClientKey(code);

                foreach (var kvp in ActiveSessions.ToArray())
                {
                    string username = kvp.Key;
                    var entry = kvp.Value;

                    try
                    {
                        entry.Callback?.OnForcedLogout(key);
                    }
                    catch
                    {
                        
                    }

                    try
                    {
                        entry.Channel?.Abort();
                    }
                    catch
                    {
                       
                    }

                    ActiveSessions.TryRemove(username, out _);
                }

                _log.Error($"[SessionManager.ForceDisconnectAll] Expulsión global ejecutada. Code={code}");

            }, nameof(ForceDisconnectAll));
        }

        private static string ToClientKey(MessageCode code)
        {
            switch (code)
            {
                case MessageCode.DatabaseUnavailable:
                    // usa tu key real del cliente
                    return "msg_DatabaseUnavailable";

                case MessageCode.ServerUnavailable:
                    return "msg_ServerUnavailable";

                default:
                    return "msg_UnknownError";
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
            catch (SqlException ex)
            {
                _log.Error($"[{context}] SQL ERROR {ex.Number}");
                return defaultValue;
            }
            catch (EntityException ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    _log.Error($"[{context}] SQL ERROR {sqlEx.Number}", sqlEx);
                    return defaultValue;
                }

                _log.Error($"[{context}] ENTITY ERROR: {ex.Message}");
                return defaultValue;
            }
            catch (Exception ex)
            {
                _log.Error($"[{context}] Unexpected exception: {ex.Message}");
                return defaultValue;
            }
        }
    }
}