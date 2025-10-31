using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
namespace DamasChinas_Server
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
	public class ChatService : IChatService
	{

		private static readonly ConcurrentDictionary<string, IChatCallback> clientes = new ConcurrentDictionary<string, IChatCallback>();

		private ChatRepository _repo = new ChatRepository();

		public void RegistrateClient(string username)
		{
			var callback = OperationContext.Current.GetCallbackChannel<IChatCallback>();
			if (!clientes.ContainsKey(username))
			{
				clientes[username] = callback;
			}
		}

		public void SendMessage(Message message)
		{

			string idUserSender = message.UsarnameSender;
			int idUserRecipient = _repo.GetIdByUsername(message.DestinationUsername);

			_repo.SaveMessage(idUserSender, idUserRecipient, message.Text);

			if (clientes.ContainsKey(message.DestinationUsername))
			{
				try
				{
					clientes[message.DestinationUsername].ReceiveMessage(message);
				}
				catch
				{
				}
			}
		}

		public Message[] GetHistoricalMessages(string usernameSender, string usernameDestino)
		{
			return _repo.GetChatByUsername(usernameSender, usernameDestino).ToArray();
		}

	}
}
