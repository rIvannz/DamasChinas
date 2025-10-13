using Damas_Chinas_Server;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace DamasChinasHost
{
    class Program
    {
        static void Main(string[] args)
        {
            // Crear URIs base para cada servicio
            Uri loginBaseAddress = new Uri("http://localhost:8739/LoginService/");
            Uri signInBaseAddress = new Uri("http://localhost:8736/SignInService/");
            Uri accountManagerBaseAddress = new Uri("http://localhost:8735/AccountManager/");

            // Crear hosts individuales para cada servicio
            using (ServiceHost loginHost = new ServiceHost(typeof(LoginService), loginBaseAddress))
            using (ServiceHost signInHost = new ServiceHost(typeof(SingInService), signInBaseAddress))
            using (ServiceHost accountHost = new ServiceHost(typeof(AccountManager), accountManagerBaseAddress))
            {
                try
                {
                    // =========================
                    // LOGIN SERVICE
                    // =========================
                    loginHost.AddServiceEndpoint(typeof(IILoginService), new BasicHttpBinding(), "");
                    var loginMetadata = new ServiceMetadataBehavior
                    {
                        HttpGetEnabled = true,
                        HttpGetUrl = loginBaseAddress
                    };
                    loginHost.Description.Behaviors.Add(loginMetadata);

                    // =========================
                    // SIGN IN SERVICE
                    // =========================
                    signInHost.AddServiceEndpoint(typeof(ISingInService), new BasicHttpBinding(), "");
                    var signInMetadata = new ServiceMetadataBehavior
                    {
                        HttpGetEnabled = true,
                        HttpGetUrl = signInBaseAddress
                    };
                    signInHost.Description.Behaviors.Add(signInMetadata);

                    // =========================
                    // ACCOUNT MANAGER SERVICE
                    // =========================
                    accountHost.AddServiceEndpoint(typeof(IAccountManager), new BasicHttpBinding(), "");
                    var accountMetadata = new ServiceMetadataBehavior
                    {
                        HttpGetEnabled = true,
                        HttpGetUrl = accountManagerBaseAddress
                    };
                    accountHost.Description.Behaviors.Add(accountMetadata);

                    // Abrir todos los servicios
                    loginHost.Open();
                    signInHost.Open();
                    accountHost.Open();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✅ Servicios WCF levantados correctamente:\n");
                    Console.ResetColor();

                    Console.WriteLine($"➡ LoginService: {loginBaseAddress}");
                    Console.WriteLine($"➡ SignInService: {signInBaseAddress}");
                    Console.WriteLine($"➡ AccountManager: {accountManagerBaseAddress}");
                    Console.WriteLine("\nPresiona <Enter> para detener los servicios...");
                    Console.ReadLine();

                    // Cerrar servicios
                    loginHost.Close();
                    signInHost.Close();
                    accountHost.Close();
                }
                catch (CommunicationException ce)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Error al iniciar los servicios: {0}", ce.Message);
                    Console.ResetColor();

                    loginHost.Abort();
                    signInHost.Abort();
                    accountHost.Abort();
                }
            }
        }
    }
}
