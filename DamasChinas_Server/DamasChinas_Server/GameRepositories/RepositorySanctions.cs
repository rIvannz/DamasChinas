using DamasChinas_Server.Dtos;
using DamasChinas_Shared.Contracts.Dtos;
using DamasChinas_Server.Utilidades;
using System;
using System.Linq;

namespace DamasChinas_Server.GameRepositories
{
    public sealed class RepositorySanctions
    {
        private const int ReportsFirstBan = 3;
        private const int ReportsSecondBan = 6;
        private const int ReportsThirdBan = 9;
        private const int ReportsPermanentBan = 12;

        private readonly Func<damas_chinasEntities> _contextFactory;

        public RepositorySanctions()
            : this(DbContextFactory.Create)
        {
        }

        public RepositorySanctions(Func<damas_chinasEntities> factory)
        {
            _contextFactory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public void ApplyBan(int userId, bool permanent, DateTime? untilUtc, string reason)
        {
            using (var db = _contextFactory())
            {
                DeactivateActiveSanctions(db, userId);

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

        public bool HasActiveBan(int userId)
        {
            DateTime now = DateTime.UtcNow;

            using (var db = _contextFactory())
            {
                var active = db.Sanciones
                    .Where(s => s.id_usuario == userId && s.activo == true)
                    .OrderByDescending(s => s.fecha_inicio)
                    .FirstOrDefault();

                if (active == null)
                {
                    return false;
                }

                if (active.fecha_fin.HasValue && active.fecha_fin.Value <= now)
                {
                    active.activo = false;
                    SaveChangesSafely(db);
                    return false;
                }

                return true;
            }
        }

        public BanInfoDto GetActiveBanInfo(int userId)
        {
            DateTime now = DateTime.UtcNow;

            using (var db = _contextFactory())
            {
                var active = db.Sanciones
                    .Where(s => s.id_usuario == userId && s.activo == true)
                    .OrderByDescending(s => s.fecha_inicio)
                    .FirstOrDefault();

                if (active == null)
                {
                    return new BanInfoDto
                    {
                        IsBanned = false,
                        IsPermanent = false,
                        BanUntilUtc = null,
                        TotalReports = 0
                    };
                }

                if (active.fecha_fin.HasValue && active.fecha_fin.Value <= now)
                {
                    active.activo = false;
                    SaveChangesSafely(db);

                    return new BanInfoDto
                    {
                        IsBanned = false,
                        IsPermanent = false,
                        BanUntilUtc = null,
                        TotalReports = 0
                    };
                }

                bool permanent = string.Equals(active.tipo_sancion, "permanente", StringComparison.OrdinalIgnoreCase);

                return new BanInfoDto
                {
                    IsBanned = true,
                    IsPermanent = permanent,
                    BanUntilUtc = permanent ? (DateTime?)null : active.fecha_fin,
                    TotalReports = 0
                };
            }
        }

        /// <summary>
        /// Aplica sanción SOLO cuando se llega EXACTAMENTE a 3/6/9 o >=12.
        /// Si ya existe una sanción activa, no la vuelve a crear.
        /// </summary>
        public BanInfoDto ApplyBanFromReports(int userId, int totalReports, string reason)
        {
            if (userId <= 0)
            {
                return new BanInfoDto
                {
                    IsBanned = false,
                    IsPermanent = false,
                    BanUntilUtc = null,
                    TotalReports = totalReports
                };
            }

            // Si ya está baneado, NO reaplicar (evita resetear tiempos)
            if (HasActiveBan(userId))
            {
                var current = GetActiveBanInfo(userId);
                current.TotalReports = totalReports;
                return current;
            }

            // Solo sancionar en hits exactos: 3, 6, 9, 12+
            if (totalReports >= ReportsPermanentBan)
            {
                ApplyBan(userId, true, null, reason);

                return new BanInfoDto
                {
                    IsBanned = true,
                    IsPermanent = true,
                    BanUntilUtc = null,
                    TotalReports = totalReports
                };
            }

            if (totalReports == ReportsThirdBan)
            {
                DateTime until = DateTime.UtcNow.AddDays(1);
                ApplyBan(userId, false, until, reason);

                return new BanInfoDto
                {
                    IsBanned = true,
                    IsPermanent = false,
                    BanUntilUtc = until,
                    TotalReports = totalReports
                };
            }

            if (totalReports == ReportsSecondBan)
            {
                DateTime until = DateTime.UtcNow.AddHours(1);
                ApplyBan(userId, false, until, reason);

                return new BanInfoDto
                {
                    IsBanned = true,
                    IsPermanent = false,
                    BanUntilUtc = until,
                    TotalReports = totalReports
                };
            }

            if (totalReports == ReportsFirstBan)
            {
                DateTime until = DateTime.UtcNow.AddMinutes(15);
                ApplyBan(userId, false, until, reason);

                return new BanInfoDto
                {
                    IsBanned = true,
                    IsPermanent = false,
                    BanUntilUtc = until,
                    TotalReports = totalReports
                };
            }

            // No hay sanción en 1,2,4,5,7,8,10,11...
            return new BanInfoDto
            {
                IsBanned = false,
                IsPermanent = false,
                BanUntilUtc = null,
                TotalReports = totalReports
            };
        }

        private static void DeactivateActiveSanctions(damas_chinasEntities db, int userId)
        {
            var actives = db.Sanciones
                .Where(s => s.id_usuario == userId && s.activo == true)
                .ToList();

            if (actives.Count == 0)
            {
                return;
            }

            foreach (var s in actives)
            {
                s.activo = false;
            }
        }

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
