using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.logic;
using DamasChinas_Server.Utilidades;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;

namespace DamasChinas_Server
{
    public class RepositoryUsers: IRepositoryUsers
    {
        private readonly Func<IApplicationDbContext> _contextFactory;

        private const string DefaultAvatarFile = "avatarIcon.png";

        public RepositoryUsers()
            : this(() => new damas_chinasEntities())
        {
        }

        public RepositoryUsers(Func<IApplicationDbContext> contextFactory)
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

                if (EntityExists<perfiles>(db,
                        p => p.username.Equals(userDto.Username, StringComparison.OrdinalIgnoreCase)))
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
                var nuevo = CreateUsuario(db, userDto);
                CreatePerfil(db, nuevo, userDto);

                return GetUserWithProfile(db, nuevo.id_usuario);
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

        public PublicFriendProfile GetFriendPublicProfile(string friendUsername)
        {
            if (string.IsNullOrWhiteSpace(friendUsername))
                throw new RepositoryValidationException(MessageCode.UsernameEmpty);

            return ExecuteInContext(db =>
            {
                var perfil = db.perfiles
                    .SingleOrDefault(p =>
                        p.username.Equals(friendUsername, StringComparison.OrdinalIgnoreCase));

                if (perfil == null)
                    throw new RepositoryValidationException(MessageCode.UserProfileNotFound);

                int idUser = perfil.id_usuario;

                int matchesPlayed = db.participantes_partida.Count(p => p.id_jugador == idUser);
                int wins = db.participantes_partida.Count(p => p.id_jugador == idUser && p.posicion_final == 1);
                int loses = db.participantes_partida.Count(p => p.id_jugador == idUser && p.posicion_final > 1);

                return new PublicFriendProfile
                {
                    Username = perfil.username,
                    Name = perfil.nombre,
                    LastName = perfil.apellido_materno,
                    SocialUrl = perfil.url,
                    AvatarFile = perfil.imagen_perfil,
                    MatchesPlayed = matchesPlayed,
                    Wins = wins,
                    Loses = loses
                };
            });
        }


