using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using DamasChinas_Server.Dtos;

namespace DamasChinas_Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ChatService : IChatService
    {
        private static readonly ConcurrentDictionary<string, IChatCallback> Clients =
            new ConcurrentDictionary<string, IChatCallback>();

        // Repositorio SOLO para métodos de instancia (ya GetIdByUsername es estático)
        private readonly ChatRepository _repo = new ChatRepository();

        public void RegistrateClient(string username)
        {
            var callback = OperationContext.Current.GetCallbackChannel<IChatCallback>();

            if (!Clients.ContainsKey(username))
            {
                Clients[username] = callback;
            }
        }

        public void SendMessage(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            try
            {
                string senderUsername = message.UsarnameSender;

                int recipientId = ChatRepository.GetIdByUsername(message.DestinationUsername);

                _repo.SaveMessage(senderUsername, recipientId, message.Text);

                if (Clients.TryGetValue(message.DestinationUsername, out IChatCallback callback))
                {
                    try
                    {
                        callback.ReceiveMessage(message);
                    }
                    catch (CommunicationException cex)
                    {
                        Debug.WriteLine(
                            $"[ChatService.SendMessage] Communication error with '{message.DestinationUsername}': {cex.Message}");
                    }
                    catch (ObjectDisposedException odex)
                    {
                        Debug.WriteLine(
                            $"[ChatService.SendMessage] Channel disposed for '{message.DestinationUsername}': {odex.Message}");
                    }
                }
            }
            catch (ArgumentException aex)
            {
                Debug.WriteLine($"[ChatService.SendMessage] Argument error: {aex.Message}");
            }
            catch (InvalidOperationException ioex)
            {
                Debug.WriteLine($"[ChatService.SendMessage] Invalid operation: {ioex.Message}");
            }
        }

        public Message[] GetHistoricalMessages(string usernameSender, string usernameRecipient)
        {
            try
            {
                var list = _repo.GetChatByUsername(usernameSender, usernameRecipient);
                return list.ToArray();
            }
            catch (ArgumentException aex)
            {
                Debug.WriteLine($"[ChatService.GetHistoricalMessages] Argument error: {aex.Message}");
                return Array.Empty<Message>();
            }
            catch (InvalidOperationException ioex)
            {
                Debug.WriteLine($"[ChatService.GetHistoricalMessages] Invalid operation: {ioex.Message}");
                return Array.Empty<Message>();
            }
        }
    }
}
