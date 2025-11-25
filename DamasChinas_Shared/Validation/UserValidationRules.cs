using System.Text.RegularExpressions;

namespace DamasChinas_Shared.Validation
{
    public static class UserValidationRules
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 50;
        public const int UsernameMinLength = 3;
        public const int UsernameMaxLength = 15;
        public const int PasswordMinLength = 8;
        public const int EmailMaxLength = 100;

        public static readonly Regex NameRegex =
            new Regex("^[a-zA-ZáéíóúÁÉÍÓÚñÑ]+$", RegexOptions.Compiled);

        public static readonly Regex UsernameRegex =
            new Regex("^[a-zA-Z0-9_-]+$", RegexOptions.Compiled);

        public static readonly Regex PasswordUppercaseRegex =
            new Regex("[A-Z]", RegexOptions.Compiled);

        public static readonly Regex PasswordLowercaseRegex =
            new Regex("[a-z]", RegexOptions.Compiled);

        public static readonly Regex PasswordDigitRegex =
            new Regex("[0-9]", RegexOptions.Compiled);

        public static readonly Regex PasswordSpecialRegex =
            new Regex("[\\W_]", RegexOptions.Compiled);

        public static readonly Regex EmailRegex =
            new Regex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", RegexOptions.Compiled);

        public static string Normalize(string value)
        {
            return value?.Trim();
        }
    }
}
