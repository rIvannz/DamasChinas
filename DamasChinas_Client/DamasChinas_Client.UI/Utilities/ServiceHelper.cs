using System.ServiceModel;

namespace DamasChinas_Client.UI.Utilities
{
	public static class ServiceHelper
	{
		/// <summary>
		/// Cierra un cliente WCF de forma segura.
		/// </summary>
		/// <param name="client">Cliente de comunicación a cerrar.</param>
		public static void SafeClose(ICommunicationObject client)
		{
			if (client == null)
			{
				return;
			}

			try
			{
				if (client.State == CommunicationState.Faulted)
				{
					client.Abort();
				}
				else
				{
					client.Close();
				}
			}
			catch
			{
				client.Abort();
			}
		}
	}
}
