using Damas_Chinas_Server.Dtos;
using Damas_Chinas_Server.Utilidades;
using System;
using System.Data.Entity; // Necesario para Include
using System.Linq;
namespace Damas_Chinas_Server
{
	public class RepositorioUsuarios
	{
		// 🔹 Crea un usuario con su perfil correspondiente
		public usuarios CrearUsuario(
			string nombre,
			string apellido,
			string correo,
			string password,
			string username)
		{
			// Validaciones
			Validator.ValidarNombre(nombre);
			Validator.ValidarNombre(apellido);
			Validator.ValidarCorreo(correo);
			Validator.ValidarPassword(password);
			Validator.ValidarUsername(username);

			using (var db = new damas_chinasEntities())
			{
				if (db.usuarios.Any(u => u.correo == correo))
				{
					throw new Exception("Ya existe un usuario con ese correo.");
				}
				if (db.perfiles.Any(p => p.username == username))
				{
					throw new Exception("Ya existe un perfil con ese nombre de usuario.");
				}
				var nuevoUsuario = new usuarios
				{
					correo = correo,
					password_hash = password,
					rol = "cliente",
					fecha_creacion = DateTime.Now
				};
				db.usuarios.Add(nuevoUsuario);
				db.SaveChanges(); // Guarda para obtener el id_usuario

				// Crear perfil asociado
				var nuevoPerfil = new perfiles
				{
					id_usuario = nuevoUsuario.id_usuario,
					username = username,
					nombre = nombre,
					apellido_materno = apellido,
					telefono = "",
					imagen_perfil = null,
					ultimo_login = null
				};
				db.perfiles.Add(nuevoPerfil);
				db.SaveChanges();

				nuevoUsuario.perfiles.Add(nuevoPerfil);
				return nuevoUsuario;
			}
		}

		public LoginResult ObtenerLoginResult(string usuarioInput, string password)
		{
			// Validación mínima
			if (string.IsNullOrWhiteSpace(usuarioInput) || string.IsNullOrWhiteSpace(password))
			{
				throw new Exception("Usuario y contraseña son requeridos.");
			}
			using (var db = new damas_chinasEntities())
			{
				var usuario = db.usuarios
					.Include(u => u.perfiles)
					.FirstOrDefault(u => u.correo == usuarioInput ||
										 u.perfiles.Any(p => p.username == usuarioInput));

				if (usuario != null && usuario.password_hash == password)
				{
					var perfil = usuario.perfiles.FirstOrDefault();
					return new LoginResult
					{
						IdUsuario = usuario.id_usuario,
						Username = perfil?.username,
						Success = true
					};
				}

				return new LoginResult
				{
					IdUsuario = 0,
					Username = null,
					Success = false
				};
			}
		}

		public PublicProfile ObtenerPerfilPublico(int idUsuario)
		{
			using (var db = new damas_chinasEntities())
			{
				var usuario = db.usuarios
								.Include(u => u.perfiles)
								.FirstOrDefault(u => u.id_usuario == idUsuario);

				if (usuario == null)
				{
					return null;
				}
				var perfil = usuario.perfiles.FirstOrDefault();

				return new PublicProfile
				{
					Username = perfil?.username ?? "N/A",
					Nombre = perfil.nombre,
					LastName = perfil.apellido_materno,
					Telefono = perfil?.telefono ?? "N/A",
					Correo = usuario.correo
				};
			}
		}

		public bool CambiarUsername(int idUsuario, string nuevoUsername)
		{
			Validator.ValidarUsername(nuevoUsername);

			using (var db = new damas_chinasEntities())
			{
				if (db.perfiles.Any(p => p.username == nuevoUsername))
				{
					throw new Exception("El nombre de usuario ya está en uso.");
				}
				var perfil = db.perfiles.FirstOrDefault(p => p.id_usuario == idUsuario);
				if (perfil == null)
				{
					throw new Exception("No se encontró el perfil del usuario.");
				}
				perfil.username = nuevoUsername;

				db.SaveChanges();
				return true;
			}
		}

		public bool CambiarPassword(int idUsuario, string nuevaPassword)
		{
			Validator.ValidarPassword(nuevaPassword);

			using (var db = new damas_chinasEntities())
			{
				var usuario = db.usuarios.FirstOrDefault(u => u.id_usuario == idUsuario);
				if (usuario == null)
				{
					throw new Exception("No se encontró el usuario.");
				}
				usuario.password_hash = nuevaPassword;

				db.SaveChanges();
				return true;
			}
		}
	}
}

