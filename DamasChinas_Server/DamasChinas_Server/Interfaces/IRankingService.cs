using System.Collections.Generic;
using System.ServiceModel;
using DamasChinas_Server.Dtos;

namespace DamasChinas_Server.Interfaces
{
    [ServiceContract]
    public interface IRankingService
    {
        [OperationContract]
        List<RankingEntry> GetTop10Ranking();
    }
}
