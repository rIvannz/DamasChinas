using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using DamasChinas_Server;
using DamasChinas_Server.Common;
using DamasChinas_Server.Game;
using DamasChinas_Server.Interfaces;
using Moq;
using Xunit;

namespace DamasChinas_Tests.logic
{
    public class RepositoryMatchesTests
    {
        [Fact]
        public void SaveMatchResultUserColorMapNullThrowsArgumentException()
        {
            var repo = new RepositoryMatches(() => Mock.Of<IApplicationDbContext>());

            Assert.True(
                Assert.Throws<ArgumentException>(() =>
                    repo.SaveMatchResult(null, "Seth")
                ).ParamName == "userColorMap"
            );
        }

        [Fact]
        public void SaveMatchResultUserColorMapEmptyThrowsArgumentException()
        {
            var repo = new RepositoryMatches(() => Mock.Of<IApplicationDbContext>());

            Assert.True(
                Assert.Throws<ArgumentException>(() =>
                    repo.SaveMatchResult(new Dictionary<string, PlayerColor>(), "Seth")
                ).ParamName == "userColorMap"
            );
        }

        [Fact]
        public void SaveMatchResultWinnerUsernameNullThrowsArgumentException()
        {
            var repo = new RepositoryMatches(() => Mock.Of<IApplicationDbContext>());

            Assert.True(
                Assert.Throws<ArgumentException>(() =>
                    repo.SaveMatchResult(
                        new Dictionary<string, PlayerColor> { { "Seth", PlayerColor.Red } },
                        null
                    )
                ).ParamName == "winnerUsername"
            );
        }

        [Fact]
        public void SaveMatchResultWinnerUsernameEmptyThrowsArgumentException()
        {
            var repo = new RepositoryMatches(() => Mock.Of<IApplicationDbContext>());

            Assert.True(
                Assert.Throws<ArgumentException>(() =>
                    repo.SaveMatchResult(
                        new Dictionary<string, PlayerColor> { { "Seth", PlayerColor.Red } },
                        ""
                    )
                ).ParamName == "winnerUsername"
            );
        }

        [Fact]
        public void SaveMatchResultProfileNotFoundThrowsUserProfileNotFound()
        {
            var mockDb = new Mock<IApplicationDbContext>();

            mockDb.Setup(db => db.partidas)
                .Returns(MockDbSetHelper.CreateMockSet(new List<partidas>()).Object);

            mockDb.Setup(db => db.participantes_partida)
                .Returns(MockDbSetHelper.CreateMockSet(new List<participantes_partida>()).Object);

            mockDb.Setup(db => db.perfiles)
                .Returns(MockDbSetHelper.CreateMockSet(new List<perfiles>()).Object);

            var repo = new RepositoryMatches(() => mockDb.Object);

            Assert.True(
                Assert.Throws<RepositoryValidationException>(() =>
                    repo.SaveMatchResult(
                        new Dictionary<string, PlayerColor> { { "Seth", PlayerColor.Red } },
                        "Seth"
                    )
                ).Code == MessageCode.UserProfileNotFound
            );
        }

        [Fact]
        public void SaveMatchResultValidInputCreatesMatchAndParticipants()
        {
            var perfiles = new List<perfiles>
            {
                new perfiles { id_usuario = 1, username = "Seth" },
                new perfiles { id_usuario = 2, username = "Ana" }
            };

            var partidas = new List<partidas>();
            var participantes = new List<participantes_partida>();

            var mockDb = new Mock<IApplicationDbContext>();

            mockDb.Setup(db => db.partidas)
                .Returns(MockDbSetHelper.CreateMockSet(partidas).Object);

            mockDb.Setup(db => db.participantes_partida)
                .Returns(MockDbSetHelper.CreateMockSet(participantes).Object);

            mockDb.Setup(db => db.perfiles)
                .Returns(MockDbSetHelper.CreateMockSet(perfiles).Object);

            var repo = new RepositoryMatches(() => mockDb.Object);

            repo.SaveMatchResult(
                new Dictionary<string, PlayerColor>
                {
                    { "Seth", PlayerColor.Red },
                    { "Ana", PlayerColor.Blue }
                },
                "Seth"
            );

            Assert.Equal(2, participantes.Count);
        }

