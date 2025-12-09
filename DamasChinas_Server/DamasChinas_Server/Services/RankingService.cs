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
            catch (Exception ex) when (
                ex is SqlException ||
                ex is EntityException ||
                ex is TimeoutException ||
                ex is InvalidOperationException)
            {
                Trace.WriteLine(
                    $"[RankingService.GetTop10Ranking - DataAccess] {ex}");

                // Se envía solo el código de mensaje; el cliente lo traducirá
                // a "En este momento no podemos mostrar el ranking..."
                throw new FaultException<MessageCode>(MessageCode.RankingUnavailable);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(
                    $"[RankingService.GetTop10Ranking - General] {ex}");

                throw new FaultException<MessageCode>(MessageCode.RankingUnavailable);
            }
        }
    }
}
