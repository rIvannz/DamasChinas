using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Logic;
using System;
using System.Collections.Concurrent;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;



namespace DamasChinas_Server
{
    [DbGuardBehavior]
    [ServiceBehavior(
       InstanceContextMode = InstanceContextMode.PerSession,
       ConcurrencyMode = ConcurrencyMode.Reentrant)]
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
                    _log.Warn($"[{OperationRegistrateClient}] null username.");
                    return;
                }

                var callback = OperationContext.Current.GetCallbackChannel<IChatCallback>();
                string key = username.Trim().ToLower();

                Clients[key] = callback;

                _log.Info($"[{OperationRegistrateClient}] Register: {key}");
            }, OperationRegistrateClient);
        }

        public void SendMessage(Message message)
        {
            ExecuteOperation(() =>
            {
                if (message == null)
                {
                    _log.Warn($"[{OperationSendMessage}] Try to send a nule message.");
                    return;
                }

                string destinationKey = message.DestinationUsername?.Trim().ToLower();
                string senderUsername = message.UsarnameSender;
                string text = message.Text;

                if (string.IsNullOrWhiteSpace(destinationKey))
                {
                    _log.Warn($"[{OperationSendMessage}] DestinationUsername null");
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
                            _log.Info($"[{OperationSendMessage_DeliverToClient}] Sent mesage to {destinationKey}");
                            callback.ReceiveMessage(message);
                        },
                        OperationSendMessage_DeliverToClient
                    );
                }
                else
                {
                    _log.Warn($"[{OperationSendMessage}] client '{destinationKey}' wasnt conected.");
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


        private void ExecuteOperation(Action action, string context)
        {
            try
            {
                _log.Info($"[{context}] START");
                action();
                _log.Info($"[{context}] SUCCESS");
            }
            catch (SqlException ex)
            {
                _log.Error($"[{context}] SQL ERROR {ex.Number}", ex);
            }
            catch (EntityException ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    _log.Error($"[{context}] SQL ERROR {sqlEx.Number}", sqlEx);
                }
                else
                {
                    _log.Error($"[{context}] ENTITY ERROR: {ex.Message}", ex);
                }
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
            catch (SqlException ex)
            {
                _log.Error($"[{context}] SQL ERROR {ex.Number}", ex);
            }
            catch (EntityException ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    _log.Error($"[{context}] SQL ERROR {sqlEx.Number}", sqlEx);
                }
                else
                {
                    _log.Error($"[{context}] ENTITY ERROR: {ex.Message}", ex);
                }
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

            return defaultValue;
        }
    }
}