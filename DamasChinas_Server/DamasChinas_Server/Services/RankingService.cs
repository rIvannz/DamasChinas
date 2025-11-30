using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Dtos;
using System.Collections.Generic;

namespace DamasChinas_Server.Services
{
    public class RankingService : IRankingService
    {
        private readonly RankingRepository _repo = new RankingRepository();

        public List<RankingEntry> GetTop10Ranking()
        {
            return _repo.GetTop10Players();
        }
    }
}
