using DamasChinas_Client.UI.LobbyServiceProxy;
using System;

namespace DamasChinas_Client.UI.Pages
{
    public static class PreLobbyPageManager
    {
        private static PreLobby _currentPage;

        // Llamado cuando entras al PreLobby
        public static void Register(PreLobby page)
        {
            _currentPage = page;
        }

        // Llamado cuando sales del PreLobby
        public static void Unregister(PreLobby page)
        {
            if (_currentPage == page)
            {
                _currentPage = null;
            }
        }

        // Cuando el servidor manda snapshot
        public static void UpdateSnapshot(LobbySnapshotDto snapshot)
        {
            _currentPage?.ApplySnapshot(snapshot);
        }

        // Cuando el servidor envía info de ban
        public static void UpdateBanInfo(BanInfoDto ban)
        {
            _currentPage?.ApplyBanInfo(ban);
        }
    }
}
