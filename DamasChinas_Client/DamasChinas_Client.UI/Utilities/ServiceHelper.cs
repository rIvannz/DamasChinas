using System.ServiceModel;

namespace DamasChinas_Client.UI.Utilities
{
	public static class ServiceHelper
	{
	
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
