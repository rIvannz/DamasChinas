using System;
using System.Collections.Concurrent;
using System.Linq;
using System.ServiceModel;
using DamasChinas_Server.Common;
using DamasChinas_Shared.Contracts.Dtos;

namespace DamasChinas_Server.Logic
{
    public sealed class GlobalSessionManager
    {
        private static readonly Lazy<GlobalSessionManager> _instance =
            new Lazy<GlobalSessionManager>(() => new GlobalSessionManager());

        public static GlobalSessionManager Instance => _instance.Value;

        private readonly ConcurrentDictionary<string, ICommunicationObject> _channels =
            new ConcurrentDictionary<string, ICommunicationObject>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, Action<MessageCode>> _notifiers =
            new ConcurrentDictionary<string, Action<MessageCode>>(StringComparer.OrdinalIgnoreCase);

        private readonly ILogService _log;

        private GlobalSessionManager()
        {
            _log = LogFactory.Create(typeof(GlobalSessionManager));
        }

        public void Register(string sessionKey, ICommunicationObject channel, Action<MessageCode> notify)
        {
            if (string.IsNullOrWhiteSpace(sessionKey) || channel == null || notify == null)
            {
                return;
            }

            _channels[sessionKey] = channel;
            _notifiers[sessionKey] = notify;

            channel.Closed += (_, __) => Unregister(sessionKey);
            channel.Faulted += (_, __) => Unregister(sessionKey);
        }

        public void Unregister(string sessionKey)
        {
            if (string.IsNullOrWhiteSpace(sessionKey))
            {
                return;
            }

            _channels.TryRemove(sessionKey, out _);
            _notifiers.TryRemove(sessionKey, out _);
        }

        public void ForceDisconnectAll(MessageCode reason)
        {
            var keys = _channels.Keys.ToArray();

            foreach (string key in keys)
            {
                if (_notifiers.TryGetValue(key, out var notify))
                {
                    try
                    {
                        notify(reason);
                    }
                    catch
                    {
                        _log.Info($"[GlobalSessionManager] Ignoered exception during forced disconnect:");
                    }
                }

                if (_channels.TryGetValue(key, out var comm))
                {
                    try
                    {
                        comm.Abort(); 
                    }
                    catch
                    {
                        _log.Info($"[GlobalSessionManager] Ignored exception during forced disconnect:");
                    }
                }

                Unregister(key);
            }

            _log.Error($"[GlobalSessionManager] ForceDisconnectAll. Reason={reason}, Count={keys.Length}");

        }
    }
}
