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
        // =========================================================
        // CÓDIGOS DE VERIFICACIÓN (EN MEMORIA)
        // =========================================================

        private static readonly Dictionary<string, (string Code, DateTime CreatedUtc)>
            _codes = new Dictionary<string, (string Code, DateTime CreatedUtc)>();

        private readonly RepositoryUsers _repository;

        public SingInService()
        {
            _repository = new RepositoryUsers();
        }

        // =========================================================
        // 1) VALIDACIÓN DE DATOS – SIN CREAR USUARIO
        // =========================================================

        public OperationResult ValidateUserData(UserDto userDto)
        {
            try
            {
                _repository.ValidateCreateUser(userDto);

                System.Diagnostics.Debug.WriteLine("[TRACE] User validation successful.");

                return OperationResult.Ok();
            }
            catch (RepositoryValidationException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Repo validation failed: {ex.Code}");

                return OperationResult.Fail(
                    $"Repository validation error: {ex.Code}",
                    ex.Code
                );
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Argument validation error: {ex.Message}");

                return OperationResult.Fail(
                    "Argument validation failure.",
                    MessageCode.UserValidationError
                );
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FATAL] SQL error in ValidateUserData: {ex.Message}");

                return OperationResult.Fail(
                    $"SQL error: {ex.Number}",
                    MessageCode.ServerUnavailable
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FATAL] Unexpected error in ValidateUserData: {ex.Message}");

                return OperationResult.Fail(
                    "Unexpected exception.",
                    MessageCode.UnknownError
                );
            }
        }

        // =========================================================
        // 2) ENVÍO DE CÓDIGO DE VERIFICACIÓN
        // =========================================================

        public OperationResult RequestVerificationCode(string email)
        {
            try
            {
                var code = GenerateCode();

                lock (_codes)
                {
                    _codes[email] = (code, DateTime.UtcNow);
                }

                EmailSender.SendVerificationEmail(email, code);

                return new OperationResult
                {
                    Success = true,
                    Code = MessageCode.CodeSentSuccessfully,
                    TechnicalDetail = "Verification code generated."
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to send verification code: {ex.Message}");

                return OperationResult.Fail(
                    "Email sending failure.",
                    MessageCode.VerificationCodeSendError
                );
            }
        }

        // =========================================================
        // 3) CREACIÓN DE USUARIO FINAL
        // =========================================================

        public OperationResult CreateUser(UserDto userDto, string code)
        {
            try
            {
                // ========== CÓDIGO ALMACENADO ==========

                string storedCode;
                DateTime createdAtUtc;

                lock (_codes)
                {
                    if (!_codes.TryGetValue(userDto.Email, out var data))
                    {
                        return OperationResult.Fail(
                            "Code not found.",
                            MessageCode.VerificationCodeNotFound
                        );
                    }

                    storedCode = data.Code;
                    createdAtUtc = data.CreatedUtc;
                }

                // ========== EXPIRACIÓN ==========

                if (DateTime.UtcNow - createdAtUtc > TimeSpan.FromMinutes(5))
                {
                    RemoveStoredCode(userDto.Email);

                    return OperationResult.Fail(
                        "Code expired.",
                        MessageCode.VerificationCodeExpired
                    );
                }

                // ========== CÓDIGO INCORRECTO ==========

                if (!string.Equals(storedCode, code, StringComparison.Ordinal))
                {
                    return OperationResult.Fail(
                        "Invalid code.",
                        MessageCode.VerificationCodeInvalid
                    );
                }

                // ========== CREACIÓN DE USUARIO ==========

                RemoveStoredCode(userDto.Email);

                var user = _repository.CreateUser(userDto);

                SendWelcomeEmail(MapToUserInfo(user, userDto));

                return OperationResult.Ok();
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FATAL] SQL exception in CreateUser: {ex.Message}");

                return OperationResult.Fail(
                    $"SQL error: {ex.Number}",
                    MessageCode.ServerUnavailable
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FATAL] Unexpected exception in CreateUser: {ex.Message}");

                return OperationResult.Fail(
                    "Unexpected exception.",
                    MessageCode.UnknownError
                );
            }
        }

        // =========================================================
        // MÉTODOS PRIVADOS
        // =========================================================

        private static void RemoveStoredCode(string email)
        {
            lock (_codes)
            {
                if (_codes.ContainsKey(email))
                {
                    _codes.Remove(email);
                }
            }
        }

        private static string GenerateCode()
        {
            var random = new Random();
            return random.Next(1000, 10000).ToString();
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
