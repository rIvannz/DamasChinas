using DamasChinas_Server;
using DamasChinas_Server.GameRepositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamasChinas_Tests.logic
{

    public class RepositoryReportsTests
    {
        private Mock<damas_chinasEntities> CreateMockContext(
            List<Reportes> reportesData,
            out Mock<DbSet<Reportes>> mockReportes)
        {
            // Usamos EXACTAMENTE tu helper:
            mockReportes = MockDbSetHelper.CreateMockSet(reportesData);

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.Reportes).Returns(mockReportes.Object);

            return mockDb;
        }

        [Fact]
        public void AddReport_CreatesReportCorrectly()
        {
            // Arrange
            var reportes = new List<Reportes>();
            var mockDb = CreateMockContext(reportes, out var mockSet);

            mockDb.Setup(db => db.SaveChanges()).Returns(1);

            var repo = new RepositoryReports(() => mockDb.Object);

            // Act
            repo.AddReport(1, 2, 10, "Toxicidad");

            // Assert
            Assert.Single(reportes);

            var r = reportes[0];
            Assert.Equal(1, r.id_usuario_reportador);
            Assert.Equal(2, r.id_usuario_reportado);
            Assert.Equal(10, r.id_partida);
            Assert.Equal("Toxicidad", r.motivo);
            Assert.Equal("pendiente", r.estado);
            Assert.True(r.fecha_reporte <= DateTime.UtcNow);
        }

     
        [Fact]
        public void AddReport_SaveFails_ThrowsWrappedException()
        {
            // Arrange
            var reportes = new List<Reportes>();
            var mockDb = CreateMockContext(reportes, out var mockSet);

            mockDb.Setup(db => db.SaveChanges())
                  .Throws(new Exception("DB error"));

            var repo = new RepositoryReports(() => mockDb.Object);

            // Act + Assert
            var ex = Assert.Throws<Exception>(() =>
                repo.AddReport(1, 2, 3, "test")
            );

            Assert.Contains("Report repository error", ex.Message);
        }

        [Fact]
        public void CountReportsForUser_NoReports_ReturnsZero()
        {
            // Arrange
            var reportes = new List<Reportes>();
            var mockDb = CreateMockContext(reportes, out var mockSet);

            var repo = new RepositoryReports(() => mockDb.Object);

            // Act
            int result = repo.CountReportsForUser(5);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void CountReportsForUser_MultipleReports_ReturnsCorrectCount()
        {
            // Arrange
            var reportes = new List<Reportes>
        {
            new Reportes { id_usuario_reportado = 7 },
            new Reportes { id_usuario_reportado = 7 },
            new Reportes { id_usuario_reportado = 10 }
        };

            var mockDb = CreateMockContext(reportes, out var mockSet);

            var repo = new RepositoryReports(() => mockDb.Object);

            // Act
            int result = repo.CountReportsForUser(7);

            // Assert
            Assert.Equal(2, result);
        }
    }
}