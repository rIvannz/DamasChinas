using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Services;
using System;
using System.Data.SqlClient;
using System.ServiceModel;

namespace DamasChinas_Server.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession,
                     ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class AccountManager : IAccountManager
    {
        private readonly RepositoryUsers _repository;

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


        public OperationResult ChangeUsername(string username, string newUsername)
        {
            return ExecuteAccountOperation(
                () =>
                {
                    bool success = _repository.ChangeUsername(username, newUsername);

                    if (success)
                    {
                        SessionManager.UpdateSessionUsername(username, newUsername);
                    }

                    return success;
                },
                MessageCode.Success,
                MessageCode.UnknownError,
                MessageCode.ServerUnavailable,
                "ChangeUsername"
            );
        }


        public OperationResult ChangePassword(string email, string newPassword)
        {
            return ExecuteAccountOperation(
                () => _repository.ChangePassword(email, newPassword),
                MessageCode.Success,
                MessageCode.UnknownError,
                MessageCode.ServerUnavailable,
                "ChangePassword"
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
                System.Diagnostics.Debug.WriteLine($"[TRACE] Starting operation: {context}");

                bool success = operation();

                result.Success = success;
                result.Code = success ? successCode : failureCode;
                result.TechnicalDetail = success
                    ? $"{context} executed successfully."
                    : $"{context} failed due to business conditions.";

                System.Diagnostics.Debug.WriteLine(success
                    ? $"[TRACE] {context} completed successfully."
                    : $"[ERROR] {context} did not complete successfully.");

                return result;
            }
            catch (SqlException ex)
            {
                result.Success = false;
                result.Code = fatalCode;
                result.TechnicalDetail = ex.Message;
                System.Diagnostics.Debug.WriteLine($"[FATAL] SQL error in {context}: {ex.Message}");
                return result;
            }
            catch (ArgumentException ex)
            {
                result.Success = false;
                result.Code = failureCode;
                result.TechnicalDetail = ex.Message;
                System.Diagnostics.Debug.WriteLine($"[ERROR] Invalid argument in {context}: {ex.Message}");
                return result;
            }
            catch (InvalidOperationException ex)
            {
                result.Success = false;
                result.Code = failureCode;
                result.TechnicalDetail = ex.Message;
                System.Diagnostics.Debug.WriteLine($"[ERROR] Invalid operation in {context}: {ex.Message}");
                return result;
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine($"[TRACE] Finishing operation: {context}");
            }
        }
    }
}
