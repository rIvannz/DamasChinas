using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;


namespace Damas_Chinas_Server
{
    public class SingInService : ISingInService
    {
        public ResultadoOperacion CrearUsuario(string nombre, string apellido, string correo, string password, string username)
        {
            var resultado = new ResultadoOperacion();

            try
            {
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

