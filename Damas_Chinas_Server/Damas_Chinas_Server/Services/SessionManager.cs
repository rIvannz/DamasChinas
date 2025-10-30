using System;
using System.Collections.Concurrent;
using Damas_Chinas_Server.Interfaces;

namespace Damas_Chinas_Server.Services
{
	public static class SessionManager
	{
		private static readonly ConcurrentDictionary<string, ISessionCallback> ActiveSessions =
			new ConcurrentDictionary<string, ISessionCallback>();

		public static void AddSession(string username, ISessionCallback callback)
		{
			ActiveSessions[username] = callback;
		}

                public static void RemoveSession(string nickname)
                {
                        ActiveSessions.TryRemove(nickname, out _);
                }

                public static ISessionCallback GetSession(string nickname)
                {
                        ActiveSessions.TryGetValue(nickname, out var callback);
                        return callback;
                }

                public static bool IsOnline(string nickname)
                {
                        return ActiveSessions.ContainsKey(nickname);
                }

                public static void UpdateSessionUsername(string currentUsername, string newUsername)
                {
                        if (string.IsNullOrWhiteSpace(currentUsername) || string.IsNullOrWhiteSpace(newUsername) ||
                                currentUsername.Equals(newUsername))
                        {
                                return;
                        }

                        if (ActiveSessions.TryRemove(currentUsername, out var callback))
                        {
                                ActiveSessions[newUsername] = callback;
                        }
                }
        }
}
