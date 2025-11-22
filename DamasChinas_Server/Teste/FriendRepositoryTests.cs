using NUnit.Framework;

namespace DamasChinas_Server.Teste.Integration
{
    [TestFixture]
    public class FriendRepositoryTests : IntegrationTestBase
    {
        [Test]
        public void SendFriendRequest_ShouldCreatePending()
        {
            var result = Repo.SendFriendRequest("alice", "bob");
            Assert.IsTrue(result);   // ← ya funciona con NUnit
        }
    }
}
