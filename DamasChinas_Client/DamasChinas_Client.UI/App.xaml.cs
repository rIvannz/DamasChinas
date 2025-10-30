using System.Windows;

namespace DamasChinas_Client.UI
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			// SoundManager.Initialize(); // inicia la música
		}
	}
}
