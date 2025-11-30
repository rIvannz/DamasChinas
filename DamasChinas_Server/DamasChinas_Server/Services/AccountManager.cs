using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;
using System;
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


        // TODO
        // ELIMINAR LOS "NUMEROS MAGICOS" DE TECHNICAL DETAIL
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
