using Damas_Chinas_Server;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using Damas_Chinas_Server.Services;       
using Damas_Chinas_Server.Interfaces;


namespace DamasChinasHost
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri loginBaseAddress = new Uri("http://localhost:8739/LoginService/");
            Uri signInBaseAddress = new Uri("http://localhost:8736/SignInService/");
            Uri accountManagerBaseAddress = new Uri("http://localhost:8735/AccountManager/");
            Uri chatBaseAddress = new Uri("net.tcp://localhost:8755/ChatService/");
            Uri amistadBaseAddress = new Uri("http://localhost:8741/FriendService/"); 
            Uri lobbyBaseAddress = new Uri("net.tcp://localhost:8751/LobbyService/"); 

            using (ServiceHost loginHost = new ServiceHost(typeof(LoginService), loginBaseAddress))
            using (ServiceHost signInHost = new ServiceHost(typeof(SingInService), signInBaseAddress))
            using (ServiceHost accountHost = new ServiceHost(typeof(AccountManager), accountManagerBaseAddress))
            using (ServiceHost ChatHost = new ServiceHost(typeof(ChatService), chatBaseAddress))
            using (ServiceHost amistadHost = new ServiceHost(typeof(FriendService), amistadBaseAddress))
            using (ServiceHost lobbyHost = new ServiceHost(typeof(LobbyService), lobbyBaseAddress)) 
            {
                try
                {
                
                    loginHost.AddServiceEndpoint(typeof(IILoginService), new BasicHttpBinding(), "");
                    var loginMetadata = new ServiceMetadataBehavior { HttpGetEnabled = true, HttpGetUrl = loginBaseAddress };
                    loginHost.Description.Behaviors.Add(loginMetadata);

                    signInHost.AddServiceEndpoint(typeof(ISingInService), new BasicHttpBinding(), "");
                    var signInMetadata = new ServiceMetadataBehavior { HttpGetEnabled = true, HttpGetUrl = signInBaseAddress };
                    signInHost.Description.Behaviors.Add(signInMetadata);

                    
                    accountHost.AddServiceEndpoint(typeof(IAccountManager), new BasicHttpBinding(), "");
                    var accountMetadata = new ServiceMetadataBehavior { HttpGetEnabled = true, HttpGetUrl = accountManagerBaseAddress };
                    accountHost.Description.Behaviors.Add(accountMetadata);

                    var saludoBinding = new NetTcpBinding
                    {
                        Security = { Mode = SecurityMode.None },
                        ReceiveTimeout = TimeSpan.MaxValue
                    };

                    var saludoMetadata = new ServiceMetadataBehavior { HttpGetEnabled = false };
                    ChatHost.Description.Behaviors.Add(saludoMetadata);

                    ChatHost.AddServiceEndpoint(typeof(IChatService), saludoBinding, "");
                    ChatHost.AddServiceEndpoint(typeof(IMetadataExchange),
                        MetadataExchangeBindings.CreateMexTcpBinding(),
                        "mex");

                 
                    amistadHost.AddServiceEndpoint(typeof(IFriendService), new BasicHttpBinding(), "");
                    var amistadMetadata = new ServiceMetadataBehavior { HttpGetEnabled = true, HttpGetUrl = amistadBaseAddress };
                    amistadHost.Description.Behaviors.Add(amistadMetadata);

                    
                    var lobbyBinding = new NetTcpBinding
                    {
                        Security = { Mode = SecurityMode.None },
                        ReceiveTimeout = TimeSpan.MaxValue
                    };

                    var lobbyMetadata = new ServiceMetadataBehavior { HttpGetEnabled = false };
                    lobbyHost.Description.Behaviors.Add(lobbyMetadata);

                    lobbyHost.AddServiceEndpoint(typeof(ILobbyService), lobbyBinding, "");
                    lobbyHost.AddServiceEndpoint(typeof(IMetadataExchange),
                        MetadataExchangeBindings.CreateMexTcpBinding(),
                        "mex");

                    loginHost.Open();
                    signInHost.Open();
                    accountHost.Open();
                    ChatHost.Open();
                    amistadHost.Open();
                    lobbyHost.Open();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✅ Servicios WCF levantados correctamente:\n");
                    Console.ResetColor();

                    Console.WriteLine($"➡ LoginService: {loginBaseAddress}");
                    Console.WriteLine($"➡ SignInService: {signInBaseAddress}");
                    Console.WriteLine($"➡ AccountManager: {accountManagerBaseAddress}");
                    Console.WriteLine($"➡ MensajeriaService (NetTcp): {chatBaseAddress}");
                    Console.WriteLine($"➡ AmistadService: {amistadBaseAddress}");
                    Console.WriteLine($"➡ LobbyService (NetTcp): {lobbyBaseAddress}");
                    Console.WriteLine("\nPresiona <Enter> para detener los servicios...");
                    Console.ReadLine();

                    
                    loginHost.Close();
                    signInHost.Close();
                    accountHost.Close();
                    ChatHost.Close();
                    amistadHost.Close();
                    lobbyHost.Close();
                }
                catch (CommunicationException ce)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Error al iniciar los servicios: {0}", ce.Message);
                    Console.ResetColor();

                    loginHost.Abort();
                    signInHost.Abort();
                    accountHost.Abort();
                    ChatHost.Abort();
                    amistadHost.Abort();
                    lobbyHost.Abort();
                }
            }
        }
    }
}
