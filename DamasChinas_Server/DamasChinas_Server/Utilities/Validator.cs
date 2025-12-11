using System;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.Common;
using DamasChinas_Shared.Validation;

namespace DamasChinas_Server.Utilidades
{
    public class Validator
    {
  
        private static string Normalize(string value)
        {
            return UserValidationRules.Normalize(value);
        }


        public static void ValidateName(string name)
        {
            name = Normalize(name);

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new RepositoryValidationException(MessageCode.InvalidNameEmpty);
            }

            if (name.Length < UserValidationRules.NameMinLength ||
                name.Length > UserValidationRules.NameMaxLength)
            {
                throw new RepositoryValidationException(MessageCode.InvalidNameLength);
            }

            if (!UserValidationRules.NameRegex.IsMatch(name))
            {
                throw new RepositoryValidationException(MessageCode.InvalidNameCharacters);
            }
        }


        public static void ValidateUsername(string username)
        {
            username = Normalize(username);

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new RepositoryValidationException(MessageCode.InvalidUsernameEmpty);
            }

            if (username.Length < UserValidationRules.UsernameMinLength ||
                username.Length > UserValidationRules.UsernameMaxLength)
            {
                throw new RepositoryValidationException(MessageCode.InvalidUsernameLength);
            }

            if (!UserValidationRules.UsernameRegex.IsMatch(username))
            {
                throw new RepositoryValidationException(MessageCode.InvalidUsernameCharacters);
            }
        }


        public static void ValidatePassword(string password)
        {
            password = Normalize(password);

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new RepositoryValidationException(MessageCode.InvalidPasswordEmpty);
            }

            if (password.Length < UserValidationRules.PasswordMinLength)
            {
                throw new RepositoryValidationException(MessageCode.InvalidPasswordLength);
            }

            if (!UserValidationRules.PasswordUppercaseRegex.IsMatch(password))
            {
                throw new RepositoryValidationException(MessageCode.InvalidPasswordUppercase);
            }

            if (!UserValidationRules.PasswordLowercaseRegex.IsMatch(password))
            {
                throw new RepositoryValidationException(MessageCode.InvalidPasswordLowercase);
            }

            if (!UserValidationRules.PasswordDigitRegex.IsMatch(password))
            {
                throw new RepositoryValidationException(MessageCode.InvalidPasswordDigit);
            }

            if (!UserValidationRules.PasswordSpecialRegex.IsMatch(password))
            {
                throw new RepositoryValidationException(MessageCode.InvalidPasswordSpecial);
            }
        }


        public static void ValidateEmail(string email)
        {
            email = Normalize(email);

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new RepositoryValidationException(MessageCode.InvalidEmailEmpty);
            }

            if (email.Length > UserValidationRules.EmailMaxLength)
            {
                throw new RepositoryValidationException(MessageCode.InvalidEmailTooLong);
            }

            if (!UserValidationRules.EmailRegex.IsMatch(email))
            {
                throw new RepositoryValidationException(MessageCode.InvalidEmailFormat);
            }
        }


        public static void ValidateUserDto(UserDto userDto)
        {
            if (userDto == null)
            {
                throw new RepositoryValidationException(MessageCode.UserValidationError);
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
                throw new RepositoryValidationException(MessageCode.UserValidationError);
            }

            loginRequest.Username = Normalize(loginRequest.Username);
            loginRequest.Password = Normalize(loginRequest.Password);

            if (string.IsNullOrWhiteSpace(loginRequest.Username))
            {
                throw new RepositoryValidationException(MessageCode.InvalidUsernameEmpty);
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
                throw new RepositoryValidationException(MessageCode.InvalidPasswordEmpty);
            }
        }
    }
}
