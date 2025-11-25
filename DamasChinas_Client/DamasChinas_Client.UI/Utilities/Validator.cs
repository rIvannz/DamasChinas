using System;
using DamasChinas_Client.UI.LogInServiceProxy;
using DamasChinas_Shared.Validation;

namespace DamasChinas_Client.UI.Utilities
{
    internal static class Validator
    {
        // ============================================================
        //  NORMALIZACIÓN
        // ============================================================
        private static string Normalize(string value)
        {
            return UserValidationRules.Normalize(value);
        }

        // ============================================================
        //  VALIDACIÓN DE NOMBRE
        // ============================================================
        public static void ValidateName(string name)
        {
            name = Normalize(name);

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ClientValidationException("msg_NameEmpty");
            }

            if (name.Length < UserValidationRules.NameMinLength ||
                name.Length > UserValidationRules.NameMaxLength)
            {
                throw new ClientValidationException("msg_NameLengthInvalid");
            }

            if (!UserValidationRules.NameRegex.IsMatch(name))
            {
                throw new ClientValidationException("msg_NameInvalidCharacters");
            }
        }

        // ============================================================
        //  VALIDACIÓN DE USERNAME
        // ============================================================
        public static void ValidateUsername(string username)
        {
            username = Normalize(username);

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ClientValidationException("msg_UsernameEmpty");
            }

            if (username.Length < UserValidationRules.UsernameMinLength ||
                username.Length > UserValidationRules.UsernameMaxLength)
            {
                throw new ClientValidationException("msg_UsernameLengthInvalid");
            }

            if (!UserValidationRules.UsernameRegex.IsMatch(username))
            {
                throw new ClientValidationException("msg_UsernameInvalidCharacters");
            }
        }

        // ============================================================
        //  VALIDACIÓN DE PASSWORD
        // ============================================================
        public static void ValidatePassword(string password)
        {
            password = Normalize(password);

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ClientValidationException("msg_PasswordEmpty");
            }

            if (password.Length < UserValidationRules.PasswordMinLength)
            {
                throw new ClientValidationException("msg_PasswordTooShort");
            }

            if (!UserValidationRules.PasswordUppercaseRegex.IsMatch(password))
            {
                throw new ClientValidationException("msg_PasswordRequiresUpper");
            }

            if (!UserValidationRules.PasswordLowercaseRegex.IsMatch(password))
            {
                throw new ClientValidationException("msg_PasswordRequiresLower");
            }

            if (!UserValidationRules.PasswordDigitRegex.IsMatch(password))
            {
                throw new ClientValidationException("msg_PasswordRequiresDigit");
            }

            if (!UserValidationRules.PasswordSpecialRegex.IsMatch(password))
            {
                throw new ClientValidationException("msg_PasswordRequiresSpecial");
            }
        }

        // ============================================================
        //  VALIDACIÓN DE EMAIL
        // ============================================================
        public static void ValidateEmail(string email)
        {
            email = Normalize(email);

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ClientValidationException("msg_EmptyEmail");
            }

            if (email.Length > UserValidationRules.EmailMaxLength)
            {
                throw new ClientValidationException("msg_EmailTooLong");
            }

            if (!UserValidationRules.EmailRegex.IsMatch(email))
            {
                throw new ClientValidationException("msg_InvalidEmail");
            }
        }

        // ============================================================
        //  VALIDACIÓN DE LOGIN REQUEST
        // ============================================================
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
                throw new ClientValidationException("msg_EmptyCredentials");
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
