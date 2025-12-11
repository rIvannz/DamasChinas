using System;
using System.Linq;
<<<<<<< HEAD
using DamasChinas_Server;                 
=======
using DamasChinas_Server;            
>>>>>>> 096d4752533982c9398c1b7e38f37cff37a7bfd7
using DamasChinas_Server.Utilidades;

namespace DamasChinas_Server.GameRepositories
{
    public sealed class RepositorySanctions
    {
        private readonly Func<damas_chinasEntities> _contextFactory;

        public RepositorySanctions()
            : this(DbContextFactory.Create)
        {
        }

        public RepositorySanctions(Func<damas_chinasEntities> factory)
        {
            _contextFactory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

<<<<<<< HEAD
        
=======

>>>>>>> 096d4752533982c9398c1b7e38f37cff37a7bfd7
        public void ApplyBan(int userId, bool permanent, DateTime? untilUtc, string reason)
        {
            using (var db = _contextFactory())
            {
                var sanction = new Sanciones
                {
                    id_usuario = userId,
                    tipo_sancion = permanent ? "permanente" : "temporal",
                    fecha_inicio = DateTime.UtcNow,
                    fecha_fin = untilUtc,
                    motivo_acumulado = reason,
                    activo = true
                };

                db.Sanciones.Add(sanction);
                SaveChangesSafely(db);
            }
        }

<<<<<<< HEAD
=======
  
>>>>>>> 096d4752533982c9398c1b7e38f37cff37a7bfd7
        public bool HasActiveBan(int userId)
        {
            DateTime now = DateTime.UtcNow;

            using (var db = _contextFactory())
            {
                return db.Sanciones.Any(s =>
                    s.id_usuario == userId &&
                    s.activo == true &&
                    (s.fecha_fin == null || s.fecha_fin > now));
            }
        }

<<<<<<< HEAD
=======

>>>>>>> 096d4752533982c9398c1b7e38f37cff37a7bfd7

        private static void SaveChangesSafely(damas_chinasEntities db)
        {
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Sanctions repository error: unable to save changes. {ex.Message}");
            }
        }
    }
}
