using Damas_Chinas_Server.Dtos;
using Damas_Chinas_Server.Utilidades;
using System;
using System.Data.Entity; // Necesario para Include
using System.Linq;
namespace Damas_Chinas_Server
{
    public class RepositoryUsers
    {
        // 🔹 Crea un usuario con su perfil correspondiente
        public usuarios CreateUser(
            string nombre,
            string apellido,
            string correo,
            string password,
            string username)
        {
            // Validaciones
            Validator.ValidateName(nombre);
            Validator.ValidateName(apellido);
            Validator.ValidateEmail(correo);
            Validator.ValidatePassword(password);
            Validator.ValidateUsername(username);

            using (var db = new damas_chinasEntities())
            {
                if (db.usuarios.Any(u => u.correo == correo))
                    throw new Exception("Ya existe un usuario con ese correo.");

                if (db.perfiles.Any(p => p.username == username))
                    throw new Exception("Ya existe un perfil con ese nombre de usuario.");

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

        public LoginResult GetLoginResult(string usuarioInput, string password)
        {
            if (string.IsNullOrWhiteSpace(usuarioInput) || string.IsNullOrWhiteSpace(password))
                throw new Exception("Usuario y contraseña son requeridos.");

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

        public PublicProfile GetPublicProfile(int idUsuario)
        {
            using (var db = new damas_chinasEntities())
            {
                var user = db.usuarios
                                .Include(u => u.perfiles)
                                .FirstOrDefault(u => u.id_usuario == idUsuario);

                if (user == null)
                    return null;

                var perfil = user.perfiles.FirstOrDefault();

                return new PublicProfile
                {
                    Username = perfil?.username ?? "N/A",
                    Name = perfil.nombre,
                    LastName = perfil.apellido_materno,
                    SocialUrl = perfil?.telefono ?? "N/A",
                    Email = user.correo
                };
            }
        }

        public bool ChangeUsername(int idUsuario, string NewUsername)
        {
            Validator.ValidateUsername(NewUsername);

            using (var db = new damas_chinasEntities())
            {
                if (db.perfiles.Any(p => p.username == NewUsername))
                    throw new Exception("El nombre de usuario ya está en uso.");

                var perfil = db.perfiles.FirstOrDefault(p => p.id_usuario == idUsuario);
                if (perfil == null)
                    throw new Exception("No se encontró el perfil del usuario.");

                perfil.username = NewUsername;

                db.SaveChanges();
                return true;
            }
        }

        public bool ChangePassword(int idUsuario, string newPassword)
        {
            Validator.ValidatePassword(newPassword);

            using (var db = new damas_chinasEntities())
            {
                var usuario = db.usuarios.FirstOrDefault(u => u.id_usuario == idUsuario);
                if (usuario == null)
                    throw new Exception("No se encontró el usuario.");

                usuario.password_hash = newPassword;

                db.SaveChanges();
                return true;
            }
        }
    }
}

