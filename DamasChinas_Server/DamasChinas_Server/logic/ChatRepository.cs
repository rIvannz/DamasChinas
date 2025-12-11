using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DamasChinas_Server
{
    public class ChatRepository
    {
        private readonly Func<damas_chinasEntities> _dbFactory;

        public ChatRepository()
            : this(() => new damas_chinasEntities())
        {
        }

        public ChatRepository(Func<damas_chinasEntities> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public virtual damas_chinasEntities CreateDb()
        {
            return _dbFactory();
        }

        public void SaveMessage(string usernameSender, int recipientId, string texto)
        {
            var senderId = GetIdByUsername(usernameSender);

            using (var context = CreateDb())
            {
                var messageEntity = new mensajes
                {
                    id_usuario_remitente = senderId,
                    id_usuario_destino = recipientId,
                    texto = texto,
                    fecha_envio = DateTime.Now
                };

                context.mensajes.Add(messageEntity);
                context.SaveChanges();
            }
        }

        public List<Message> GetChatByUsername(string usernameSender, string usernameRecipient)
        {
            int idSender = GetIdByUsername(usernameSender);
            int idRecipient = GetIdByUsername(usernameRecipient);

            using (var context = CreateDb())
            {
                var mensajes = context.mensajes
                    .Where(m =>
                        (m.id_usuario_remitente == idSender && m.id_usuario_destino == idRecipient) ||
                        (m.id_usuario_remitente == idRecipient && m.id_usuario_destino == idSender)
                    )
                    .OrderBy(m => m.fecha_envio)
                    .Select(m => new Message
                    {
                        UsarnameSender = m.id_usuario_remitente == idSender ? usernameSender : usernameRecipient,
                        DestinationUsername = m.id_usuario_destino == idSender ? usernameSender : usernameRecipient,
                        Text = m.texto,
                        SendDate = m.fecha_envio
                    })
                    .ToList();

                return mensajes;
            }
        }

        public int GetIdByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException(MessageCode.RepositoryUsernameEmpty.ToString(), nameof(username));
            }

            using (var context = CreateDb())
            {
                int userId = context.usuarios
                    .Where(u => u.perfiles.Any(p => p.username.ToLower() == username.ToLower()))
                    .Select(u => u.id_usuario)
                    .FirstOrDefault();

                if (userId == 0)
                {
                    throw new InvalidOperationException(MessageCode.RepositoryUserNotFound.ToString());
                }

                return userId;
            }
        }


    }
}

