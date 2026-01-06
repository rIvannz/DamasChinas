using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DamasChinas_Server.Logic;


namespace DamasChinas_Tests.Utilities
{
    public class DbOutageCoordinatorTests
    {
        [Fact]
        public void TripFirstCallExecutesAllActions()
        {
             
            DbOutageCoordinator.ResetForTests();

            bool logCalled = false;
            bool telegramCalled = false;
            bool sessionsCalled = false;
            bool guestsCalled = false;

            DbOutageCoordinator.LogError = (_, __) => logCalled = true;
            DbOutageCoordinator.TelegramSender = _ => telegramCalled = true;
            DbOutageCoordinator.DisconnectSessions = _ => sessionsCalled = true;
            DbOutageCoordinator.DisconnectGuests = _ => guestsCalled = true;

             
            DbOutageCoordinator.Trip(new Exception("DB down"));


            Assert.True(
              logCalled &&
              telegramCalled &&
              sessionsCalled &&
              guestsCalled
          );

        }

        [Fact]
        public void TripCalledTwiceOnlyExecutesOnce()
        {
             
            DbOutageCoordinator.ResetForTests();

            int telegramCalls = 0;

            DbOutageCoordinator.TelegramSender = _ => telegramCalls++;
            DbOutageCoordinator.LogError = (_, __) => { };
            DbOutageCoordinator.DisconnectSessions = _ => { };
            DbOutageCoordinator.DisconnectGuests = _ => { };

             
            DbOutageCoordinator.Trip(new Exception("DB down"));
            DbOutageCoordinator.Trip(new Exception("DB down again"));

             
            Assert.Equal(1, telegramCalls);
        }

        [Fact]
        public void TripWhenTelegramThrowsDoesNotThrow()
        {
             
            DbOutageCoordinator.ResetForTests();

            DbOutageCoordinator.TelegramSender =
                _ => throw new Exception("Telegram down");

            DbOutageCoordinator.LogError = (_, __) => { };
            DbOutageCoordinator.DisconnectSessions = _ => { };
            DbOutageCoordinator.DisconnectGuests = _ => { };

             
            var ex = Record.Exception(() =>
                DbOutageCoordinator.Trip(new Exception("DB down"))
            );

             
            Assert.Null(ex);
        }

        [Fact]
        public void TripWhenDisconnectSessionsThrowsDoesNotThrow()
        {
             
            DbOutageCoordinator.ResetForTests();

            DbOutageCoordinator.DisconnectSessions =
                _ => throw new Exception("Session error");

            DbOutageCoordinator.TelegramSender = _ => { };
            DbOutageCoordinator.DisconnectGuests = _ => { };
            DbOutageCoordinator.LogError = (_, __) => { };

             
            var ex = Record.Exception(() =>
                DbOutageCoordinator.Trip(new Exception("DB down"))
            );

             
            Assert.Null(ex);
        }

        [Fact]
        public void TripWhenInnerExceptionOccursLogsError()
        {
             
            DbOutageCoordinator.ResetForTests();

            bool errorLogged = false;

            DbOutageCoordinator.LogError = (_, __) => errorLogged = true;
            DbOutageCoordinator.TelegramSender =
                _ => throw new Exception("Telegram fail");

            DbOutageCoordinator.DisconnectSessions = _ => { };
            DbOutageCoordinator.DisconnectGuests = _ => { };

             
            DbOutageCoordinator.Trip(new Exception("DB down"));

             
            Assert.True(errorLogged);
        }

    }
}
