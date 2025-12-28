using System;

namespace DamasChinas_Server.Utilities.Email
{
    public static class EmailLanguageMapper
    {
        public static EmailLanguage FromCultureCode(string cultureCode)
        {
            if (string.IsNullOrWhiteSpace(cultureCode))
            {
                return EmailLanguage.English;
            }

            string code = cultureCode.Trim();

            if (code.StartsWith("es", StringComparison.OrdinalIgnoreCase))
            {
                return EmailLanguage.Spanish;
            }

            if (code.StartsWith("pt", StringComparison.OrdinalIgnoreCase))
            {
                return EmailLanguage.Portuguese;
            }

            if (code.StartsWith("fr", StringComparison.OrdinalIgnoreCase))
            {
                return EmailLanguage.French;
            }

            return EmailLanguage.English;
        }
    }
}
