using Damas_Chinas_Server.Dtos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Damas_Chinas_Server
{
	public class RepositorioAmistades
	{
		/// <summary>
		/// Obtiene la lista de amigos de un usuario, incluyendo su estado y avatar.
		/// </summary>
		/// <param name="idUsuario">ID del usuario base.</param>
		/// <returns>Lista de amigos en formato AmigoDto.</returns>
		public List<AmigoDto> ObtenerAmigos(int idUsuario)
		{
			using (var db = new damas_chinasEntities())
			{
				// 1️⃣ Obtener todas las relaciones de amistad del usuario
				var amistades = db.amistades
					.Include("usuarios.perfiles")
					.Include("usuarios1.perfiles")
					.Where(a => a.id_usuario1 == idUsuario || a.id_usuario2 == idUsuario)
					.ToList();

				if (amistades == null || amistades.Count == 0)
				{
					return new List<AmigoDto>();
				}
				// 2️⃣ Determinar los usuarios amigos (el opuesto de la relación)
				var amigos = amistades.Select(a =>
					a.id_usuario1 == idUsuario ? a.usuarios1 : a.usuarios
				).ToList();

				// 3️⃣ Mapear a AmigoDto (solo los datos necesarios para el cliente)
				var listaAmigos = amigos.Select(u =>
				{
					var perfil = u.perfiles.FirstOrDefault();
					return new AmigoDto
					{
						IdAmigo = u.id_usuario, // interno, no se expone al cliente
						Username = perfil?.username ?? "N/A",
						EnLinea = true,
						Avatar = perfil?.imagen_perfil ?? "default.png"
					};
				}).ToList();

				return listaAmigos;
			}
		}

		/// <summary>
		/// Agrega una nueva relación de amistad entre dos usuarios.
		/// </summary>
		public bool AgregarAmistad(int idUsuario1, int idUsuario2)
		{
			if (idUsuario1 == idUsuario2)
			{
				throw new Exception("Un usuario no puede agregarse a sí mismo.");
			}
			using (var db = new damas_chinasEntities())
			{
				// Verificar que ambos usuarios existan
				var existe1 = db.usuarios.Any(u => u.id_usuario == idUsuario1);
				var existe2 = db.usuarios.Any(u => u.id_usuario == idUsuario2);

				if (!existe1 || !existe2)
				{
					throw new Exception("Uno o ambos usuarios no existen.");
				}
				// Evitar duplicados
				bool yaSonAmigos = db.amistades.Any(a =>
					(a.id_usuario1 == idUsuario1 && a.id_usuario2 == idUsuario2) ||
					(a.id_usuario1 == idUsuario2 && a.id_usuario2 == idUsuario1)
				);

				if (yaSonAmigos)
				{
					throw new Exception("Estos usuarios ya son amigos.");
				}
				// Crear la amistad
				var nuevaAmistad = new amistades
				{
					id_usuario1 = idUsuario1,
					id_usuario2 = idUsuario2,
					fecha_amistad = DateTime.Now
				};

				db.amistades.Add(nuevaAmistad);
				db.SaveChanges();

				return true;
			}
		}

		/// <summary>
		/// Elimina una relación de amistad existente.
		/// </summary>
		public bool EliminarAmistad(int idUsuario1, int idUsuario2)
		{
			using (var db = new damas_chinasEntities())
			{
				var amistad = db.amistades.FirstOrDefault(a =>
					(a.id_usuario1 == idUsuario1 && a.id_usuario2 == idUsuario2) ||
					(a.id_usuario1 == idUsuario2 && a.id_usuario2 == idUsuario1)
				);

				if (amistad == null)
				{
					throw new Exception("No existe una amistad entre estos usuarios.");
				}
				db.amistades.Remove(amistad);
				db.SaveChanges();
				return true;
			}
		}
	}
}
