using System;
using System.Linq;
using System.Data.Entity; // Necesario para Include

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
            using (var db = new damas_chinasEntities())
            {
                // Verificar si ya existe el correo o username
                if (db.usuarios.Any(u => u.correo == correo))
                    throw new Exception("Ya existe un usuario con ese correo.");

                if (db.perfiles.Any(p => p.username == username))
                    throw new Exception("Ya existe un perfil con ese nombre de usuario.");

                // Crear usuario
                var nuevoUsuario = new usuarios
                {
                    correo = correo,
                    password_hash = password, // Guardar texto plano
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

        // 🔹 Obtener LoginResult con Eager Loading para evitar errores de contexto
        public LoginResult ObtenerLoginResult(string usuarioInput, string password)
        {
            using (var db = new damas_chinasEntities())
            {
                // Cargar perfiles con Include para evitar ObjectContext disposed
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

   
    }
}
