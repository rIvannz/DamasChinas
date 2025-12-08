using System.Collections.Generic;
using DamasChinas_Server.Game;

namespace DamasChinas_Server.Interfaces
{
    public interface IRepositoryMatches
    {
        void SaveMatchResult(Dictionary<string, PlayerColor> userColorMap, string winnerUsername);
    }
}
