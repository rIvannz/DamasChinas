using System.ServiceModel;
using System.Windows;
using DamasChinas_Client.UI.Utilities;

namespace DamasChinas_Client.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);


            LanguageManager.ApplySavedLanguage();

            SoundManager.Initialize();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                if (ClientSession.IsLoggedIn && ClientSession.SessionClient != null)
                {
                    var client = ClientSession.SessionClient;

                    string username = ClientSession.CurrentProfile?.Username;

                    if (!string.IsNullOrWhiteSpace(username))
                    {
                        try
                        {
                            if (client.State == CommunicationState.Opened)
                            {
                                client.Unsubscribe(username);
                                client.Close();
                            }
                            else
                            {
                                client.Abort();
                            }
                        }
                        catch
                        {
                            client.Abort();
                        }
                    }
                }
            }
            catch
            {
            }

            base.OnExit(e);
        }
    }
}
