using DamasChinas_Server;
using DamasChinas_Server.Common;
using DamasChinas_Server.Services;
using DamasChinas_Server.Utilities;
using System;
using System.ServiceModel;

namespace DamasChinasHost
{
    internal static class Program
    {
        private static readonly ILogService _log =
            LogFactory.Create(typeof(Program));

        static void Main(string[] args)
        {
            ServiceHost[] hosts =
            {
                new ServiceHost(typeof(LoginService)),
                new ServiceHost(typeof(SingInService)),
                new ServiceHost(typeof(AccountManager)),
                new ServiceHost(typeof(ChatService)),
                new ServiceHost(typeof(FriendService)),
                new ServiceHost(typeof(LobbyService)),
                new ServiceHost(typeof(SessionService)),
                new ServiceHost(typeof(RankingService)),
                new ServiceHost(typeof(MatchService)),
                new ServiceHost(typeof(GuestSessionService)),
            };

            foreach (var host in hosts)
            {
                try
                {
                    host.Open();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($" {host.Description.ServiceType.Name} active.");
                }
                catch (AddressAlreadyInUseException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(
                        $"error on iniciar {host.Description.ServiceType.Name}: {ex.Message}");

                    _log.Error(
                        $"[Program] Error at start {host.Description.ServiceType.Name}",
                        ex);

                    host.Abort();
                }
                finally
                {
                    Console.ResetColor();
                }
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" DAMAS");
            Console.ResetColor();
            Console.WriteLine("Press enter to shutdown te server...");
            Console.ReadLine();

          
            foreach (var host in hosts)
            {
                try
                {
                    if (host.State == CommunicationState.Opened)
                    {
                        host.Close();
                    }
                    else
                    {
                        host.Abort();
                    }
                }
                catch
                {
                    host.Abort();
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nserver stoped succes.");
            Console.ResetColor();
        }
    }
}
