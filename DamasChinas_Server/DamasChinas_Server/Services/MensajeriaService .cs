using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Damas_Chinas_Server.Dtos;
using Damas_Chinas_Server;
namespace Damas_Chinas_Server
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
	public class MensajeriaService : IMensajeriaService
	{

		private static Dictionary<string, IMensajeriaCallback> clients = new Dictionary<string, IMensajeriaCallback>();

		private MensajesRepository _repo = new MensajesRepository();

		public void RegisterClient(string username)
		{
			var callback = OperationContext.Current.GetCallbackChannel<IMensajeriaCallback>();
			if (!clientes.ContainsKey(username))
			{
				clientes[username] = callback;
			}
		}

		public void SendMessage(Mensaje message)
		{

			int senderId = message.IdUsuario;
			int destinationId = _repo.ObtenerIdPorUsername(message.UsernameDestino);

			_repo.GuardarMensaje(senderId, destinationId, message.Texto);

			if (clients.ContainsKey(message.UsernameDestino))
			{
				try
				{
					clients[message.UsernameDestino].RecibirMensaje(message);
				}
				catch
				{
					clients.Remove(message.UsernameDestino);
				}
			}
		}

		public Mensaje[] GetMessageHistory(int userId, string destinationUsername)
		{
			return _repo.ObtenerHistorialPorUsername(userId, destinationUsername).ToArray();
		}
	}
}
