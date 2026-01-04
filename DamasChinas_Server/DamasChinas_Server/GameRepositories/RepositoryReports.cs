using DamasChinas_Server.Common;
using DamasChinas_Server.Utilidades;
using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;

namespace DamasChinas_Server.GameRepositories
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

        public void AddReport(int reporterId, int reportedId, int? idPartida, int? codigoLobby, string motivo)
        {
            using (var db = _contextFactory())
            {
                var report = new Reportes
                {
                    id_usuario_reportador = reporterId,
                    id_usuario_reportado = reportedId,

                    id_partida = idPartida,

                    codigo_lobby = codigoLobby,

                    motivo = motivo ?? string.Empty,
                    fecha_reporte = DateTime.UtcNow,
                    estado = "pendiente"
                };

                db.Reportes.Add(report);
                SaveChangesSafely(db);
            }
        }

        public int CountReportsForUser(int reportedId)
        {
            using (var db = _contextFactory())
            {
                return db.Reportes.Count(r => r.id_usuario_reportado == reportedId);
            }
        }

        public int AddReportAndGetTotal(int reporterId, int reportedId, int? idPartida, int? codigoLobby, string motivo)
        {
            AddReport(reporterId, reportedId, idPartida, codigoLobby, motivo);
            return CountReportsForUser(reportedId);
        }

        private static void SaveChangesSafely(damas_chinasEntities db)
        {
            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException)
            {
                throw new RepositoryValidationException(MessageCode.DatabaseUnavailable);
            }
            catch (DbUpdateException)
            {
                throw new RepositoryValidationException(MessageCode.DatabaseUnavailable);
            }
            catch (EntityException)
            {
                throw new RepositoryValidationException(MessageCode.DatabaseUnavailable);
            }
            catch (SqlException)
            {
                throw new RepositoryValidationException(MessageCode.DatabaseUnavailable);
            }
        }
    }
}
