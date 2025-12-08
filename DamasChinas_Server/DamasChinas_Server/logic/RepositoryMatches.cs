using DamasChinas_Server.Common;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.logic;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;

namespace DamasChinas_Server
{
    public class RepositoryMatches : IRepositoryMatches
    {
        private readonly Func<IApplicationDbContext> _contextFactory;

        public RepositoryMatches()
            : this(() => new damas_chinasEntities())
        {
        }

        public RepositoryMatches(Func<IApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public void SaveMatchResult(Dictionary<string, Game.PlayerColor> userColorMap, string winnerUsername)
        {
            if (userColorMap == null || userColorMap.Count == 0)
                throw new ArgumentException("userColorMap cannot be null or empty", nameof(userColorMap));

            if (string.IsNullOrWhiteSpace(winnerUsername))
                throw new ArgumentException("winnerUsername cannot be null or empty", nameof(winnerUsername));

            ExecuteInContext(db =>
            {
                // 1) Crear partida
                var partida = new partidas
                {
                    fecha_partida = DateTime.UtcNow
                };

                db.partidas.Add(partida);
                SaveChangesSafely(db);

                int idPartida = partida.id_partida;

                // 2) Orden de posiciones (ganador primero)
                var ordered = new List<string> { winnerUsername };
                ordered.AddRange(
                    userColorMap.Keys.Where(u =>
                        !u.Equals(winnerUsername, StringComparison.OrdinalIgnoreCase))
                );

                int pos = 1;

                foreach (string user in ordered)
                {
                    var perfil = db.perfiles.SingleOrDefault(
                        p => p.username.Equals(user, StringComparison.OrdinalIgnoreCase));

                    if (perfil == null)
                        throw new RepositoryValidationException(MessageCode.UserProfileNotFound);

                    var participante = new participantes_partida
                    {
                        id_partida = idPartida,
                        id_jugador = perfil.id_usuario,
                        posicion_final = pos
                    };

                    db.participantes_partida.Add(participante);
                    pos++;
                }

                // 3) Guardar todos
                SaveChangesSafely(db);

                return true;
            });
        }

        // ============================================================
        // HELPERS
        // ============================================================

        private static void SaveChangesSafely(IApplicationDbContext db)
        {
            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException)
            {
                throw new RepositoryValidationException(MessageCode.UnknownError);
            }
        }

        private T ExecuteInContext<T>(Func<IApplicationDbContext, T> operation)
        {
            using (var db = _contextFactory())
            {
                return operation(db);
            }
        }
    }
}
