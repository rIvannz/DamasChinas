using Damas_Chinas_Server.Utilidades;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Damas_Chinas_Server
{
    public class SingInService : ISingInService
    {
        private readonly RepositoryUsers _repositorio;

        public SingInService()
        {
            _repositorio = new RepositoryUsers();
        }

        public OperationResult CreateUser(string name, string lastName, string email, string password, string username)
        {
            var _result = new OperationResult();

            try
            {
                var _user = _repositorio.CreateUser(name, lastName, email, password, username);
                var _profile = _user.perfiles.FirstOrDefault();

                _result.Succes = true;
                _result.Messaje = "Usuario creado correctamente.";
                _result.User = new UserInfo
                {
                    IdUser = _user.id_usuario,
                    Username = _profile?.username ?? username,
                    Email = _user.correo,
                    FullName = _profile != null
                        ? $"{_profile.nombre} {_profile.apellido_materno}"
                        : $"{name} {lastName}"
                };

                // --- Enviar correo de bienvenida en segundo plano ---
                Task.Run(async () =>
                {
                    try
                    {
                        string subject = "Bienvenido a Damas Chinas";
                        string body = $"Hola {_result.User.FullName},<br><br>¡Gracias por registrarte en Damas Chinas! Tu usuario es <b>{_result.User.Username}</b>.<br><br>Disfruta jugando!";
                        await Correo.SendAsync(email, subject, body, html: true);
                    }
                    catch
                    {
                    }
                });
            }
            catch (Exception ex)
            {
                _result.Succes = false;
                _result.Messaje = $"Error al crear usuario: {ex.Message}";
                _result.User = null;
            }

            return _result;
        }
    }
}
