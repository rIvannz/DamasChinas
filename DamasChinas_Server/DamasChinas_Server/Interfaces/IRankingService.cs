using System.Collections.Generic;
using System.ServiceModel;
using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;

namespace DamasChinas_Server.Interfaces
{
    [ServiceContract]
    public interface IRankingService
    {
        [OperationContract]
        [FaultContract(typeof(MessageCode))]
        List<RankingEntry> GetTop10Ranking();
    }
}
