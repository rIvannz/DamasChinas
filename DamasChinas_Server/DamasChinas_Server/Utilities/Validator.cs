using System;
using System.Text.RegularExpressions;
using DamasChinas_Server.Dtos;

namespace DamasChinas_Server.Utilidades
{
	internal static class Validator
	{
		private static readonly Regex NameRegex = new Regex("^[a-zA-ZáéíóúÁÉÍÓÚñÑ]+$", RegexOptions.Compiled);
		private static readonly Regex UsernameRegex = new Regex("^[a-zA-Z0-9_-]+$", RegexOptions.Compiled);
		private static readonly Regex PasswordUppercaseRegex = new Regex("[A-Z]", RegexOptions.Compiled);
		private static readonly Regex PasswordLowercaseRegex = new Regex("[a-z]", RegexOptions.Compiled);
		private static readonly Regex PasswordDigitRegex = new Regex("[0-9]", RegexOptions.Compiled);
		private static readonly Regex PasswordSpecialRegex = new Regex("[\\W_]", RegexOptions.Compiled);
		private static readonly Regex EmailRegex = new Regex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", RegexOptions.Compiled);

		private static string Normalize(string value)
		{
			return value?.Trim();
		}

		public static void ValidateName(string name)
		{
			name = Normalize(name);

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("El nombre no puede estar vacío.");
			}
			if (name.Length < 2 || name.Length > 50)
			{
				throw new ArgumentException("El nombre debe tener entre 2 y 50 caracteres.");
			}
			if (!NameRegex.IsMatch(name))
			{
				throw new ArgumentException("El nombre contiene caracteres inválidos.");
			}
		}

		public static void ValidateUsername(string username)
		{
			username = Normalize(username);

			if (string.IsNullOrWhiteSpace(username))
			{
				throw new ArgumentException("El nombre de usuario no puede estar vacío.");
			}
			if (username.Length < 3 || username.Length > 15)
			{
				throw new ArgumentException("El nombre de usuario debe tener entre 3 y 15 caracteres.");
			}
			if (!UsernameRegex.IsMatch(username))
			{
				throw new ArgumentException("El nombre de usuario contiene caracteres inválidos.");
			}
		}

		public static void ValidatePassword(string password)
		{
			password = Normalize(password);

			if (string.IsNullOrWhiteSpace(password))
			{
				throw new ArgumentException("La contraseña no puede estar vacía.");
			}
			if (password.Length < 8)
			{
				throw new ArgumentException("La contraseña debe tener al menos 8 caracteres.");
			}
			if (!PasswordUppercaseRegex.IsMatch(password))
			{
				throw new ArgumentException("La contraseña debe tener al menos una letra mayúscula.");
			}
			if (!PasswordLowercaseRegex.IsMatch(password))
			{
				throw new ArgumentException("La contraseña debe tener al menos una letra minúscula.");
			}
			if (!PasswordDigitRegex.IsMatch(password))
			{
				throw new ArgumentException("La contraseña debe tener al menos un número.");
			}
			if (!PasswordSpecialRegex.IsMatch(password))
			{
				throw new ArgumentException("La contraseña debe tener al menos un carácter especial.");
			}
		}

		public static void ValidateEmail(string email)
		{
			email = Normalize(email);

			if (string.IsNullOrWhiteSpace(email))
			{
				throw new ArgumentException("El correo no puede estar vacío.");
			}
			if (email.Length > 100)
			{
				throw new ArgumentException("El correo es demasiado largo.");
			}
			if (!EmailRegex.IsMatch(email))
			{
				throw new ArgumentException("El correo tiene un formato inválido.");
			}
		}

		public static void ValidateUserDto(UserDto userDto)
		{
			if (userDto == null)
			{
				throw new ArgumentNullException("El objeto UserDto no puede ser nulo.");
			}
			userDto.Name = Normalize(userDto.Name);
			userDto.LastName = Normalize(userDto.LastName);
			userDto.Email = Normalize(userDto.Email);
			userDto.Username = Normalize(userDto.Username);
			

			ValidateName(userDto.Name);
			ValidateName(userDto.LastName);
			ValidateEmail(userDto.Email);
			ValidateUsername(userDto.Username);
			
		}

		public static void ValidateLoginRequest(LoginRequest loginRequest)
		{
			if (loginRequest == null)
			{
				throw new ArgumentNullException("El objeto LoginRequest no puede ser nulo.");
			}
			loginRequest.Username = Normalize(loginRequest.Username);
			loginRequest.Password = Normalize(loginRequest.Password);

			if (string.IsNullOrWhiteSpace(loginRequest.Username))
			{
				throw new ArgumentException("El usuario o correo es requerido.");
			}
			if (loginRequest.Username.Contains("@"))
			{
				ValidateEmail(loginRequest.Username);
			}
			else
			{
				ValidateUsername(loginRequest.Username);
			}

			if (string.IsNullOrWhiteSpace(loginRequest.Password))
			{
				throw new ArgumentException("La contraseña es requerida.");
			}
		}
	}
}
