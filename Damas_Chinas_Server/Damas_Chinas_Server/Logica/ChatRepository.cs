using System;
using System.Linq;

namespace Damas_Chinas_Server
{
    public class ChatRepository
    {
        private readonly damas_chinasEntities _context = new damas_chinasEntities();

        public void SaveMessage(int senderId, int recipientId, string texto)
        {
            var messageEntity = new mensajes
            {
                id_usuario_remitente = senderId,
                id_usuario_destino = recipientId,
                texto = texto,
                fecha_envio = DateTime.Now
            };

            _context.mensajes.Add(messageEntity);
            _context.SaveChanges();
        }

        public IQueryable<mensajes> GetHistoricalMessages(int idUser1, int idUser2)
        {
            return _context.mensajes
                .Where(m => (m.id_usuario_remitente == idUser1 && m.id_usuario_destino == idUser2) ||
                            (m.id_usuario_remitente == idUser2 && m.id_usuario_destino == idUser1))
                .OrderBy(m => m.fecha_envio);
        }
        public IQueryable<Message> GetChatByUsername(int idUserSender, string usernameRecipient)
        {
            var idUserERecipient = _context.usuarios
                .Where(u => u.perfiles.Any(p => p.username.ToLower() == usernameRecipient.ToLower()))
                .Select(u => u.id_usuario)
                .FirstOrDefault();

            if (idUserERecipient == 0)
                return Enumerable.Empty<Message>().AsQueryable();

            var mensajesQuery = _context.mensajes
                .Where(m =>
                    (m.id_usuario_remitente == idUserSender && m.id_usuario_destino == idUserERecipient) ||
                    (m.id_usuario_remitente == idUserERecipient && m.id_usuario_destino == idUserSender)
                )
                .OrderBy(m => m.fecha_envio)
                .Select(m => new Message
                {
                    IdUser = m.id_usuario_remitente,
                    IdDestinationUsername = m.id_usuario_destino,
                    DestinationUsername = _context.usuarios
                                        .Where(u => u.id_usuario == m.id_usuario_destino)
                                        .Select(u => u.perfiles.FirstOrDefault().username)
                                        .FirstOrDefault(),
                    Text = m.texto,
                    SendDate = m.fecha_envio
                });

            return mensajesQuery;
        }




        public int GetIdByUsername(string username)
        {
            var user = _context.usuarios
                .FirstOrDefault(u => u.perfiles.Any(p => p.username == username));

            if (user == null)
                throw new Exception($"No se encontró el usuario con username '{username}'");

            return user.id_usuario;
        }

    }

}
