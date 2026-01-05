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

                EventHandler closedHandler = null;
                EventHandler faultedHandler = null;

                closedHandler = (_, __) => Remove(key);
                faultedHandler = (_, __) => Remove(key);

                channel.Closed -= closedHandler;
                channel.Faulted -= faultedHandler;

                channel.Closed += closedHandler;
                channel.Faulted += faultedHandler;

                _log.Info($"[Add] Guest callback add: {key}");
            }
            catch (InvalidOperationException ex)
            {
                _log.Error($"[Add] Invalid WCF context for guest callback: {key}", ex);
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
                _log.Info($"[Remove] Guest callback removed: {key}");
            }
            catch (InvalidOperationException ex)
            {
                _log.Error($"[Add] Invalid WCF context for guest callback: {key}", ex);
            }
            catch (CommunicationException ex)
            {
                _log.Error($"[Add] Communication error while adding guest callback: {key}", ex);
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
                catch (InvalidOperationException ex)
                {
                    _log.Warn($"[NotifyAll] fall callback, removed: {kv.Key}. {ex.Message}");
                    _callbacks.TryRemove(kv.Key, out _);

                }
                catch (CommunicationException ex)
                {
                    _log.Warn($"[NotifyAll] fall callback, removed: {kv.Key}. {ex.Message}");
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
                catch 
                {
                    _log.Warn($"[NotifyAll] ForceDisconectAll fall for a client ");

                }

                try
                {
                    kv.Value.Channel?.Abort();
                }
                catch 
                {
                    _log.Warn($"[NotifyAll] ForceDisconectAll ffall for a client ");

                }

                _callbacks.TryRemove(kv.Key, out _);
            }
        }

        private static string ToClientKey(MessageCode code)
        {

            return "msg_" + code.ToString();
        }
    }
}