        public bool ChangeUsername(string username, string newUsername)
        {
            Validator.ValidateUsername(newUsername);

            var current = username?.Trim();
            if (string.IsNullOrWhiteSpace(current))
            {
                throw new RepositoryValidationException(MessageCode.UsernameEmpty);
            }

            return ExecuteInContext(db =>
            {
                if (EntityExists<perfiles>(db,
                        p => p.username.Equals(newUsername, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new RepositoryValidationException(MessageCode.UsernameExists);
                }

                var perfil = GetPerfilByUsername(db, current);

                perfil.username = newUsername;
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

        public bool ChangePasswordbyemail (string email, string newPassword)
        {
            Validator.ValidatePassword(newPassword);

            return ExecuteInContext(db =>
            {
                var usuario = db.usuarios
                                .FirstOrDefault(u => u.correo.ToLower() == email.ToLower());

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

        public bool ChangeAvatar(int idUser, string avatarFile)
        {
            if (idUser <= 0)
            {
                throw new RepositoryValidationException(MessageCode.UserNotFound);
            }

            if (string.IsNullOrWhiteSpace(avatarFile))
            {
                throw new RepositoryValidationException(MessageCode.AvatarUpdateFailed);
            }

            return ExecuteInContext(db =>
            {
                var perfil = db.perfiles.SingleOrDefault(p => p.id_usuario == idUser);

                if (perfil == null)
                {
                    throw new RepositoryValidationException(MessageCode.UserProfileNotFound);
                }

                perfil.imagen_perfil = avatarFile;
                SaveChangesSafely(db);
                return true;
            });
        }
        public bool ChangeSocialUrl(int idUser, string socialUrl)
        {
            if (idUser <= 0)
                throw new RepositoryValidationException(MessageCode.UserNotFound);

            socialUrl = socialUrl?.Trim() ?? string.Empty;

            return ExecuteInContext(db =>
            {
                var perfil = db.perfiles.SingleOrDefault(p => p.id_usuario == idUser);

                if (perfil == null)
                    throw new RepositoryValidationException(MessageCode.UserProfileNotFound);

                perfil.url = socialUrl;

                SaveChangesSafely(db);
                return true;
            });
        }


        private static usuarios CreateUsuario(IApplicationDbContext db, UserDto userDto)
        {
            var usuario = new usuarios
            {
                correo = userDto.Email,
                password_hash = userDto.Password,
                rol = "cliente",
                fecha_creacion = DateTime.UtcNow,

                numero_reportes = 0,
                baneado_permanentemente = false,
                esta_sancionado = false,

                fecha_desbaneo = null
            };

            db.usuarios.Add(usuario);
            SaveChangesSafely(db);
            return usuario;
        }



        private static void CreatePerfil(IApplicationDbContext db, usuarios usuario, UserDto userDto)
        {
            var perfil = new perfiles
            {
                id_usuario = usuario.id_usuario,
                username = userDto.Username,
                nombre = userDto.Name,
                apellido_materno = userDto.LastName,
                url = string.Empty,
                imagen_perfil = DefaultAvatarFile,
                ultimo_login = null
            };

            db.perfiles.Add(perfil);
            SaveChangesSafely(db);
        }

        private static usuarios GetUserWithProfile(IApplicationDbContext db, int idUsuario)
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

        private static perfiles GetPerfilByUsername(IApplicationDbContext db, string username)
        {
            var perfil = db.perfiles.SingleOrDefault(
                p => p.username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (perfil == null)
            {
                throw new RepositoryValidationException(MessageCode.UserProfileNotFound);
            }

            return perfil;
        }

        private static usuarios FindUserForLogin(IApplicationDbContext db, string credential)
        {
            return db.usuarios
                .Include(u => u.perfiles)
                .FirstOrDefault(u =>
                    u.correo == credential ||
                    u.perfiles.Any(p => p.username == credential));
        }

        private static PublicProfile BuildPublicProfile(usuarios user)
        {
            var perfil = user.perfiles.FirstOrDefault();

            using (var db = new damas_chinasEntities())
            {
                int idUser = user.id_usuario;

                int matchesPlayed = db.participantes_partida
                    .Count(p => p.id_jugador == idUser);

                int wins = db.participantes_partida
                    .Count(p => p.id_jugador == idUser && p.posicion_final == 1);

                int loses = db.participantes_partida
                    .Count(p => p.id_jugador == idUser && p.posicion_final > 1);

                return new PublicProfile
                {
                    IdUser = idUser,
                    Username = perfil?.username ?? "N/A",
                    Name = perfil?.nombre ?? "N/A",
                    LastName = perfil?.apellido_materno ?? "N/A",
                    SocialUrl = perfil?.url ?? "N/A",
                    Email = user.correo,
                    AvatarFile = perfil?.imagen_perfil ?? "avatarIcon.png",

                    MatchesPlayed = matchesPlayed,
                    Wins = wins,
                    Loses = loses
                };
            }
        }


        private static bool EntityExists<T>(IApplicationDbContext db, Expression<Func<T, bool>> predicate)
            where T : class
        {
            return db.Set<T>().Any(predicate);
        }

        private static void SaveChangesSafely(IApplicationDbContext db)
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

        private T ExecuteInContext<T>(Func<IApplicationDbContext, T> operation)
        {
            using (var db = _contextFactory())
            {
                return operation(db);
            }


        }

        public string GetEmailByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new RepositoryValidationException(MessageCode.UsernameEmpty);
            }

            return ExecuteInContext(db =>
            {
                var result = (from u in db.usuarios
                              join p in db.perfiles on u.id_usuario equals p.id_usuario
                              where p.username.Equals(username, StringComparison.OrdinalIgnoreCase)
                              select u.correo)
                             .FirstOrDefault();

                if (result == null)
                {
                    throw new RepositoryValidationException(MessageCode.UserNotFound);
                }

                return result;
            });
        }

    }

}
