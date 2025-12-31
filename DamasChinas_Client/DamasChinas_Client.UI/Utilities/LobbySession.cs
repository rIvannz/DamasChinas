using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI
{
    public static class LobbySession
    {
        public static LobbyManager Manager { get; } = new LobbyManager();

        public static void Reset()
        {
            try { Manager.Reset(); } catch { }
        }
    }
}
