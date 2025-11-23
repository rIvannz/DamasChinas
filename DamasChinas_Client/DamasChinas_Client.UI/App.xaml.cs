using DamasChinas_Client.UI.Utilities;
using System.Windows;

namespace DamasChinas_Client.UI
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
            bool enableMusic = true;

            if (enableMusic)
            {
                SoundManager.Initialize();
            }
        }
	}
}
