using System;
using System.ServiceModel;
using Damas_Chinas_Server;

namespace Host
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ServiceHost loginHost = null;
            ServiceHost signInHost = null;

            try
            {
                loginHost = new ServiceHost(typeof(LoginService));
                loginHost.Open();
                Console.WriteLine("✅ LoginService está listo en http://localhost:8733/LoginService/");

                signInHost = new ServiceHost(typeof(SingInService));
                signInHost.Open();
                Console.WriteLine("✅ SingInService está listo en net.tcp://localhost:8091/SingInService/");

                Console.WriteLine("\nPresione [Enter] para cerrar los servicios...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al iniciar los servicios: {ex.Message}");
            }
            finally
            {
                if (loginHost?.State == CommunicationState.Opened)
                    loginHost.Close();
                if (signInHost?.State == CommunicationState.Opened)
                    signInHost.Close();
            }
        }
    }
}
