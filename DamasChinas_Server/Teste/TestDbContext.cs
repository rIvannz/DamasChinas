using System.Data.Common;

namespace DamasChinas_Server.Teste.Integration
{
    public class TestDbContext : damas_chinasEntities
    {
        public TestDbContext(DbConnection connection)
            : base(connection, contextOwnsConnection: false)
        {
        }
    }
}
