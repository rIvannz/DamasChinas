using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
namespace DamasChinas_Server
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
	public class ChatService : IChatService
	{

		private static readonly ConcurrentDictionary<string, IChatCallback> clients = new ConcurrentDictionary<string, IChatCallback>();

		private readonly ChatRepository _repo = new ChatRepository();

		public void RegistrateClient(string username)
		{
			var callback = OperationContext.Current.GetCallbackChannel<IChatCallback>();
			if (!clients.ContainsKey(username))
			{
				clients[username] = callback;
			}
		}

        public void SendMessage(Message message)
        {

            string idUserSender = message.UsarnameSender;
            int idUserRecipient = _repo.GetIdByUsername(message.DestinationUsername);

            _repo.SaveMessage(idUserSender, idUserRecipient, message.Text);

            if (clients.ContainsKey(message.DestinationUsername))
            {
                try
                {
                    clients[message.DestinationUsername].ReceiveMessage(message);
                }
                catch (CommunicationException ex)
                {
                    Debug.WriteLine($"[SendMessage] Error comunicando con el cliente '{message.DestinationUsername}': {ex.Message}");
                }
                catch (ObjectDisposedException ex)
                {
                    Debug.WriteLine($"[SendMessage] Canal cerrado para '{message.DestinationUsername}': {ex.Message}");
                }
 
            }
        }


        public Message[] GetHistoricalMessages(string usernameSender, string usernameRecipient)
		{
			return _repo.GetChatByUsername(usernameSender, usernameRecipient).ToArray();
		}

	}
}
