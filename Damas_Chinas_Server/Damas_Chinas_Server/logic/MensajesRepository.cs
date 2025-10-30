using System;
using System.Linq;

namespace Damas_Chinas_Server
{
	public class MensajesRepository
	{
		private readonly damas_chinasEntities _context = new damas_chinasEntities();

		public void GuardarMensaje(int remitenteId, int destinatarioId, string texto)
		{
			var mensaje = new mensajes
			{
				id_usuario_remitente = remitenteId,
				id_usuario_destino = destinatarioId,
				texto = texto,
				fecha_envio = DateTime.Now
			};

			_context.mensajes.Add(mensaje);
			_context.SaveChanges();
		}

		public IQueryable<mensajes> ObtenerHistorialPorId(int idUsuario1, int idUsuario2)
		{
			return _context.mensajes
				.Where(m => (m.id_usuario_remitente == idUsuario1 && m.id_usuario_destino == idUsuario2) ||
							(m.id_usuario_remitente == idUsuario2 && m.id_usuario_destino == idUsuario1))
				.OrderBy(m => m.fecha_envio);
		}

		public IQueryable<Mensaje> ObtenerHistorialPorUsername(int idUsuario, string usernameDestino)
		{
			var idDestino = _context.usuarios
				.Where(u => u.perfiles.Any(p => p.username.ToLower() == usernameDestino.ToLower()))
				.Select(u => u.id_usuario)
				.FirstOrDefault();

			if (idDestino == 0)
			{
				return Enumerable.Empty<Mensaje>().AsQueryable();
			}
			var mensajesQuery = _context.mensajes
				.Where(m =>
					(m.id_usuario_remitente == idUsuario && m.id_usuario_destino == idDestino) ||
					(m.id_usuario_remitente == idDestino && m.id_usuario_destino == idUsuario)
				)
				.OrderBy(m => m.fecha_envio)
				.Select(m => new Mensaje
				{
					IdUsuario = m.id_usuario_remitente,
					IdUsuarioDestino = m.id_usuario_destino,
					UsernameDestino = _context.usuarios
										.Where(u => u.id_usuario == m.id_usuario_destino)
										.Select(u => u.perfiles.FirstOrDefault().username)
										.FirstOrDefault(),
					Texto = m.texto,
					FechaEnvio = m.fecha_envio
				});

			return mensajesQuery;
		}

		public int ObtenerIdPorUsername(string username)
		{
			var usuario = _context.usuarios
				.FirstOrDefault(u => u.perfiles.Any(p => p.username == username));

			if (usuario == null)
			{
				throw new Exception($"No se encontró el usuario con username '{username}'");
			}
			return usuario.id_usuario;
		}
	}
}
