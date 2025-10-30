using System;
using System.Linq;
using System.Threading.Tasks;
using Damas_Chinas_Server.Dtos;
using Damas_Chinas_Server.Utilidades;

namespace Damas_Chinas_Server
{
	public class SingInService : ISingInService
	{
		private readonly RepositoryUsers _repository;

		public SingInService()
		{
			_repository = new RepositoryUsers();
		}

		public OperationResult CreateUser(UserDto userDto)
		{
			var result = new OperationResult();

			try
			{
				var user = _repository.CreateUser(userDto);
				result.User = MapToUserInfo(user, userDto);
				result.Succes = true;
				result.Messaje = "Usuario creado correctamente.";

				EnviarCorreoBienvenida(result.User);
			}
			catch (ArgumentException ex)
			{
				result.Succes = false;
				result.Messaje = ex.Message;
			}
			catch (InvalidOperationException ex)
			{
				result.Succes = false;
				result.Messaje = ex.Message;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex);
				result.Succes = false;
				result.Messaje = "Ocurrió un error inesperado al crear el usuario.";
			}

			return result;
		}

		private void EnviarCorreoBienvenida(UserInfo user)
		{
			Task.Run(async () =>
			{
				try
				{
					await Correo.EnviarBienvenidaAsync(user).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine(ex);
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