        [Fact]
        public void SaveMatchResultWinnerAssignedPositionOne()
        {
            var perfiles = new List<perfiles>
            {
                new perfiles { id_usuario = 1, username = "Seth" },
                new perfiles { id_usuario = 2, username = "Ana" }
            };

            var participantes = new List<participantes_partida>();

            var mockDb = new Mock<IApplicationDbContext>();

            mockDb.Setup(db => db.partidas)
                .Returns(MockDbSetHelper.CreateMockSet(new List<partidas>()).Object);

            mockDb.Setup(db => db.participantes_partida)
                .Returns(MockDbSetHelper.CreateMockSet(participantes).Object);

            mockDb.Setup(db => db.perfiles)
                .Returns(MockDbSetHelper.CreateMockSet(perfiles).Object);

            var repo = new RepositoryMatches(() => mockDb.Object);

            repo.SaveMatchResult(
                new Dictionary<string, PlayerColor>
                {
                    { "Ana", PlayerColor.Blue },
                    { "Seth", PlayerColor.Red }
                },
                "Seth"
            );

            var winner = participantes.First(p => p.id_jugador == 1);

            Assert.Equal(1, winner.posicion_final);
        }

        [Fact]
        public void SaveMatchResultFirstSaveChangesFailsThrowsDatabaseUnavailable()
        {
            var mockDb = new Mock<IApplicationDbContext>();

            mockDb.Setup(db => db.partidas)
                .Returns(MockDbSetHelper.CreateMockSet(new List<partidas>()).Object);

            mockDb.Setup(db => db.SaveChanges())
                .Throws(new Exception("DB down"));

            var repo = new RepositoryMatches(() => mockDb.Object);

            Assert.True(
                Assert.Throws<RepositoryValidationException>(() =>
                    repo.SaveMatchResult(
                        new Dictionary<string, PlayerColor> { { "Seth", PlayerColor.Red } },
                        "Seth"
                    )
                ).Code == MessageCode.DatabaseUnavailable
            );
        }

        [Fact]
        public void SaveMatchResultSecondSaveChangesFailsThrowsDatabaseUnavailable()
        {
            var perfiles = new List<perfiles>
            {
                new perfiles { id_usuario = 1, username = "Seth" }
            };

            var saveCount = 0;

            var mockDb = new Mock<IApplicationDbContext>();

            mockDb.Setup(db => db.partidas)
                .Returns(MockDbSetHelper.CreateMockSet(new List<partidas>()).Object);

            mockDb.Setup(db => db.participantes_partida)
                .Returns(MockDbSetHelper.CreateMockSet(new List<participantes_partida>()).Object);

            mockDb.Setup(db => db.perfiles)
                .Returns(MockDbSetHelper.CreateMockSet(perfiles).Object);

            mockDb.Setup(db => db.SaveChanges())
                .Callback(() =>
                {
                    saveCount++;
                    if (saveCount == 2)
                    {
                        throw new Exception("DB down");
                    }
                });

            var repo = new RepositoryMatches(() => mockDb.Object);

            Assert.True(
                Assert.Throws<RepositoryValidationException>(() =>
                    repo.SaveMatchResult(
                        new Dictionary<string, PlayerColor> { { "Seth", PlayerColor.Red } },
                        "Seth"
                    )
                ).Code == MessageCode.DatabaseUnavailable
            );
        }

        [Fact]
        public void SaveMatchResultSaveChangesValidationFailsThrowsUnknownError()
        {
            var mockDb = new Mock<IApplicationDbContext>();

            mockDb.Setup(db => db.partidas)
                .Returns(MockDbSetHelper.CreateMockSet(new List<partidas>()).Object);

            mockDb.Setup(db => db.SaveChanges())
                .Throws(new DbEntityValidationException());

            var repo = new RepositoryMatches(() => mockDb.Object);

            Assert.True(
                Assert.Throws<RepositoryValidationException>(() =>
                    repo.SaveMatchResult(
                        new Dictionary<string, PlayerColor> { { "Seth", PlayerColor.Red } },
                        "Seth"
                    )
                ).Code == MessageCode.UnknownError
            );
        }
    }
}
