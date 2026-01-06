using DamasChinas_Server;
using DamasChinas_Server.Utilidades;
using System;
using Xunit;


namespace DamasChinas_Pruebas.Utilities
{
    public class DbContextFactoryTests
    {
        public DbContextFactoryTests()
        {
            DbContextFactory.Reset();
        }

        [Fact]
        public void Configure_ReplacesFactory()
        {
            bool called = false;

            DbContextFactory.Configure(() =>
            {
                called = true;
                return new damas_chinasEntities();
            });

            var ctx = DbContextFactory.Create();

            Assert.NotNull(ctx);
            Assert.True(called);
        }

        [Fact]
        public void Configure_Throws_WhenNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                DbContextFactory.Configure(null)
            );
        }

        [Fact]
        public void Reset_RestoresDefaultFactory()
        {
            bool customCalled = false;

            DbContextFactory.Configure(() =>
            {
                customCalled = true;
                return new damas_chinasEntities();
            });

            DbContextFactory.Reset();

            var ctx = DbContextFactory.Create();

            Assert.True(
    ctx is not null &&
    !customCalled
);

        }

        [Fact]
        public void Create_UsesConfiguredFactory()
        {
            int counter = 0;

            DbContextFactory.Configure(() =>
            {
                counter++;
                return new damas_chinasEntities();
            });

            DbContextFactory.Create();
            DbContextFactory.Create();

            Assert.Equal(2, counter);
        }
    }
}
