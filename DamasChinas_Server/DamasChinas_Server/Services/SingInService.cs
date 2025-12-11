using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Services;
using DamasChinas_Server.Utilidades;
using DamasChinas_Server.Utilities;
using DamasChinas_Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DamasChinas_Server
{
    public class SingInService : ISingInService
    {
        private static readonly Dictionary<string, (string Code, DateTime CreatedUtc)> _codes =
            new Dictionary<string, (string Code, DateTime CreatedUtc)>();

        private readonly RepositoryUsers _repository;
        private readonly ILogService _log;

      

        private const string OperationValidateUserData = nameof(ValidateUserData);
        private const string OperationRequestVerificationCode = nameof(RequestVerificationCode);
        private const string OperationCreateUser = nameof(CreateUser);
        private const string OperationGenerateCode = nameof(GenerateCode);
        private const string OperationSendWelcomeEmail = nameof(SendWelcomeEmail);
        private const string OperationRemoveStoredCode = nameof(RemoveStoredCode);


        public SingInService()
            : this(new RepositoryUsers(), LogFactory.Create<SingInService>())
        {
        }

        internal SingInService(RepositoryUsers repository, ILogService log)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }


        public OperationResult ValidateUserData(UserDto userDto)
        {
            return ExecuteOperation(
                () =>
                {
                    _repository.ValidateCreateUser(userDto);
                    return OperationResult.Ok();
                },
                OperationValidateUserData,
                ex =>
                {
                    if (ex is RepositoryValidationException rve)
                        return OperationResult.Fail($"Repository validation error: {rve.Code}", rve.Code);

                    if (ex is ArgumentException)
                        return OperationResult.Fail("Argument validation failure.", MessageCode.UserValidationError);

                    if (ex is SqlException sql)
                        return OperationResult.Fail($"SQL error: {sql.Number}", MessageCode.ServerUnavailable);

                    return OperationResult.Fail("Unexpected exception.", MessageCode.UnknownError);
                }
            );
        }

        public OperationResult RequestVerificationCode(string email)
        {
            return ExecuteOperation(
                () =>
                {
                    string normalizedEmail = email.Trim().ToLower();


                    var code = GenerateCode();

                    lock (_codes)
                    {
                        _codes[normalizedEmail] = (code, DateTime.UtcNow);
                    }


                    EmailSender.SendVerificationEmail(email, code);


                    _log.Info($"[RequestVerificationCode] EMAIL SENT OK");

                    return new OperationResult
                    {
                        Success = true,
                        Code = MessageCode.CodeSentSuccessfully,
                        TechnicalDetail = "Verification code generated."
                    };
                },
                OperationRequestVerificationCode,
                ex =>
                {
                    _log.Error($"[RequestVerificationCode] ERROR sending email: {ex.Message}", ex);
                    return OperationResult.Fail("Email sending failure.", MessageCode.VerificationCodeSendError);
                }
            );
        }


        public OperationResult CreateUser(UserDto userDto, string code)
        {
            return ExecuteOperation(
                () =>
                {
                    string storedCode;
                    DateTime createdUtc;

                    lock (_codes)
                    {
                        if (!_codes.TryGetValue(userDto.Email, out var data))
                        {
                            return OperationResult.Fail("Code not found.", MessageCode.VerificationCodeNotFound);
                        }
                        storedCode = data.Code;
                        createdUtc = data.CreatedUtc;
                    }

                    if (DateTime.UtcNow - createdUtc > TimeSpan.FromMinutes(5))
                    {
                        RemoveStoredCode(userDto.Email);
                        return OperationResult.Fail("Code expired.", MessageCode.VerificationCodeExpired);
                    }

                    if (!string.Equals(storedCode, code, StringComparison.Ordinal))
                    {
                        return OperationResult.Fail("Invalid code.", MessageCode.VerificationCodeInvalid);
                    }
                    RemoveStoredCode(userDto.Email);

                    var user = _repository.CreateUser(userDto);

                    SendWelcomeEmail(MapToUserInfo(user, userDto));

                    return OperationResult.Ok();
                },
                OperationCreateUser,
                ex =>
                {
                    if (ex is SqlException sql)
                    {
                        return OperationResult.Fail($"SQL error: {sql.Number}", MessageCode.ServerUnavailable);
                    }
                    return OperationResult.Fail("Unexpected exception.", MessageCode.UnknownError);
                }
            );
        }


        private static void RemoveStoredCode(string email)
        {
            lock (_codes)
                if (_codes.ContainsKey(email))
                {
                    _codes.Remove(email);
                }
        }

        private static string GenerateCode()
        {
            return new Random()
                .Next(1000, 10000)
                .ToString();
        }



        private static void SendWelcomeEmail(UserInfo user)
        {
            Task.Run(async () =>
            {
                try
                {
                    await Email.SendWelcomeAsync(user).ConfigureAwait(false);
                }
                catch { }
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

   
        private OperationResult ExecuteOperation(
            Func<OperationResult> action,
            string context,
            Func<Exception, OperationResult> onError)
        {
            try
            {
                _log.Info($"[{context}] START");
                var result = action();
                _log.Info($"[{context}] SUCCESS");
                return result;
            }
            catch (Exception ex)
            {
                _log.Error($"[{context}] Unexpected exception: {ex.Message}", ex);
                return onError(ex);
            }
        }
    }
}
