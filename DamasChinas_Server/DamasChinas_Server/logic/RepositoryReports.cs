using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DamasChinas_Server.Repositories
{
    public sealed class RepositoryReports
    {
        private readonly Func<damas_chinasEntities> _contextFactory;

        public RepositoryReports()
            : this(DbContextFactory.Create)
        {
        }

        public RepositoryReports(Func<damas_chinasEntities> factory)
        {
            _contextFactory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public void AddReport(int reporterId, int reportedId, int matchId, string reason)
        {
            using (var db = _contextFactory())
            {
                var entity = new Reportes
                {
                    id_usuario_reportador = reporterId,
                    id_usuario_reportado = reportedId,
                    id_partida = matchId,
                    motivo = reason,
                    fecha_reporte = DateTime.UtcNow,
                    estado = "pendiente"
                };

                db.Reportes.Add(entity);
                db.SaveChanges();
            }
        }
    }
}
