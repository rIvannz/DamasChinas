using System;
using Damas_Chinas_Server.Dtos;
using Damas_Chinas_Server.Services;

namespace Damas_Chinas_Server
{
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
                                        var success = _repository.ChangeUsername(username, newUsername);

                                        if (success)
                                        {
                                                SessionManager.UpdateSessionUsername(username, newUsername);
                                        }

                                        return success;
                                },
                                "Nombre de usuario actualizado correctamente.",
                                "Error al actualizar el nombre de usuario.",
                                "Error al actualizar el nombre de usuario");
                }

		public OperationResult ChangePassword(string correo, string nuevaPassword)
		{
			return ExecuteAccountOperation(
				() => _repository.ChangePassword(correo, nuevaPassword),
				"Contraseña actualizada correctamente.",
				"Error al actualizar la contraseña.",
				"Error al actualizar la contraseña");
		}

		private static OperationResult ExecuteAccountOperation(Func<bool> operation, string successMessage, string failureMessage, string errorPrefix)
		{
			try
			{
				bool success = operation();
				return new OperationResult
				{
					Succes = success,
					Messaje = success ? successMessage : failureMessage,
					User = null
				};
			}
			catch (Exception ex)
			{
				return new OperationResult
				{
					Succes = false,
					Messaje = $"{errorPrefix}: {ex.Message}",
					User = null
				};
			}
		}
	}
}
