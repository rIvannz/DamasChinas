using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Utilidades;

namespace DamasChinas_Server
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

        public void ValidateCreateUser(UserDto userDto)
        {
            Validator.ValidateUserDto(userDto);

            ExecuteInContext(db =>
            {
                if (EntityExists<usuarios>(db, u => u.correo == userDto.Email))
                {
                    throw new RepositoryValidationException(MessageCode.UserDuplicateEmail);
                }

                if (EntityExists<perfiles>(db, p => p.username.Equals(userDto.Username, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new RepositoryValidationException(MessageCode.UsernameExists);
                }

                return true;
            });
        }

        public usuarios CreateUser(UserDto userDto)
        {
            return ExecuteInContext(db =>
            {
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
                    throw new RepositoryValidationException(MessageCode.LoginInvalidCredentials);
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
                throw new RepositoryValidationException(MessageCode.UsernameEmpty);
            }

            return ExecuteInContext(db =>
            {
                if (EntityExists<perfiles>(db, p => p.username.Equals(newUsername, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new RepositoryValidationException(MessageCode.UsernameExists);
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
                {
                    throw new RepositoryValidationException(MessageCode.UserNotFound);
                }

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

                var perfil = GetPerfilByUsername(db, username);
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

        private static bool EntityExists<T>(damas_chinasEntities db, Expression<Func<T, bool>> predicate)
            where T : class
        {
            return db.Set<T>().Any(predicate);
        }

        private static perfiles GetPerfilByUsername(damas_chinasEntities db, string username)
        {
            var perfil = db.perfiles.SingleOrDefault(
                p => p.username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (perfil == null)
            {
                throw new RepositoryValidationException(MessageCode.UserProfileNotFound);
            }

            return perfil;
        }

        private static usuarios GetUserWithProfile(damas_chinasEntities db, int idUsuario)
        {
            var user = db.usuarios
                         .Include(u => u.perfiles)
                         .SingleOrDefault(u => u.id_usuario == idUsuario);

            if (user == null)
            {
                throw new RepositoryValidationException(MessageCode.UserNotFound);
            }

            return user;
        }

        private static PublicProfile BuildPublicProfile(usuarios user)
        {
            if (user == null)
            {
                throw new RepositoryValidationException(MessageCode.UserNotFound);
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


        // aca le quite el return de void y el tipo perfiles.
        private static void CreatePerfil(damas_chinasEntities dataBase, usuarios usuario, UserDto userDto)
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

            dataBase.perfiles.Add(perfil);
            SaveChangesSafely(dataBase);
        }

        private static void SaveChangesSafely(damas_chinasEntities db)
        {
            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException)
            {
                throw new RepositoryValidationException(MessageCode.UnknownError);
            }
        }
    }
}

