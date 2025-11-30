using System;
using System.Collections.Concurrent;
using System.Linq;
using System.ServiceModel;
using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;

namespace DamasChinas_Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ChatService : IChatService
    {
        private static readonly ConcurrentDictionary<string, IChatCallback> Clients =
            new ConcurrentDictionary<string, IChatCallback>();

        private readonly ChatRepository _repo;
        private readonly ILogService _log;

 
        private const string OperationRegistrateClient = nameof(RegistrateClient);
        private const string OperationSendMessage = nameof(SendMessage);
        private const string OperationSendMessage_SaveMessage = OperationSendMessage + ".SaveMessage";
        private const string OperationSendMessage_DeliverToClient = OperationSendMessage + ".DeliverToClient";

        private const string OperationGetHistoricalMessages = nameof(GetHistoricalMessages);


        public ChatService()
            : this(new ChatRepository(), LogFactory.Create<ChatService>())
        {
        }

        internal ChatService(ChatRepository repo, ILogService log)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public void RegistrateClient(string username)
        {
            ExecuteOperation(() =>
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    _log.Warn($"[{OperationRegistrateClient}] Username vacío o nulo.");
                    return;
                }

                var callback = OperationContext.Current.GetCallbackChannel<IChatCallback>();
                string key = username.Trim().ToLower();

                Clients[key] = callback;

                _log.Info($"[{OperationRegistrateClient}] Registrado: {key}");
            }, OperationRegistrateClient);
        }

        public void SendMessage(Message message)
        {
            ExecuteOperation(() =>
            {
                if (message == null)
                {
                    _log.Warn($"[{OperationSendMessage}] Se intentó enviar un mensaje nulo.");
                    return;
                }

                string destinationKey = message.DestinationUsername?.Trim().ToLower();
                string senderUsername = message.UsarnameSender;
                string text = message.Text;

                if (string.IsNullOrWhiteSpace(destinationKey))
                {
                    _log.Warn($"[{OperationSendMessage}] DestinationUsername vacío o nulo.");
                    return;
                }

                _log.Info($"[{OperationSendMessage}] {senderUsername} → {destinationKey}");

                ExecuteOperation(
                    () =>
                    {
                        int idRecipient = _repo.GetIdByUsername(destinationKey);
                        _repo.SaveMessage(senderUsername, idRecipient, text);
                    },
                    OperationSendMessage_SaveMessage
                );

                if (Clients.TryGetValue(destinationKey, out var callback))
                {
                    ExecuteOperation(
                        () =>
                        {
                            _log.Info($"[{OperationSendMessage_DeliverToClient}] Enviando mensaje a {destinationKey}");
                            callback.ReceiveMessage(message);
                        },
                        OperationSendMessage_DeliverToClient
                    );
                }
                else
                {
                    _log.Warn($"[{OperationSendMessage}] Cliente '{destinationKey}' no conectado.");
                }

            }, OperationSendMessage);
        }

        public Message[] GetHistoricalMessages(string usernameSender, string usernameRecipient)
        {
            return ExecuteOperation(
                () =>
                {
                    _log.Info($"[{OperationGetHistoricalMessages}] {usernameSender} ↔ {usernameRecipient}");
                    return _repo.GetChatByUsername(usernameSender, usernameRecipient).ToArray();
                },
                OperationGetHistoricalMessages,
                Array.Empty<Message>()
            );
        }

        // ============================
        //   HELPERS
        // ============================

        private void ExecuteOperation(Action action, string context)
        {
            try
            {
                _log.Info($"[{context}] START");
                action();
                _log.Info($"[{context}] SUCCESS");
            }
            catch (ArgumentException ex)
            {
                _log.Warn($"[{context}] ArgumentException: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                _log.Warn($"[{context}] InvalidOperationException: {ex.Message}");
            }
            catch (CommunicationException ex)
            {
                _log.Warn($"[{context}] CommunicationException: {ex.Message}");
            }
            catch (Exception ex)
            {
                _log.Error($"[{context}] Unexpected exception: {ex.Message}", ex);
            }
        }

        private T ExecuteOperation<T>(Func<T> func, string context, T defaultValue)
        {
            try
            {
                _log.Info($"[{context}] START");
                var result = func();
                _log.Info($"[{context}] SUCCESS");
                return result;
            }
            catch (ArgumentException ex)
            {
                _log.Warn($"[{context}] ArgumentException: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                _log.Warn($"[{context}] InvalidOperationException: {ex.Message}");
            }
            catch (CommunicationException ex)
            {
                _log.Warn($"[{context}] CommunicationException: {ex.Message}");
            }
            catch (Exception ex)
            {
                _log.Error($"[{context}] Unexpected exception: {ex.Message}", ex);
            }

            return defaultValue;
        }
    }
}
