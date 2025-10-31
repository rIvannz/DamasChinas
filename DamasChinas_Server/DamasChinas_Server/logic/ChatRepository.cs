using System;
using System.Collections.Generic;
using System.Linq;

namespace DamasChinas_Server
{
	public class ChatRepository
	{
		public void SaveMessage(string usernameSender, int recipientId, string texto)
		{
			var senderId = GetIdByUsername(usernameSender);

			using (var context = new damas_chinasEntities())
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
			var idSender = GetIdByUsername(usernameSender);
			var idRecipient = GetIdByUsername(usernameRecipient);

			using (var context = new damas_chinasEntities())
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
				throw new ArgumentException("El nombre de usuario no puede estar vacío.");
			}
			using (var context = new damas_chinasEntities())
			{
				var userId = context.usuarios
					.Where(u => u.perfiles.Any(p => p.username.ToLower() == username.ToLower()))
					.Select(u => u.id_usuario)
					.FirstOrDefault();

				if (userId == 0)
				{
					throw new InvalidOperationException($"No se encontró el usuario con username '{username}'");
				}
				return userId;
			}
		}
	}
}
