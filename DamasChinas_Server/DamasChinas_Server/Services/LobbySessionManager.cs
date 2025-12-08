using System;
using System.Collections.Concurrent;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Common;

namespace DamasChinas_Server.Services
{
    public static class LobbySessionManager
    {
        private static readonly ConcurrentDictionary<string, ILobbyCallback> _activeLobbySessions =
            new ConcurrentDictionary<string, ILobbyCallback>(StringComparer.OrdinalIgnoreCase);

        private static readonly ILogService _log = LogFactory.Create(typeof(LobbySessionManager));

        // =========================================================
        //  REGISTRAR CALLBACK (LOGIN / CREATE / JOIN)
        // =========================================================
        public static void Add(string username, ILobbyCallback callback)
        {
            if (string.IsNullOrWhiteSpace(username) || callback == null)
                return;

            _activeLobbySessions[username] = callback;
            _log.Info($"[LobbySessionManager] Callback agregado: {username}");
        }

        // =========================================================
        //  REMOVER CALLBACK (LEAVE / DISCONNECT)
        // =========================================================
        public static void Remove(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return;

            _activeLobbySessions.TryRemove(username, out _);
            _log.Info($"[LobbySessionManager] Callback removido: {username}");
        }

        // =========================================================
        //  OBTENER CALLBACK PARA SNAPSHOTS / INVITACIONES
        // =========================================================
        public static ILobbyCallback Get(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            _activeLobbySessions.TryGetValue(username, out var callback);
            return callback;
        }

        // =========================================================
        //  CHECK ONLINE
        // =========================================================
        public static bool IsOnline(string username)
        {
            return _activeLobbySessions.ContainsKey(username);
        }
    }
}
