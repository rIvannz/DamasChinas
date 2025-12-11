using System;
using System.Linq;
using DamasChinas_Server.Utilidades;

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



        public void AddReport(int reporterId, int reportedId, int matchId, string reason)
        {
            using (var db = _contextFactory())
            {
                var report = new Reportes
                {
                    id_usuario_reportador = reporterId,
                    id_usuario_reportado = reportedId,
                    id_partida = matchId,
                    motivo = reason,
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


        private static void SaveChangesSafely(damas_chinasEntities db)
        {
            try
            {
                db.SaveChanges();
            }
            catch
            {
                throw new Exception("Report repository error: unable to save changes.");
            }
        }
    }
}
