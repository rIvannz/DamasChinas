using System;
using System.Collections.Generic;

namespace DamasChinas_Server.Utilidades
{
    internal static class TelegramSettingsProvider
    {
        private static readonly IDictionary<string, string> _settings =
            KeyValueFileLoader.Load("Config/telegramSettings.txt");

        public static string BotToken =>
            GetRequired("TelegramBotToken");

        public static string ChatId =>
            GetRequired("TelegramChatId");

        public static TimeSpan RetryDelay =>
            TimeSpan.FromMinutes(GetInt("TelegramRetryMinutes", 3));

        public static string TimeFormat =>
            GetOptional("TelegramTimeFormat", "yyyy-MM-dd HH:mm:ss");

        public static string GetDbDownHeader(string languageCode) =>
            GetLocalized("DbDownHeader", languageCode);

        public static string GetDbDownTemplate(string languageCode) =>
            GetLocalized("DbDownTemplate", languageCode);

        private static string GetLocalized(string baseKey, string languageCode)
        {
            string lang = NormalizeLanguage(languageCode);
            string key = $"{baseKey}_{lang}";

            if (_settings.TryGetValue(key, out string value))
            {
                return value;
            }

            // fallback a inglés
            return _settings[$"{baseKey}_en"];
        }

        private static string GetRequired(string key)
        {
            if (!_settings.TryGetValue(key, out string value))
            {
                throw new InvalidOperationException(
                    $"Missing Telegram setting: {key}");
            }

            return value;
        }

        private static string GetOptional(string key, string defaultValue)
        {
            return _settings.TryGetValue(key, out string value)
                ? value
                : defaultValue;
        }

        private static int GetInt(string key, int defaultValue)
        {
            if (_settings.TryGetValue(key, out string value)
                && int.TryParse(value, out int parsed))
            {
                return parsed;
            }

            return defaultValue;
        }

        private static string NormalizeLanguage(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return "en";
            }

            int dashIndex = languageCode.IndexOf('-');
            return dashIndex > 0
                ? languageCode.Substring(0, dashIndex).ToLowerInvariant()
                : languageCode.ToLowerInvariant();
        }
    }
}
