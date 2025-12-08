using System;
using DamasChinas_Server.Utilidades;

namespace DamasChinas_Server.GameRepositories
{
    public sealed class RepositoryMatches
    {
        private readonly Func<damas_chinasEntities> _contextFactory;

        public RepositoryMatches()
            : this(DbContextFactory.Create)
        {
        }

        public RepositoryMatches(Func<damas_chinasEntities> factory)
        {
            _contextFactory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        // =========================================================
        // PARTIDAS
        // =========================================================

        public int CreateMatch()
        {
            using (var db = _contextFactory())
            {
                var match = new partidas
                {
                    fecha_partida = DateTime.UtcNow
                };

                db.partidas.Add(match);
                SaveChangesSafely(db);

                return match.id_partida;
            }
        }

        public void AddPlayerResult(int matchId, int userId, int finalPosition)
        {
            using (var db = _contextFactory())
            {
                var entry = new participantes_partida
                {
                    id_partida = matchId,
                    id_jugador = userId,
                    posicion_final = finalPosition
                };

                db.participantes_partida.Add(entry);
                SaveChangesSafely(db);
            }
        }

        // =========================================================
        // HELPERS
        // =========================================================

        private static void SaveChangesSafely(damas_chinasEntities db)
        {
            try
            {
                db.SaveChanges();
            }
            catch
            {
                throw new Exception("Match repository error: unable to save changes.");
            }
        }
    }
}
