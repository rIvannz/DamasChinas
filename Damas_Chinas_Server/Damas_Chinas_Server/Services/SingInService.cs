using System;
using System.Linq;
using System.Threading.Tasks;
using Damas_Chinas_Server.Utilidades;

namespace Damas_Chinas_Server
{
    public class SingInService : ISingInService
    {
        public ResultadoOperacion CrearUsuario(string nombre, string apellido, string correo, string password, string username)
        {
            var resultado = new ResultadoOperacion();

            try
            {
                // --- Validaciones ---
                Validator.ValidarNombre(nombre);
                Validator.ValidarNombre(apellido);
                Validator.ValidarCorreo(correo);
                Validator.ValidarUsername(username);
                Validator.ValidarPassword(password);

                // --- Crear usuario ---
                var repo = new RepositorioUsuarios();
                var usuario = repo.CrearUsuario(nombre, apellido, correo, password, username);

                var perfil = usuario.perfiles.FirstOrDefault();

                resultado.Exito = true;
                resultado.Mensaje = "Usuario creado correctamente.";
                resultado.Usuario = new UsuarioInfo
                {
                    IdUsuario = usuario.id_usuario,
                    Username = perfil?.username ?? username,
                    Correo = usuario.correo,
                    NombreCompleto = perfil != null
                        ? $"{perfil.nombre} {perfil.apellido_materno}"
                        : $"{nombre} {apellido}"
                };

                // --- Enviar correo de bienvenida en segundo plano ---
                Task.Run(async () =>
                {
                    try
                    {
                        string asunto = "Bienvenido a Damas Chinas";
                        string cuerpo = $"Hola {resultado.Usuario.NombreCompleto},<br><br>¡Gracias por registrarte en Damas Chinas! Tu usuario es <b>{resultado.Usuario.Username}</b>.<br><br>Disfruta jugando!";
                        await Correo.EnviarAsync(correo, asunto, cuerpo, html: true);
                    }
                    catch
                    {
                        // Opcional: log del error, no bloquea al usuario
                    }
                });
            }
            catch (Exception ex)
            {
                resultado.Exito = false;
                resultado.Mensaje = $"Error al crear usuario: {ex.Message}";
                resultado.Usuario = null;
            }

            return resultado;
        }
    }
}
