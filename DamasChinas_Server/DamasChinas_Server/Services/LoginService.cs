using System;
using System.ServiceModel;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Services;

namespace DamasChinas_Server
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
	public class LoginService : ILoginService
	{
		private readonly RepositoryUsers _repository;

		public LoginService()
			: this(new RepositoryUsers())
		{
		}

		internal LoginService(RepositoryUsers repository)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		}

		public void Login(LoginRequest loginRequest)
		{
			var callback = OperationContext.Current.GetCallbackChannel<ILoginCallback>();

			try
			{
				var profile = _repository.Login(loginRequest);

				SessionManager.AddSession(profile.Username, callback);

				callback.OnLoginSuccess(profile);
			}
			catch (Exception ex)
			{
				callback.OnLoginError($"Error al iniciar sesión: {ex.Message}");
			}
		}
	}
}
