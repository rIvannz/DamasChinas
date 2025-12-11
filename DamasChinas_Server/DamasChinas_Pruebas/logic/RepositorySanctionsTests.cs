using DamasChinas_Server;
using DamasChinas_Server.GameRepositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamasChinas_Tests.logic
{
    public class RepositorySanctionsTests
    {
        private RepositorySanctions CreateRepo(Mock<damas_chinasEntities> mockDb)
        {
            return new RepositorySanctions(() => mockDb.Object);
        }

        [Fact]
        public void ApplyBan_PermanentBan_AddsSanctionCorrectly()
        {
            // Arrange
            var sancionesList = new List<Sanciones>();
            var mockSet = MockDbSetHelper.CreateMockSet(sancionesList);

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.Sanciones).Returns(mockSet.Object);

            mockDb.Setup(db => db.SaveChanges());

            var repo = CreateRepo(mockDb);

            // Act
            repo.ApplyBan(10, permanent: true, untilUtc: null, reason: "Toxicity");

            // Assert
            Assert.Single(sancionesList);
            var s = sancionesList.First();
            Assert.Equal(10, s.id_usuario);
            Assert.Equal("permanente", s.tipo_sancion);
            Assert.True(s.activo);
            Assert.Null(s.fecha_fin);
            Assert.Equal("Toxicity", s.motivo_acumulado);
            mockDb.Verify(db => db.SaveChanges(), Times.Once);
        }

        [Fact]
        public void ApplyBan_TemporaryBan_AssignsCorrectEndDate()
        {
            // Arrange
            var sancionesList = new List<Sanciones>();
            var mockSet = MockDbSetHelper.CreateMockSet(sancionesList);

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.Sanciones).Returns(mockSet.Object);
            mockDb.Setup(db => db.SaveChanges());

            var repo = CreateRepo(mockDb);

            DateTime until = DateTime.UtcNow.AddDays(3);

            // Act
            repo.ApplyBan(20, permanent: false, untilUtc: until, reason: "AFK");

            // Assert
            Assert.Single(sancionesList);
            var s = sancionesList.First();
            Assert.Equal("temporal", s.tipo_sancion);
            Assert.Equal(until, s.fecha_fin);
            Assert.Equal("AFK", s.motivo_acumulado);
            Assert.True(s.activo);
        }

        [Fact]
        public void ApplyBan_SaveChangesFails_ThrowsWrappedException()
        {
            // Arrange
            var sancionesList = new List<Sanciones>();
            var mockSet = MockDbSetHelper.CreateMockSet(sancionesList);

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.Sanciones).Returns(mockSet.Object);

            mockDb.Setup(db => db.SaveChanges())
                  .Throws(new Exception("DB crash"));

            var repo = CreateRepo(mockDb);

            // Act + Assert
            var ex = Assert.Throws<Exception>(() =>
                repo.ApplyBan(1, false, null, "Error test")
            );

            Assert.Contains("Sanctions repository error", ex.Message);
        }




        [Fact]
        public void HasActiveBan_NoSanctions_ReturnsFalse()
        {
            // Arrange
            var sancionesList = new List<Sanciones>();
            var mockSet = MockDbSetHelper.CreateMockSet(sancionesList);

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.Sanciones).Returns(mockSet.Object);

            var repo = CreateRepo(mockDb);

            // Act
            bool result = repo.HasActiveBan(5);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasActiveBan_UserHasInactiveSanction_ReturnsFalse()
        {
            // Arrange
            var list = new List<Sanciones>
        {
            new Sanciones
            {
                id_usuario = 5,
                activo = false,
                fecha_fin = DateTime.UtcNow.AddDays(1)
            }
        };

            var mockSet = MockDbSetHelper.CreateMockSet(list);

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.Sanciones).Returns(mockSet.Object);

            var repo = CreateRepo(mockDb);

            // Act
            bool result = repo.HasActiveBan(5);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasActiveBan_UserHasExpiredSanction_ReturnsFalse()
        {
            // Arrange
            var list = new List<Sanciones>
        {
            new Sanciones
            {
                id_usuario = 7,
                activo = true,
                fecha_fin = DateTime.UtcNow.AddMinutes(-5) // expiró
            }
        };

            var mockSet = MockDbSetHelper.CreateMockSet(list);

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.Sanciones).Returns(mockSet.Object);

            var repo = CreateRepo(mockDb);

            // Act
            bool result = repo.HasActiveBan(7);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasActiveBan_UserHasPermanentActiveBan_ReturnsTrue()
        {
            // Arrange
            var list = new List<Sanciones>
        {
            new Sanciones
            {
                id_usuario = 3,
                activo = true,
                fecha_fin = null
            }
        };

            var mockSet = MockDbSetHelper.CreateMockSet(list);

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.Sanciones).Returns(mockSet.Object);

            var repo = CreateRepo(mockDb);

            // Act
            bool result = repo.HasActiveBan(3);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasActiveBan_TemporaryBanNotExpired_ReturnsTrue()
        {
            // Arrange
            var list = new List<Sanciones>
        {
            new Sanciones
            {
                id_usuario = 8,
                activo = true,
                fecha_fin = DateTime.UtcNow.AddHours(2)
            }
        };

            var mockSet = MockDbSetHelper.CreateMockSet(list);

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.Sanciones).Returns(mockSet.Object);

            var repo = CreateRepo(mockDb);

            // Act
            bool result = repo.HasActiveBan(8);

            // Assert
            Assert.True(result);
        }
    }
}