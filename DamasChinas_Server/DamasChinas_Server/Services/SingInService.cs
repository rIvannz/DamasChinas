using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Services;
using DamasChinas_Server.Utilidades;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DamasChinas_Server
{

    public class SingInService : ISingInService
    {
        private readonly RepositoryUsers _repository;

        public SingInService()
        {
            _repository = new RepositoryUsers();
        }

  
        public OperationResult CreateUser(UserDto userDto)
        {
            var result = new OperationResult();

            try
            {
                var user = _repository.CreateUser(userDto);

                result.Success = true;
                result.Code = MessageCode.Success;
                result.TechnicalDetail = "User created successfully.";

                // Optional: send welcome email asynchronously
                SendWelcomeEmail(MapToUserInfo(user, userDto));

                System.Diagnostics.Debug.WriteLine("[TRACE] User created successfully.");
            }
            catch (ArgumentException ex)
            {
                result.Success = false;
                result.Code = MessageCode.UserDuplicateEmail;
                result.TechnicalDetail = ex.Message;
                System.Diagnostics.Debug.WriteLine($"[ERROR] Duplicate email during registration: {ex.Message}");
            }
            catch (SqlException ex)
            {
                result.Success = false;
                result.Code = MessageCode.ServerUnavailable;
                result.TechnicalDetail = ex.Message;
                System.Diagnostics.Debug.WriteLine($"[FATAL] Database connection failed during registration: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                result.Success = false;
                result.Code = MessageCode.UnknownError;
                result.TechnicalDetail = ex.Message;
                System.Diagnostics.Debug.WriteLine($"[ERROR] Invalid operation during registration: {ex.Message}");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Code = MessageCode.UnknownError;
                result.TechnicalDetail = ex.Message;
                System.Diagnostics.Debug.WriteLine($"[FATAL] Unexpected exception in CreateUser: {ex.Message}");
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("[TRACE] CreateUser operation finished.");
            }

            return result;
        }

        
        private void SendWelcomeEmail(UserInfo user)
        {
            Task.Run(async () =>
            {
                try
                {
                    await Correo.EnviarBienvenidaAsync(user).ConfigureAwait(false);
                    System.Diagnostics.Debug.WriteLine("[TRACE] Welcome email sent successfully.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to send welcome email: {ex.Message}");
                }
            });
        }

       
        private UserInfo MapToUserInfo(usuarios user, UserDto userDto)
        {
            var profile = user.perfiles.FirstOrDefault();

            return new UserInfo
            {
                IdUser = user.id_usuario,
                Username = profile?.username ?? userDto.Username,
                Email = user.correo,
                FullName = profile != null
                    ? $"{profile.nombre} {profile.apellido_materno}"
                    : $"{userDto.Name} {userDto.LastName}"
            };
        }
    }
}
