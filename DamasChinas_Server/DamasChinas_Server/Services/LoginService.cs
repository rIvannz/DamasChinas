using DamasChinas_Server.Common;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.GameRepositories;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Logic;
using DamasChinas_Server.Services;
using DamasChinas_Shared.Contracts.Dtos;
using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.ServiceModel;

namespace DamasChinas_Server
{
    [DbGuardBehavior]
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class LoginService : ILoginService
    {
        private readonly RepositoryUsers _repository;
        private readonly ILogService _log;

        private const string OperationLogin = nameof(Login);

        public LoginService()
            : this(new RepositoryUsers(), LogFactory.Create<LoginService>())
        {
        }

        internal LoginService(RepositoryUsers repository, ILogService log)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public void Login(LoginRequest loginRequest)
        {
            ExecuteOperation(
                () =>
                {
                    var callback = OperationContext.Current.GetCallbackChannel<ILoginCallback>();

                    try
                    {
                        _log.Info($"[{OperationLogin}] Intentando login para: {loginRequest?.Username}");

                        var profile = _repository.Login(loginRequest);

                        int userId = profile.IdUser;

                        var sanctionsRepo = new RepositorySanctions();
                        if (sanctionsRepo.HasActiveBan(userId))
                        {
                            BanInfoDto banInfo = sanctionsRepo.GetActiveBanInfo(userId);
                            callback.OnLoginBanned(banInfo);
                            _log.Warn($"[{OperationLogin}] Usuario baneado: {profile.Username}");
                            return;
                        }

                        _log.Info($"[{OperationLogin}] Login exitoso: {profile.Username}");
                        callback.OnLoginSuccess(profile);
                    }
                    catch (EntityException)
                    {
                        callback.OnLoginError(MessageCode.ServerUnavailable);
                    }
                    catch (SqlException)
                    {
                        callback.OnLoginError(MessageCode.ServerUnavailable);
                    }
                    catch (DbUpdateException)
                    {
                        callback.OnLoginError(MessageCode.ServerUnavailable);
                    }
                    catch (DbEntityValidationException)
                    {
                        callback.OnLoginError(MessageCode.ServerUnavailable);
                    }
                    catch (Exception ex)
                    {
                        _log.Warn($"[{OperationLogin}] Login fallido: {ex.Message}");
                        callback.OnLoginError(MessageCode.LoginInvalidCredentials);
                    }
                },
                OperationLogin
            );
        }



        private void ExecuteOperation(Action action, string context, Action<Exception> onError = null)
        {
            try
            {
                _log.Info($"[{context}] START");
                action();
                _log.Info($"[{context}] SUCCESS");
            }
            catch (SqlException ex)
            {
                _log.Error($"[{context}] SQL ERROR {ex.Number}");
                onError?.Invoke(ex);
            }
            catch (EntityException ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    _log.Error($"[{context}] SQL ERROR {sqlEx.Number}", sqlEx);
                    onError?.Invoke(sqlEx);
                }
                else
                {
                    _log.Error($"[{context}] ENTITY ERROR: {ex.Message}");
                    onError?.Invoke(ex);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"[{context}] Unexpected exception: {ex.Message}");
                onError?.Invoke(ex);
            }
        }
    }
}