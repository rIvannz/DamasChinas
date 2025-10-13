using Damas_Chinas_Server.Dtos;
using System;
using System.Data.Entity;
using System.Linq;
using Damas_Chinas_Server.Utilidades;

namespace Damas_Chinas_Server
{
    public class AccountManager : IAccountManager
    {
        public PublicProfile ObtenerPerfilPublico(int idUsuario)
        {
            using (var db = new damas_chinasEntities())
            {
                var usuario = db.usuarios
                                .Include(u => u.perfiles)
                                .FirstOrDefault(u => u.id_usuario == idUsuario);

                if (usuario == null)
                    return null;

                var perfil = usuario.perfiles.FirstOrDefault();

                return new PublicProfile
                {
                    Username = perfil?.username ?? "N/A",
                    Nombre = perfil?.nombre ?? "N/A",
                    LastName = perfil?.apellido_materno ?? "N/A",
                    Telefono = perfil?.telefono ?? "N/A",
                    Correo = usuario.correo
                };
            }
        }

        public ResultadoOperacion CambiarUsername(int idUsuario, string nuevoUsername)
        {
            try
            {
                // --- Validación usando Validator ---
                Validator.ValidarUsername(nuevoUsername);

                using (var db = new damas_chinasEntities())
                {
                    // Verificar si el username ya existe
                    if (db.perfiles.Any(p => p.username == nuevoUsername))
                    {
                        return new ResultadoOperacion
                        {
                            Exito = false,
                            Mensaje = "El nombre de usuario ya está en uso.",
                            Usuario = null
                        };
                    }

                    var perfil = db.perfiles.FirstOrDefault(p => p.id_usuario == idUsuario);
                    if (perfil == null)
                    {
                        return new ResultadoOperacion
                        {
                            Exito = false,
                            Mensaje = "No se encontró el perfil del usuario.",
                            Usuario = null
                        };
                    }

                    perfil.username = nuevoUsername;
                    db.SaveChanges();

                    return new ResultadoOperacion
                    {
                        Exito = true,
                        Mensaje = "Nombre de usuario actualizado correctamente.",
                        Usuario = null
                    };
                }
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
                // --- Validación usando Validator ---
                Validator.ValidarPassword(nuevaPassword);

                using (var db = new damas_chinasEntities())
                {
                    var usuario = db.usuarios.FirstOrDefault(u => u.id_usuario == idUsuario);
                    if (usuario == null)
                    {
                        return new ResultadoOperacion
                        {
                            Exito = false,
                            Mensaje = "No se encontró el usuario.",
                            Usuario = null
                        };
                    }

                    usuario.password_hash = nuevaPassword;
                    db.SaveChanges();

                    return new ResultadoOperacion
                    {
                        Exito = true,
                        Mensaje = "Contraseña actualizada correctamente.",
                        Usuario = null
                    };
                }
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

