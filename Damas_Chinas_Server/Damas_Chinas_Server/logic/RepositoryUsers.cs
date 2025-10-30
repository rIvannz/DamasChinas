using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using Damas_Chinas_Server.Dtos;
using Damas_Chinas_Server.Utilidades;

namespace Damas_Chinas_Server
{
	public class RepositoryUsers
	{
		private readonly Func<damas_chinasEntities> _contextFactory;

		public RepositoryUsers()
			: this(DbContextFactory.Create)
		{
		}

		public RepositoryUsers(Func<damas_chinasEntities> contextFactory)
		{
			_contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
		}

		public usuarios CreateUser(UserDto userDto)
		{
			Validator.ValidateUserDto(userDto);

			return ExecuteInContext(db =>
			{
				if (EntityExists<usuarios>(db, u => u.correo == userDto.Email))
				{
					throw new InvalidOperationException("Ya existe un usuario con ese correo.");
				}
				if (EntityExists<perfiles>(db, p => p.username.Equals(userDto.Username, StringComparison.OrdinalIgnoreCase)))
				{
					throw new InvalidOperationException("Ya existe un perfil con ese nombre de usuario.");
				}
				var nuevoUsuario = CreateUsuario(db, userDto);
				CreatePerfil(db, nuevoUsuario, userDto);

				return GetUserWithProfile(db, nuevoUsuario.id_usuario);
			});
		}

		public PublicProfile Login(LoginRequest loginRequest)
		{
			Validator.ValidateLoginRequest(loginRequest);

			return ExecuteInContext(db =>
			{
				var user = FindUserForLogin(db, loginRequest.Username);

				if (user == null || user.password_hash != loginRequest.Password)
				{
					throw new UnauthorizedAccessException("Credenciales inválidas.");
				}
				return BuildPublicProfile(user);
			});
		}

		public PublicProfile GetPublicProfile(int idUsuario)
		{
			return ExecuteInContext(db =>
			{
				var user = GetUserWithProfile(db, idUsuario);
				return BuildPublicProfile(user);
			});
		}

		public bool ChangeUsername(string username, string newUsername)
		{
			Validator.ValidateUsername(newUsername);

			var currentUsername = username?.Trim();
			if (string.IsNullOrWhiteSpace(currentUsername))
			{
				throw new ArgumentException("El nombre de usuario original es inválido.", nameof(username));
			}
			return ExecuteInContext(db =>
			{
				if (EntityExists<perfiles>(db, p => p.username.Equals(newUsername, StringComparison.OrdinalIgnoreCase)))
				{
					throw new InvalidOperationException("El nombre de usuario ya está en uso.");
				}
				var perfil = GetPerfilByUsername(db, currentUsername);

				perfil.username = newUsername;
				db.Entry(perfil).Property(p => p.username).IsModified = true;
				SaveChangesSafely(db);

				return true;
			});
		}

        public bool ChangePassword(string username, string newPassword)
        {
            Validator.ValidatePassword(newPassword);

            return ExecuteInContext(db =>
            {
                var usuario = (from u in db.usuarios
                               join p in db.perfiles on u.id_usuario equals p.id_usuario
                               where p.username.ToLower() == username.ToLower()
                               select u).FirstOrDefault();

                if (usuario == null)
                    throw new InvalidOperationException("No se encontró el usuario con ese nombre de usuario.");

                usuario.password_hash = newPassword;
                SaveChangesSafely(db);
                return true;
            });
        }


        public int GetUserIdByUsername(string username)
		{
			Validator.ValidateUsername(username);

			return ExecuteInContext(db =>
			{
				var perfil = GetPerfilByUsername(db, username, "No se encontró ningún usuario con ese nombre");
				return perfil.id_usuario;
			});
		}

		private damas_chinasEntities CreateContext()
		{
			return _contextFactory();
		}

		private T ExecuteInContext<T>(Func<damas_chinasEntities, T> operation)
		{
			using (var db = CreateContext())
			{
				return operation(db);
			}
		}

		private static usuarios FindUserForLogin(damas_chinasEntities db, string credential)
		{
			return db.usuarios
					 .Include(u => u.perfiles)
					 .FirstOrDefault(u =>
						 u.correo == credential ||
						 u.perfiles.Any(p => p.username == credential));
		}

		private static bool EntityExists<T>(damas_chinasEntities db, Expression<Func<T, bool>> predicate) where T : class
		{
			return db.Set<T>().Any(predicate);
		}

		private static T GetEntityOrThrow<T>(damas_chinasEntities db, Expression<Func<T, bool>> predicate, string errorMessage) where T : class
		{
			var entity = db.Set<T>().SingleOrDefault(predicate);
			if (entity == null)
			{
				throw new InvalidOperationException(errorMessage);
			}
			return entity;
		}

		private static perfiles GetPerfilByUsername(damas_chinasEntities db, string username, string errorMessage = "No se encontró el perfil del usuario")
		{
			return GetEntityOrThrow<perfiles>(db,
				p => p.username.Equals(username, StringComparison.OrdinalIgnoreCase),
				errorMessage);
		}

		private static usuarios GetUserWithProfile(damas_chinasEntities db, int idUsuario)
		{
			var user = db.usuarios
						 .Include(u => u.perfiles)
						 .SingleOrDefault(u => u.id_usuario == idUsuario);

			if (user == null)
			{
				throw new InvalidOperationException("No se encontró el usuario especificado.");
			}
			return user;
		}

		private static PublicProfile BuildPublicProfile(usuarios user)
		{
			if (user == null)
			{
				throw new ArgumentNullException(nameof(user));
			}
			var perfil = user.perfiles.FirstOrDefault();

			return new PublicProfile
			{
				Username = perfil?.username ?? "N/A",
				Name = perfil?.nombre ?? "N/A",
				LastName = perfil?.apellido_materno ?? "N/A",
				SocialUrl = perfil?.url ?? "N/A",
				Email = user.correo
			};
		}

		private static usuarios CreateUsuario(damas_chinasEntities db, UserDto userDto)
		{
			var usuario = new usuarios
			{
				correo = userDto.Email,
				password_hash = userDto.Password,
				rol = "cliente",
				fecha_creacion = DateTime.UtcNow
			};

			db.usuarios.Add(usuario);
			SaveChangesSafely(db);
			return usuario;
		}

		private static perfiles CreatePerfil(damas_chinasEntities db, usuarios usuario, UserDto userDto)
		{
			var perfil = new perfiles
			{
				id_usuario = usuario.id_usuario,
				username = userDto.Username,
				nombre = userDto.Name,
				apellido_materno = userDto.LastName,
				url = string.Empty,
				imagen_perfil = null,
				ultimo_login = null
			};

			db.perfiles.Add(perfil);
			SaveChangesSafely(db);
			return perfil;
		}

		private static void SaveChangesSafely(damas_chinasEntities db)
		{
			try
			{
				db.SaveChanges();
			}
			catch (DbEntityValidationException ex)
			{
				foreach (var eve in ex.EntityValidationErrors)
				{
					Console.WriteLine($"Entidad: {eve.Entry.Entity.GetType().Name}");
					foreach (var ve in eve.ValidationErrors)
					{
						Console.WriteLine($"Propiedad: {ve.PropertyName}, Error: {ve.ErrorMessage}");
					}
				}

				throw;
			}
		}
	}
}
