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

        // ✅ idPartida y codigoLobby son opcionales
        public void AddReport(int reporterId, int reportedId, int? idPartida, int? codigoLobby, string motivo)
        {
            using (var db = _contextFactory())
            {
                var report = new Reportes
                {
                    id_usuario_reportador = reporterId,
                    id_usuario_reportado = reportedId,

                    // ✅ si viene de lobby, idPartida debe ir NULL para NO romper FK
                    id_partida = idPartida,

                    // ✅ nuevo campo (requiere update del EDMX)
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
            catch (Exception ex)
            {
                throw new Exception($"Report repository error: unable to save changes. {ex.Message}");
            }
        }
    }
}
