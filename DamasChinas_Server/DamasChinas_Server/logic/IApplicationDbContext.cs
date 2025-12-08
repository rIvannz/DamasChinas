using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace DamasChinas_Server
{
    public interface IApplicationDbContext : IDisposable
    {
   
        DbSet<usuarios> usuarios { get; }
        DbSet<perfiles> perfiles { get; }

      
        DbSet<amistades> amistades { get; }
        DbSet<bloqueos> bloqueos { get; }

 
        DbSet<solicitudes_amistad> solicitudes_amistad { get; }
        DbSet<mensajes> mensajes { get; }

  
        DbSet<partidas> partidas { get; }
        DbSet<participantes_partida> participantes_partida { get; }

     
        DbSet<movimientos> movimientos { get; }

 
        DbSet<T> Set<T>() where T : class;
        DbEntityEntry Entry(object entity);
        int SaveChanges();
    }
}
