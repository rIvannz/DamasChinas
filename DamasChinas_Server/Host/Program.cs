using DamasChinas_Server;
using DamasChinas_Server.Services;
using DamasChinas_Server.Utilities;
using System;
using System.ServiceModel;

namespace DamasChinasHost
{
    internal static class Program
    {
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
            };

            foreach (var host in hosts)
            {
                try
                {
                    host.Open();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($" {host.Description.ServiceType.Name} activo.");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error al iniciar {host.Description.ServiceType.Name}: {ex.Message}");
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
            Console.WriteLine("detener...");
            Console.ReadLine();

            foreach (var host in hosts)
            {
                try
                {
                    if (host.State == CommunicationState.Opened)
                    {
                        host.Close();
                    }
                }
                catch
                {
                    host.Abort();
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n Servidor detenido correctamente.");
            Console.ResetColor();

        }
    }
}
