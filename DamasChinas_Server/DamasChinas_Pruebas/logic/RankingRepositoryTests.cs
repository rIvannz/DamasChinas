using DamasChinas_Server;
using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.logic;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace DamasChinas_Pruebas.logic
{
    public class RankingRepositoryTests
    {
        [Fact]
        public void GetTop10Players_NoPlayers_ReturnsEmptyList()
        {
            // Arrange
            var partidas = new List<participantes_partida>();
            var usuarios = new List<usuarios>();

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.participantes_partida)
                  .Returns(MockDbSetHelper.CreateMockSet(partidas).Object);

            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new Mock<RankingRepository> { CallBase = true };
            repo.Setup(r => r.CreateDbContext()).Returns(mockDb.Object);

            // Act
            var result = repo.Object.GetTop10Players();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetTop10Players_OnePlayer_ReturnsCorrectStats()
        {
            // Arrange
            var partidas = new List<participantes_partida>
    {
        new participantes_partida { id_jugador = 1, posicion_final = 1 },
        new participantes_partida { id_jugador = 1, posicion_final = 2 }
    };

            var usuarios = new List<usuarios>
    {
        new usuarios
        {
            id_usuario = 1,
            perfiles = new List<perfiles>
            {
                new perfiles { username = "seth", imagen_perfil = "pic.png" }
            }
        }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.participantes_partida)
                  .Returns(MockDbSetHelper.CreateMockSet(partidas).Object);

            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new Mock<RankingRepository> { CallBase = true };
            repo.Setup(r => r.CreateDbContext()).Returns(mockDb.Object);

            // Act
            var result = repo.Object.GetTop10Players();

            // Assert
            Assert.Single(result);
            Assert.Equal("seth", result[0].Username);
            Assert.Equal(2, result[0].MatchesPlayed);
            Assert.Equal(1, result[0].Wins);
            Assert.Equal(1, result[0].Loses);
            Assert.Equal(0.5, result[0].WinRate);
        }

        [Fact]
        public void GetTop10Players_OrdersCorrectly_ByWinsThenMatches()
        {
            // Arrange
            var partidas = new List<participantes_partida>
    {
        new participantes_partida { id_jugador = 1, posicion_final = 1 }, // 1 win
        new participantes_partida { id_jugador = 2, posicion_final = 1 }, // 2 wins
        new participantes_partida { id_jugador = 2, posicion_final = 1 },
        new participantes_partida { id_jugador = 3, posicion_final = 2 }, // 0 wins, 3 matches
        new participantes_partida { id_jugador = 3, posicion_final = 2 },
        new participantes_partida { id_jugador = 3, posicion_final = 2 }
    };

            var usuarios = new List<usuarios>
    {
        new usuarios { id_usuario = 1, perfiles = new List<perfiles>{ new perfiles{ username="A" } } },
        new usuarios { id_usuario = 2, perfiles = new List<perfiles>{ new perfiles{ username="B" } } },
        new usuarios { id_usuario = 3, perfiles = new List<perfiles>{ new perfiles{ username="C" } } }
    };

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.participantes_partida)
                  .Returns(MockDbSetHelper.CreateMockSet(partidas).Object);

            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new Mock<RankingRepository> { CallBase = true };
            repo.Setup(r => r.CreateDbContext()).Returns(mockDb.Object);

            // Act
            var result = repo.Object.GetTop10Players();

            // Assert (should be B > A > C)
            Assert.Equal("B", result[0].Username);
            Assert.Equal("A", result[1].Username);
            Assert.Equal("C", result[2].Username);
        }

        [Fact]
        public void GetTop10Players_ReturnsMax10()
        {
            // Arrange
            var partidas = new List<participantes_partida>();
            var usuarios = new List<usuarios>();

            for (int i = 1; i <= 20; i++)
            {
                partidas.Add(new participantes_partida { id_jugador = i, posicion_final = 1 });
                usuarios.Add(new usuarios
                {
                    id_usuario = i,
                    perfiles = new List<perfiles> { new perfiles { username = $"player{i}" } }
                });
            }

            var mockDb = new Mock<damas_chinasEntities>();
            mockDb.Setup(db => db.participantes_partida)
                  .Returns(MockDbSetHelper.CreateMockSet(partidas).Object);
            mockDb.Setup(db => db.usuarios)
                  .Returns(MockDbSetHelper.CreateMockSet(usuarios).Object);

            var repo = new Mock<RankingRepository> { CallBase = true };
            repo.Setup(r => r.CreateDbContext()).Returns(mockDb.Object);

            // Act
            var result = repo.Object.GetTop10Players();

            // Assert
            Assert.Equal(10, result.Count);
        }

    }
}
