using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.ServiceModel;

namespace DamasChinas_Server.Services
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class AccountManager : IAccountManager
    {
        private readonly RepositoryUsers _repository;
        private readonly ILogService _log;

        private const string OperationChangeUsername = nameof(ChangeUsername);
        private const string OperationChangePassword = nameof(ChangePassword);
        private const string OperationChangeAvatar = nameof(ChangeAvatar);
        private const string OperationChangeSocialUrl = nameof(ChangeSocialUrl);

    
        private static readonly Dictionary<string, (string Code, DateTime CreatedUtc)> _passwordCodes =
            new Dictionary<string, (string Code, DateTime CreatedUtc)>();

        public AccountManager()
            : this(new RepositoryUsers(), LogFactory.Create<AccountManager>())
        {
        }

        internal AccountManager(RepositoryUsers repository, ILogService log)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public PublicProfile GetPublicProfile(int idUser)
        {
            _log.Info($"[GetPublicProfile] idUser={idUser}");
            return _repository.GetPublicProfile(idUser);
        }

        public PublicFriendProfile GetFriendPublicProfile(string username)
        {
            _log.Info($"[GetFriendPublicProfile] username={username}");
            return _repository.GetFriendPublicProfile(username);
        }

        public OperationResult ChangeUsername(string username, string newUsername)
        {
            _log.Info($"[ChangeUsername] username={username}, newUsername={newUsername}");

            return ExecuteAccountOperation(
                () =>
                {
                    bool ok = _repository.ChangeUsername(username, newUsername);

                    if (ok)
                    {
                        _log.Info($"[ChangeUsername] SUCCESS username={username} → {newUsername}");
                        SessionManager.UpdateSessionUsername(username, newUsername);
                    }
                    else
                    {
                        _log.Warn($"[ChangeUsername] FAIL username={username} → {newUsername}");
                    }

                    return ok;
                },
                MessageCode.Success,
                MessageCode.UnknownError,
                MessageCode.ServerUnavailable,
                OperationChangeUsername
            );
        }

        public OperationResult ChangePassword(string email, string newPassword)
        {
            _log.Info($"[ChangePassword] email={email}");
            return ExecuteAccountOperation(
                () => _repository.ChangePassword(email, newPassword),
                MessageCode.Success,
                MessageCode.UnknownError,
                MessageCode.ServerUnavailable,
                OperationChangePassword
            );
        }

        public OperationResult ChangeAvatar(int idUser, string avatarFile)
        {
            _log.Info($"[ChangeAvatar] idUser={idUser}, avatarFile={avatarFile}");
            return ExecuteAccountOperation(
                () => _repository.ChangeAvatar(idUser, avatarFile),
                MessageCode.AvatarUpdateSuccess,
                MessageCode.AvatarUpdateFailed,
                MessageCode.ServerUnavailable,
                OperationChangeAvatar
            );
        }

        public OperationResult ChangeSocialUrl(int idUser, string socialUrl)
        {
            _log.Info($"[ChangeSocialUrl] idUser={idUser}, socialUrl={socialUrl}");

            return ExecuteAccountOperation(
                () => _repository.ChangeSocialUrl(idUser, socialUrl),
                MessageCode.Success,
                MessageCode.UnknownError,
                MessageCode.ServerUnavailable,
                nameof(ChangeSocialUrl)
            );
        }

    
        public OperationResult RequestPasswordChangeCode(string email)
        {
            const string context = "RequestPasswordChangeCode";
            _log.Info($"[{context}]");

            try
            {
                var code = GenerateVerificationCode();

                lock (_passwordCodes)
                    _passwordCodes[email] = (code, DateTime.UtcNow);

                EmailSender.SendVerificationEmail(email, code);

                return new OperationResult
                {
                    Success = true,
                    Code = MessageCode.CodeSentSuccessfully,
                    TechnicalDetail = $"{context}: Code generated and sent."
                };
            }
            catch (SqlException ex)
            {
                _log.Error($"[{context}] SQL ERROR {ex.Number}", ex);
                return OperationResult.Fail("SQL error.", MessageCode.ServerUnavailable);
            }
            catch (Exception ex)
            {
                _log.Error($"[{context}] Unexpected exception {ex.Message}", ex);
                return OperationResult.Fail("Unexpected error.", MessageCode.UnknownError);
            }
        }

        public OperationResult ConfirmPasswordChange(string email, string code, string newPassword)
        {
            const string context = "ConfirmPasswordChange";
            _log.Info($"[{context}] email={email}");

            try
            {
                string storedCode;
                DateTime createdUtc;

                lock (_passwordCodes)
                {
                    if (!_passwordCodes.TryGetValue(email, out var data))
                    {
                        return OperationResult.Fail("Code not found.", MessageCode.VerificationCodeNotFound);
                    }

                    storedCode = data.Code;
                    createdUtc = data.CreatedUtc;
                }

                if (DateTime.UtcNow - createdUtc > TimeSpan.FromMinutes(5))
                {
                    RemoveStoredPasswordCode(email);
                    return OperationResult.Fail("Code expired.", MessageCode.VerificationCodeExpired);
                }

                if (!string.Equals(storedCode, code, StringComparison.Ordinal))
                {
                    return OperationResult.Fail("Invalid code.", MessageCode.VerificationCodeInvalid);

                }

                RemoveStoredPasswordCode(email);
                bool ok = _repository.ChangePasswordbyemail(email, newPassword);

                if (!ok)
                {
                    return OperationResult.Fail("Failed updating password.", MessageCode.UnknownError);
                }

                return OperationResult.Ok();
            }
            catch (SqlException ex)
            {
                _log.Error($"[{context}] SQL ERROR {ex.Number}", ex);
                return OperationResult.Fail("SQL error.", MessageCode.ServerUnavailable);
            }
            catch (Exception ex)
            {
                _log.Error($"[{context}] Unexpected exception {ex.Message}", ex);
                return OperationResult.Fail("Unexpected error.", MessageCode.UnknownError);
            }
        }

       
        private static void RemoveStoredPasswordCode(string email)
        {
            lock (_passwordCodes)
            {
                if (_passwordCodes.ContainsKey(email))
                    _passwordCodes.Remove(email);
            }
        }

        private static string GenerateVerificationCode()
        {
            return new Random().Next(1000, 10000).ToString();
        }

  
        private static OperationResult ExecuteAccountOperation(
         Func<bool> operation,
         MessageCode successCode,
         MessageCode failureCode,
         MessageCode fatalCode,
         string context)
        {
            var result = new OperationResult();

            try
            {
                bool success = operation();

                result.Success = success;
                result.Code = success ? successCode : failureCode;
                result.TechnicalDetail = success
                    ? $"Operation '{context}' executed successfully."
                    : $"Operation '{context}' failed.";

                return result;
            }
            catch (SqlException ex)
            {
                LogStatic.Error($"[SQL ERROR] {context} (Number={ex.Number})", ex);

                result.Success = false;
                result.Code = fatalCode;
                result.TechnicalDetail = $"SQL error ({ex.Number})";
                return result;
            }
            catch (ArgumentException ex)
            {
                LogStatic.Warn($"[ARGUMENT ERROR] {context}", ex);

                result.Success = false;
                result.Code = failureCode;
                result.TechnicalDetail = "Argument error.";
                return result;
            }
            catch (InvalidOperationException ex)
            {
                LogStatic.Error($"[INVALID OPERATION] {context}", ex);

                result.Success = false;
                result.Code = failureCode;
                result.TechnicalDetail = "Invalid operation.";
                return result;
            }
        }
    }

    internal static class LogStatic
    {
        private static readonly ILogService _log =
            LogFactory.Create<AccountManager>();

        public static void Info(string msg) => _log.Info(msg);
        public static void Warn(string msg, Exception ex = null) => _log.Warn($"{msg} | {ex?.Message}");
        public static void Error(string msg, Exception ex = null) => _log.Error(msg, ex);
    }
}
