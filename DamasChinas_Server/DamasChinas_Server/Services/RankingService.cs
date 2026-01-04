using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Logic;
using DamasChinas_Server.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Diagnostics;
using System.ServiceModel;

namespace DamasChinas_Server.Services
{
    [DbGuardBehavior]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class RankingService : IRankingService
    {
        private readonly RankingRepository _repository;

        public RankingService()
            : this(new RankingRepository())
        {
        }

        public RankingService(RankingRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public List<RankingEntry> GetTop10Ranking()
        {
            try
            {
                return _repository.GetTop10Players();
            }
            catch (SqlException )
            {
                throw new FaultException<MessageCode>(MessageCode.RankingUnavailable);
            }
            catch (EntityException )
            {
                throw new FaultException<MessageCode>(MessageCode.RankingUnavailable);
            }
            catch (TimeoutException )
            {
                throw new FaultException<MessageCode>(MessageCode.RankingUnavailable);
            }

        }

    }
}
