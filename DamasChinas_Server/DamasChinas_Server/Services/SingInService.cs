using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Services;
using DamasChinas_Server.Utilidades;
using DamasChinas_Server.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DamasChinas_Server
{
    public class SingInService : ISingInService
    {
        private static readonly Dictionary<string, (string Code, DateTime Created)> _codes =
            new Dictionary<string, (string Code, DateTime Created)>();

        private readonly RepositoryUsers _repository;

        public SingInService()
        {
            _repository = new RepositoryUsers();
        }

     
        public OperationResult ValidateUserData(UserDto userDto)
        {
            try
            {
                _repository.ValidateCreateUser(userDto);

                System.Diagnostics.Debug.WriteLine("[TRACE] User validation successful.");

                return new OperationResult
                {
                    Success = true,
                    Code = MessageCode.Success,
                    TechnicalDetail = "Validation completed."
                };
            }
            catch (RepositoryValidationException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Repo validation failed: {ex.Code}");

                return new OperationResult
                {
                    Success = false,
                    Code = ex.Code,
                    TechnicalDetail = $"Repository validation error: {ex.Code}"
                };
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Argument validation error: {ex.Message}");

                return new OperationResult
                {
                    Success = false,
                    Code = MessageCode.UserValidationError,
                    TechnicalDetail = "Argument validation failure."
                };
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FATAL] SQL error in ValidateUserData: {ex.Message}");

                return new OperationResult
                {
                    Success = false,
                    Code = MessageCode.ServerUnavailable,
                    TechnicalDetail = $"SQL error: {ex.Number}"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FATAL] Unexpected error in ValidateUserData: {ex.Message}");

                return new OperationResult
                {
                    Success = false,
                    Code = MessageCode.UnknownError,
                    TechnicalDetail = "Unexpected exception"
                };
            }
        }


       
        public OperationResult RequestVerificationCode(string email)
        {
            try
            {
                var code = GenerateCode();

                lock (_codes)
                {
                    _codes[email] = (code, DateTime.Now);
                }

                EmailSender.SendVerificationEmail(email, code);

                var result = OperationResult.Ok();
                result.Code = MessageCode.CodeSentSuccessfully;
                result.TechnicalDetail = "Verification code generated.";

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to send verification code: {ex.Message}");

                return new OperationResult
                {
                    Success = false,
                    Code = MessageCode.VerificationCodeSendError,
                    TechnicalDetail = "Email sending failure."
                };
            }
        }


   
        public OperationResult CreateUser(UserDto userDto, string code)
        {
            var result = new OperationResult();

            try
            {
                _repository.ValidateCreateUser(userDto);

                string storedCode;
                DateTime createdAt;

                lock (_codes)
                {
                    if (!_codes.TryGetValue(userDto.Email, out var data))
                    {
                        return new OperationResult
                        {
                            Success = false,
                            Code = MessageCode.VerificationCodeNotFound,
                            TechnicalDetail = "Code not found."
                        };
                    }

                    storedCode = data.Code;
                    createdAt = data.Created;
                }

                if (DateTime.UtcNow - createdAt > TimeSpan.FromMinutes(5))
                {
                    lock (_codes)
                    {
                        _codes.Remove(userDto.Email);
                    }

                    return new OperationResult
                    {
                        Success = false,
                        Code = MessageCode.VerificationCodeExpired,
                        TechnicalDetail = "Code expired."
                    };
                }

                if (!string.Equals(storedCode, code, StringComparison.Ordinal))
                {
                    return new OperationResult
                    {
                        Success = false,
                        Code = MessageCode.VerificationCodeInvalid,
                        TechnicalDetail = "Invalid code."
                    };
                }

                lock (_codes)
                {
                    _codes.Remove(userDto.Email);
                }

                var user = _repository.CreateUser(userDto);
                result = OperationResult.Ok();

                SendWelcomeEmail(MapToUserInfo(user, userDto));
            }
            catch (ArgumentException ex)
            {
                result.Success = false;
                result.Code = MessageCode.UserValidationError;
                result.TechnicalDetail = "Argument validation failure.";
                System.Diagnostics.Debug.WriteLine($"[ERROR] Argument exception: {ex.Message}");
            }
            catch (SqlException ex)
            {
                result.Success = false;
                result.Code = MessageCode.ServerUnavailable;
                result.TechnicalDetail = $"SQL error: {ex.Number}";
                System.Diagnostics.Debug.WriteLine($"[FATAL] SQL exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Code = MessageCode.UnknownError;
                result.TechnicalDetail = "Unexpected exception.";
                System.Diagnostics.Debug.WriteLine($"[FATAL] Unexpected exception: {ex.Message}");
            }

            return result;
        }


        private void SendWelcomeEmail(UserInfo user)
        {
            Task.Run(async () =>
            {
                try
                {
                    await Email.SendWelcomeAsync(user).ConfigureAwait(false);
                    System.Diagnostics.Debug.WriteLine("[TRACE] Welcome email sent successfully.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to send welcome email: {ex.Message}");
                }
            });
        }

        private static string GenerateCode()
        {
            var random = new Random();
            return random.Next(1000, 10000).ToString();
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
