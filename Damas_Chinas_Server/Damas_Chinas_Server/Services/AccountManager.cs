using Damas_Chinas_Server.Dtos;
using System;
using System.Data.Entity;

namespace Damas_Chinas_Server
{
    public class AccountManager : IAccountManager
    {
        private readonly RepositorioUsuarios _repositorio;

        public AccountManager()
        {
            _repositorio = new RepositorioUsuarios();
        }

        public PublicProfile ObtenerPerfilPublico(int idUsuario)
        {
            return _repositorio.ObtenerPerfilPublico(idUsuario);
        }

        public ResultadoOperacion CambiarUsername(int idUsuario, string nuevoUsername)
        {
            try
            {
                bool exito = _repositorio.CambiarUsername(idUsuario, nuevoUsername);
                return new ResultadoOperacion
                {
                    Exito = exito,
                    Mensaje = exito ? "Nombre de usuario actualizado correctamente." : "Error al actualizar el nombre de usuario.",
                    Usuario = null
                };
            }
            catch (Exception ex)
            {
                return new ResultadoOperacion
                {
                    Exito = false,
                    Mensaje = $"Error al actualizar el nombre de usuario: {ex.Message}",
                    Usuario = null
                };
            }
        }

        public ResultadoOperacion CambiarPassword(int idUsuario, string nuevaPassword)
        {
            try
            {
                bool exito = _repositorio.CambiarPassword(idUsuario, nuevaPassword);
                return new ResultadoOperacion
                {
                    Exito = exito,
                    Mensaje = exito ? "Contraseña actualizada correctamente." : "Error al actualizar la contraseña.",
                    Usuario = null
                };
            }
            catch (Exception ex)
            {
                return new ResultadoOperacion
                {
                    Exito = false,
                    Mensaje = $"Error al actualizar la contraseña: {ex.Message}",
                    Usuario = null
                };
            }
        }
    }
}

