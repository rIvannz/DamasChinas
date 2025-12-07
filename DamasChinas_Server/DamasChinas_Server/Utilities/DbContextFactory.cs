using System;
using System.Threading;

namespace DamasChinas_Server.Utilidades
{
    public static class DbContextFactory
    {
        private static Func<damas_chinasEntities> _factory = () => new damas_chinasEntities();

        public static void Configure(Func<damas_chinasEntities> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            Interlocked.Exchange(ref _factory, factory);
        }

        public static void Reset()
        {
            Interlocked.Exchange(ref _factory, () => new damas_chinasEntities());
        }

        public static damas_chinasEntities Create()
        {
            return _factory();
        }
    }
}
