using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Diagnostics;
using System.ServiceModel;
using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Repositories;

namespace DamasChinas_Server.Services
{
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
            catch (SqlException ex)
            {
                Trace.WriteLine($"[RankingService][SQL] {ex.Message}");
                throw new FaultException<MessageCode>(MessageCode.RankingUnavailable);
            }
            catch (EntityException ex)
            {
                Trace.WriteLine($"[RankingService][EF] {ex.Message}");
                throw new FaultException<MessageCode>(MessageCode.RankingUnavailable);
            }
            catch (TimeoutException ex)
            {
                Trace.WriteLine($"[RankingService][Timeout] {ex.Message}");
                throw new FaultException<MessageCode>(MessageCode.RankingUnavailable);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[RankingService][Unexpected] {ex}");
                throw new FaultException<MessageCode>(MessageCode.RankingUnavailable);
            }
        }

    }
}
