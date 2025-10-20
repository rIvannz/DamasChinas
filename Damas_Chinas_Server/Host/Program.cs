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
            Uri saludoBaseAddress = new Uri("net.tcp://localhost:8755/MensajeriaService/");
            Uri amistadBaseAddress = new Uri("http://localhost:8741/AmistadService/"); // CORREGIDO: puerto libre

            // Crear hosts individuales para cada servicio
            using (ServiceHost loginHost = new ServiceHost(typeof(LoginService), loginBaseAddress))
            using (ServiceHost signInHost = new ServiceHost(typeof(SingInService), signInBaseAddress))
            using (ServiceHost accountHost = new ServiceHost(typeof(AccountManager), accountManagerBaseAddress))
            using (ServiceHost saludoHost = new ServiceHost(typeof(MensajeriaService), saludoBaseAddress))
            using (ServiceHost amistadHost = new ServiceHost(typeof(AmistadService), amistadBaseAddress))




            {
                try
                {

                    // =========================
                    // LOGIN SERVICE
                    // =========================
                    loginHost.AddServiceEndpoint(typeof(IILoginService), new BasicHttpBinding(), "");
                    var loginMetadata = new ServiceMetadataBehavior { HttpGetEnabled = true, HttpGetUrl = loginBaseAddress };
                    loginHost.Description.Behaviors.Add(loginMetadata);

                    // =========================
                    // SIGN IN SERVICE
                    // =========================
                    signInHost.AddServiceEndpoint(typeof(ISingInService), new BasicHttpBinding(), "");
                    var signInMetadata = new ServiceMetadataBehavior { HttpGetEnabled = true, HttpGetUrl = signInBaseAddress };
                    signInHost.Description.Behaviors.Add(signInMetadata);

                    // =========================
                    // ACCOUNT MANAGER SERVICE
                    // =========================
                    accountHost.AddServiceEndpoint(typeof(IAccountManager), new BasicHttpBinding(), "");
                    var accountMetadata = new ServiceMetadataBehavior { HttpGetEnabled = true, HttpGetUrl = accountManagerBaseAddress };
                    accountHost.Description.Behaviors.Add(accountMetadata);

                    // =========================
                    // SALUDO SERVICE (NetTcp + Callback)
                    // =========================
                    var saludoBinding = new NetTcpBinding
                    {
                        Security = { Mode = SecurityMode.None },
                        ReceiveTimeout = TimeSpan.MaxValue
                    };

                    // Crear el comportamiento de metadata
                    var saludoMetadata = new ServiceMetadataBehavior { HttpGetEnabled = false };
                    saludoHost.Description.Behaviors.Add(saludoMetadata);

                    // Agregar endpoint del servicio
                    saludoHost.AddServiceEndpoint(typeof(IMensajeriaService), saludoBinding, "");

                    // Agregar endpoint MEX TCP para que Visual Studio lo detecte
                    saludoHost.AddServiceEndpoint(
                        typeof(IMetadataExchange),
                        MetadataExchangeBindings.CreateMexTcpBinding(),
                        "mex" // Esto crea net.tcp://localhost:8755/MensajeriaService/mex
                    );

                    // =========================
                    // AMISTAD SERVICE (HTTP)
                    // =========================
                    amistadHost.AddServiceEndpoint(typeof(IAmistadService), new BasicHttpBinding(), "");
                    var amistadMetadata = new ServiceMetadataBehavior { HttpGetEnabled = true, HttpGetUrl = amistadBaseAddress };
                    amistadHost.Description.Behaviors.Add(amistadMetadata);

                    // Abrir todos los servicios
                    loginHost.Open();
                    signInHost.Open();
                    accountHost.Open();
                    saludoHost.Open();
                    amistadHost.Open();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✅ Servicios WCF levantados correctamente:\n");
                    Console.ResetColor();

                    Console.WriteLine($"➡ LoginService: {loginBaseAddress}");
                    Console.WriteLine($"➡ SignInService: {signInBaseAddress}");
                    Console.WriteLine($"➡ AccountManager: {accountManagerBaseAddress}");
                    Console.WriteLine($"➡ SaludoService (NetTcp): {saludoBaseAddress}");
                    Console.WriteLine($"➡ AmistadService: {amistadBaseAddress}");
                    Console.WriteLine("\nPresiona <Enter> para detener los servicios...");
                    Console.ReadLine();

                    // Cerrar servicios
                    loginHost.Close();
                    signInHost.Close();
                    accountHost.Close();
                    saludoHost.Close();
                    amistadHost.Close();
                }
                catch (CommunicationException ce)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Error al iniciar los servicios: {0}", ce.Message);
                    Console.ResetColor();

                    loginHost.Abort();
                    signInHost.Abort();
                    accountHost.Abort();
                    saludoHost.Abort();
                    amistadHost.Abort();
                }
            }
        }
    }
}
