using System;
using DamasChinas_Client.UI.LogInServiceProxy;
using DamasChinas_Shared.Validation;

namespace DamasChinas_Client.UI.Utilities
{
    internal static class Validator
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
                // msg_InvalidNameEmpty
                throw new ClientValidationException(MessageKeys.InvalidNameEmpty);
            }

            if (name.Length < UserValidationRules.NameMinLength ||
                name.Length > UserValidationRules.NameMaxLength)
            {
                // msg_InvalidNameLength
                throw new ClientValidationException(MessageKeys.InvalidNameLength);
            }

            if (!UserValidationRules.NameRegex.IsMatch(name))
            {
                // msg_InvalidNameCharacters
                throw new ClientValidationException(MessageKeys.InvalidNameCharacters);
            }
        }


        public static void ValidateUsername(string username)
        {
            username = Normalize(username);

            if (string.IsNullOrWhiteSpace(username))
            {
                // msg_InvalidUsernameEmpty
                throw new ClientValidationException(MessageKeys.InvalidUsernameEmpty);
            }

            if (username.Length < UserValidationRules.UsernameMinLength ||
                username.Length > UserValidationRules.UsernameMaxLength)
            {
                // msg_InvalidUsernameLength
                throw new ClientValidationException(MessageKeys.InvalidUsernameLength);
            }

            if (!UserValidationRules.UsernameRegex.IsMatch(username))
            {
                // msg_InvalidUsernameCharacters
                throw new ClientValidationException(MessageKeys.InvalidUsernameCharacters);
            }
        }


        public static void ValidatePassword(string password)
        {
            password = Normalize(password);

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ClientValidationException(MessageKeys.InvalidPasswordEmpty);
            }

            if (password.Length < UserValidationRules.PasswordMinLength)
            {
                throw new ClientValidationException(MessageKeys.InvalidPasswordLength);
            }

            if (!UserValidationRules.PasswordUppercaseRegex.IsMatch(password))
            {
                throw new ClientValidationException(MessageKeys.InvalidPasswordUppercase);
            }

            if (!UserValidationRules.PasswordLowercaseRegex.IsMatch(password))
            {
                throw new ClientValidationException(MessageKeys.InvalidPasswordLowercase);
            }

            if (!UserValidationRules.PasswordDigitRegex.IsMatch(password))
            {
                throw new ClientValidationException(MessageKeys.InvalidPasswordDigit);
            }

            if (!UserValidationRules.PasswordSpecialRegex.IsMatch(password))
            {
                throw new ClientValidationException(MessageKeys.InvalidPasswordSpecial);
            }
        }


        public static void ValidateEmail(string email)
        {
            email = Normalize(email);

            if (string.IsNullOrWhiteSpace(email))
            {
                // msg_InvalidEmailEmpty
                throw new ClientValidationException(MessageKeys.InvalidEmailEmpty);
            }

            if (email.Length > UserValidationRules.EmailMaxLength)
            {
                // msg_InvalidEmailTooLong
                throw new ClientValidationException(MessageKeys.InvalidEmailTooLong);
            }

            if (!UserValidationRules.EmailRegex.IsMatch(email))
            {
                // msg_InvalidEmailFormat
                throw new ClientValidationException(MessageKeys.InvalidEmailFormat);
            }
        }


        public static void ValidateLoginRequest(LoginRequest loginRequest)
        {
            if (loginRequest == null)
            {
                throw new ArgumentNullException(nameof(loginRequest));
            }

            loginRequest.Username = Normalize(loginRequest.Username);
            loginRequest.Password = Normalize(loginRequest.Password);

            if (string.IsNullOrWhiteSpace(loginRequest.Username) ||
                string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                // msg_EmptyCredentials (validación universal)
                throw new ClientValidationException(MessageKeys.EmptyCredentials);
            }

            if (loginRequest.Username.Contains("@"))
            {
                ValidateEmail(loginRequest.Username);
            }
            else
            {
                ValidateUsername(loginRequest.Username);
            }
        }
    }
}
