using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;

namespace DamasChinas_Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ChatService : IChatService
    {
        private static readonly ConcurrentDictionary<string, IChatCallback> Clients =
            new ConcurrentDictionary<string, IChatCallback>();

        private readonly ChatRepository _repo = new ChatRepository();

        public void RegistrateClient(string username)
        {
            var callback = OperationContext.Current.GetCallbackChannel<IChatCallback>();

            string key = username.Trim().ToLower();

            Clients[key] = callback;

            Debug.WriteLine($"[RegistrateClient] Registrado: {key}");
        }

        public void SendMessage(Message message)
        {
            if (message == null) return;

            string destinationKey = message.DestinationUsername?.Trim().ToLower();

            // Guarda en la BD
            string senderUsername = message.UsarnameSender;
            int idRecipient = _repo.GetIdByUsername(message.DestinationUsername.Trim().ToLower());
            _repo.SaveMessage(senderUsername, idRecipient, message.Text);

            if
                (destinationKey != null && Clients.TryGetValue(destinationKey, out var callback))
            {
                try
                {
                    Debug.WriteLine($"[ChatService] Enviando mensaje a {destinationKey}");
                    callback.ReceiveMessage(message);
                }
                catch (CommunicationException ex)
                {
                    Debug.WriteLine($"[SendMessage] Error comunicando con '{destinationKey}': {ex.Message}");
                }
                catch (ObjectDisposedException ex)
                {
                    Debug.WriteLine($"[SendMessage] Canal cerrado para '{destinationKey}': {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine($"[ChatService] Cliente '{destinationKey}' no conectado.");
            }
        }
        public Message[] GetHistoricalMessages(string usernameSender, string usernameRecipient)
        {
            try
            {
                return _repo.GetChatByUsername(usernameSender, usernameRecipient).ToArray();

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