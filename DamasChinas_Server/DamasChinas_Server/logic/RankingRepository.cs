using DamasChinas_Server.Dtos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DamasChinas_Server
{
    public class RankingRepository
    {
        private readonly RepositoryUsers _userRepo = new RepositoryUsers();
        private readonly Func<damas_chinasEntities> _dbFactory;

        public RankingRepository()
        {
            _dbFactory = () => new damas_chinasEntities();
        }

        public RankingRepository(Func<damas_chinasEntities> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public virtual damas_chinasEntities CreateDbContext()
        {
            return _dbFactory();
        }

        public List<RankingEntry> GetTop10Players()
        {
            using (var db = CreateDbContext())
            {
                var data =
                    (from p in db.participantes_partida
                     group p by p.id_jugador into g
                     let stats = new
                     {
                         Matches = g.Count(),
                         Wins = g.Count(x => x.posicion_final == 1)
                     }
                     where stats.Matches > 0
                     orderby stats.Wins descending, stats.Matches descending
                     select new
                     {
                         PlayerId = g.Key,
                         stats.Matches,
                         stats.Wins
                     })
                    .Take(10)
                    .ToList();

                var ranking = new List<RankingEntry>();

                foreach (var row in data)
                {
                    var user = db.usuarios
                        .Include(u => u.perfiles)
                        .FirstOrDefault(u => u.id_usuario == row.PlayerId);

                    if (user == null)
                        continue;

                    var profile = user.perfiles.FirstOrDefault();

                    ranking.Add(new RankingEntry
                    {
                        Username = profile?.username ?? "N/A",
                        AvatarFile = profile?.imagen_perfil ?? "avatarIcon.png",
                        MatchesPlayed = row.Matches,
                        Wins = row.Wins,
                        Loses = row.Matches - row.Wins,
                        WinRate = row.Matches == 0 ? 0 : (double)row.Wins / row.Matches
                    });
                }

                return ranking;
            }
        }
    }
}
