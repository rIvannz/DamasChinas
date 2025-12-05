using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using DamasChinas_Server.Common;
using DamasChinas_Server.Interfaces;

namespace DamasChinas_Server.Services
{
    public static class FriendCallbackManager
    {
        private static readonly ConcurrentDictionary<string, IFriendCallback> _callbacks
            = new ConcurrentDictionary<string, IFriendCallback>();

        private static readonly ILogService _log =
            LogFactory.Create(typeof(FriendCallbackManager));

        private const string OperationRegister = nameof(Register);
        private const string OperationUnregister = nameof(Unregister);
        private const string OperationGetCallback = nameof(GetCallback);

        // ============================================================
        // REGISTRAR CALLBACK
        // ============================================================
        public static void Register(string username, IFriendCallback callback)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || callback == null)
                {
                    _log.Warn($"[{OperationRegister}] Username o callback inválido.");
                    return;
                }

                _callbacks[username] = callback;

                _log.Info($"[{OperationRegister}] Callback registrado para: {username}");
            }
            catch (Exception ex)
            {
                _log.Error($"[{OperationRegister}] Error registrando callback de '{username}'", ex);
            }
        }

        // ============================================================
        // REMOVER CALLBACK
        // ============================================================
        public static void Unregister(string username)
        {
            try
            {
                if (_callbacks.TryRemove(username, out _))
                {
                    _log.Info($"[{OperationUnregister}] Callback removido para: {username}");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"[{OperationUnregister}] Error removiendo '{username}'", ex);
            }
        }

        // ============================================================
        // OBTENER CALLBACK
        // ============================================================
        public static IFriendCallback GetCallback(string username)
        {
            try
            {
                if (_callbacks.TryGetValue(username, out var callback))
                {
                    // Verificar si el canal está abierto
                    if (callback is ICommunicationObject channel)
                    {
                        if (channel.State == CommunicationState.Opened)
                        {
                            return callback;
                        }

                        // Si no está abierto → remover
                        _callbacks.TryRemove(username, out _);
                        _log.Warn($"[{OperationGetCallback}] Canal cerrado. Eliminado: {username}");
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _log.Error($"[{OperationGetCallback}] Error obteniendo callback de '{username}'", ex);
                return null;
            }
        }
    }
}
