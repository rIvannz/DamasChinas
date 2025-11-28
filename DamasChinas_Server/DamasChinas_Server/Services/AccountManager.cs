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

        private const string OperationChangeUsername = nameof(ChangeUsername);
        private const string OperationChangePassword = nameof(ChangePassword);
        private const string OperationChangeAvatar = nameof(ChangeAvatar);

        public AccountManager()
            : this(new RepositoryUsers())
        {
        }

        internal AccountManager(RepositoryUsers repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public PublicProfile GetPublicProfile(int idUser)
        {
            return _repository.GetPublicProfile(idUser);
        }

        public PublicFriendProfile GetFriendPublicProfile(string username)
        {
            return _repository.GetFriendPublicProfile(username);
        }

        public OperationResult ChangeUsername(string username, string newUsername)
        {
            return ExecuteAccountOperation(
                () =>
                {
                    bool ok = _repository.ChangeUsername(username, newUsername);

                    if (ok)
                    {
                        SessionManager.UpdateSessionUsername(username, newUsername);
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
            return ExecuteAccountOperation(
                () => _repository.ChangeAvatar(idUser, avatarFile),
                MessageCode.AvatarUpdateSuccess,
                MessageCode.AvatarUpdateFailed,
                MessageCode.ServerUnavailable,
                OperationChangeAvatar
            );
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
                result.Success = false;
                result.Code = fatalCode;
                result.TechnicalDetail = $"SQL error ({ex.Number})";
                return result;
            }
            catch (ArgumentException)
            {
                result.Success = false;
                result.Code = failureCode;
                result.TechnicalDetail = "Argument error.";
                return result;
            }
            catch (InvalidOperationException)
            {
                result.Success = false;
                result.Code = failureCode;
                result.TechnicalDetail = "Invalid operation.";
                return result;
            }
        }
    }
}
