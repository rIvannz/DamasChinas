using Damas_Chinas_Server;
using Damas_Chinas_Server.Services;
using System;
using System.ServiceModel;

namespace DamasChinasHost
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.Title = "DAMAS";

			ServiceHost[] hosts =
			{
				new ServiceHost(typeof(LoginService)),
				new ServiceHost(typeof(SingInService)),
				new ServiceHost(typeof(AccountManager)),
				new ServiceHost(typeof(ChatService)),
				new ServiceHost(typeof(FriendService)),
                new ServiceHost(typeof(LobbyService)),


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
