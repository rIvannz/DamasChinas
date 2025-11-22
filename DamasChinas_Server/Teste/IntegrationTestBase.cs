using System.Data.Common;
using System.Data.SQLite;
using NUnit.Framework;

namespace DamasChinas_Server.Teste.Integration
{
    public abstract class IntegrationTestBase
    {
        protected DbConnection Connection;
        protected TestFriendRepository Repo;

        [SetUp]
        public void Setup()
        {
            Connection = new SQLiteConnection("Data Source=:memory:;Version=3;");
            Connection.Open();

            Repo = new TestFriendRepository(Connection);

            using (var db = Repo.CreateDbContext())
            {
                db.Database.ExecuteSqlCommand(@"
                    CREATE TABLE usuarios (id_usuario INTEGER PRIMARY KEY);
                    CREATE TABLE perfiles (id_perfil INTEGER PRIMARY KEY, id_usuario INTEGER, username TEXT, imagen_perfil TEXT);
                    CREATE TABLE amistades (id_amistad INTEGER PRIMARY KEY, id_usuario1 INTEGER, id_usuario2 INTEGER, fecha_amistad TEXT);
                    CREATE TABLE solicitudes_amistad (id_solicitud INTEGER PRIMARY KEY, id_emisor INTEGER, id_receptor INTEGER, fecha_envio TEXT, estado TEXT);
                    CREATE TABLE bloqueos (id_bloqueo INTEGER PRIMARY KEY, id_bloqueador INTEGER, id_bloqueado INTEGER, fecha_bloqueo TEXT);
                ");
            }

            Seed();
        }

        private void Seed()
        {
            using (var db = Repo.CreateDbContext())
            {
                db.usuarios.Add(new usuarios { id_usuario = 1 });
                db.perfiles.Add(new perfiles { id_usuario = 1, username = "alice" });

                db.usuarios.Add(new usuarios { id_usuario = 2 });
                db.perfiles.Add(new perfiles { id_usuario = 2, username = "bob" });

                db.SaveChanges();
            }
        }
    }
}
