using System.Data.Common;

namespace DamasChinas_Server.Teste.Integration
{
    public class TestFriendRepository : FriendRepository
    {
        private readonly DbConnection _conn;

        public TestFriendRepository(DbConnection conn)
        {
            _conn = conn;
        }

        public override damas_chinasEntities CreateDbContext()
        {
            return new TestDbContext(_conn);
        }
    }
}
