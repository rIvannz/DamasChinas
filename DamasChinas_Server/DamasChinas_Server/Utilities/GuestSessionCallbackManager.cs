using DamasChinas_Shared.Contracts;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace DamasChinas_Server.Utilities
{
    internal static class GuestSessionCallbackManager
    {
        private static readonly ConcurrentDictionary<string, IGuestSessionCallback> _callbacks
            = new ConcurrentDictionary<string, IGuestSessionCallback>(StringComparer.OrdinalIgnoreCase);

        public static void Add(string guestUsername, IGuestSessionCallback callback)
        {
            if (string.IsNullOrWhiteSpace(guestUsername) || callback == null)
            {
                return;
            }

            _callbacks[guestUsername] = callback;
        }

        public static void Remove(string guestUsername)
        {
            if (string.IsNullOrWhiteSpace(guestUsername))
            {
                return;
            }

            _callbacks.TryRemove(guestUsername, out _);
        }

        public static void NotifyAll(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                code = "ServerUnavailable";
            }

            foreach (var kv in _callbacks.ToArray())
            {
                try
                {
                    kv.Value.OnServerMessage(code);
                }
                catch
                {
                    _callbacks.TryRemove(kv.Key, out _);
                }
            }
        }
    }
}
