using System;
using System.ServiceModel;
using Damas_Chinas_Server.Dtos;
using Damas_Chinas_Server.Interfaces;
using Damas_Chinas_Server.Services;

namespace Damas_Chinas_Server
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
