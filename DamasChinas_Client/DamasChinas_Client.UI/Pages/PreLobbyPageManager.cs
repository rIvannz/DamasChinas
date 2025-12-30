using DamasChinas_Client.UI.LobbyServiceProxy;
using DamasChinas_Shared.Contracts.Dtos;
using System;

namespace DamasChinas_Client.UI.Pages
{
    public static class PreLobbyPageManager
    {
        private static PreLobby _currentPage;


        public static void Register(PreLobby page)
        {
            _currentPage = page;
        }

   
        public static void Unregister(PreLobby page)
        {
            if (_currentPage == page)
            {
                _currentPage = null;
            }
        }

      
        public static void UpdateSnapshot(LobbySnapshotDto snapshot)
        {
            _currentPage?.ApplySnapshot(snapshot);
        }

        public static void UpdateBanInfo(BanInfoDto ban)
        {
            _currentPage?.ApplyBanInfo(ban);
        }
    }
}
