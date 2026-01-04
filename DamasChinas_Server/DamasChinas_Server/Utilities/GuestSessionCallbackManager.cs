using System;
using System.Collections.Concurrent;
using System.Linq;
using System.ServiceModel;
using DamasChinas_Server.Common;
using DamasChinas_Shared.Contracts;

namespace DamasChinas_Server.Utilities
{
    public static class GuestSessionCallbackManager
    {
        private sealed class GuestEntry
        {
            public IGuestSessionCallback Callback { get; }
            public ICommunicationObject Channel { get; }

            public GuestEntry(IGuestSessionCallback callback, ICommunicationObject channel)
            {
                Callback = callback;
                Channel = channel;
            }
        }

        private static readonly ConcurrentDictionary<string, GuestEntry> _callbacks =
            new ConcurrentDictionary<string, GuestEntry>(StringComparer.OrdinalIgnoreCase);

        private static readonly ILogService _log = LogFactory.Create(typeof(GuestSessionCallbackManager));

        private static string Normalize(string username)
        {
            return username?.Trim().ToLowerInvariant();
        }

        public static void Add(string guestUsername, IGuestSessionCallback callback)
        {
            string key = Normalize(guestUsername);
            if (string.IsNullOrWhiteSpace(key) || callback == null)
                return;

            try
            {
                var channel = OperationContext.Current?.Channel;
                if (channel == null)
                    return;

                _callbacks[key] = new GuestEntry(callback, channel);

   
                channel.Closed -= (_, __) => Remove(key);
                channel.Faulted -= (_, __) => Remove(key);
                channel.Closed += (_, __) => Remove(key);
                channel.Faulted += (_, __) => Remove(key);

                _log.Info($"[Add] Guest callback agregado: {key}");
            }
            catch (Exception ex)
            {
                _log.Error($"[Add] Error al agregar guest callback: {key}", ex);
            }
        }

        public static void Remove(string guestUsername)
        {
            string key = Normalize(guestUsername);
            if (string.IsNullOrWhiteSpace(key))
                return;

            try
            {
                _callbacks.TryRemove(key, out _);
                _log.Info($"[Remove] Guest callback removido: {key}");
            }
            catch (Exception ex)
            {
                _log.Error($"[Remove] Error al remover guest callback: {key}", ex);
            }
        }

        public static void NotifyAll(MessageCode code)
        {
            string payload = ToClientKey(code);

            foreach (var kv in _callbacks.ToArray())
            {
                try
                {
                    kv.Value.Callback.OnServerMessage(payload);
                }
                catch (Exception ex)
                {
                    _log.Warn($"[NotifyAll] Falló callback, removiendo: {kv.Key}. {ex.Message}");
                    _callbacks.TryRemove(kv.Key, out _);
                }
            }
        }

        public static void ForceDisconnectAll(MessageCode code)
        {
            string payload = ToClientKey(code);

            foreach (var kv in _callbacks.ToArray())
            {
                try
                {
                    kv.Value.Callback.OnServerMessage(payload);
                }
                catch { }

                try
                {
                    kv.Value.Channel?.Abort();
                }
                catch { }

                _callbacks.TryRemove(kv.Key, out _);
            }
        }

        private static string ToClientKey(MessageCode code)
        {

            return "msg_" + code.ToString();
        }
    }
}
