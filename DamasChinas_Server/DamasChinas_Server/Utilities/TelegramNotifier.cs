using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DamasChinas_Server.Utilidades
{
    internal static class TelegramNotifier
    {
        public static void NotifyDatabaseUnavailable(
            string languageCode,
            string technicalDetail)
        {
            string timestamp =
                DateTime.Now.ToString(TelegramSettingsProvider.TimeFormat);

            string header =
                TelegramSettingsProvider.GetDbDownHeader(languageCode);

            string template =
                TelegramSettingsProvider.GetDbDownTemplate(languageCode);

            string message =
                header + "\n\n" +
                string.Format(template, timestamp, technicalDetail);

            SendAsync(message);
        }

        private static void SendAsync(string message)
        {
            Task.Run(() => TrySendAsync(message));
        }

        private static async Task TrySendAsync(string message)
        {
            if (TrySend(message))
            {
                return;
            }

            await Task.Delay(TelegramSettingsProvider.RetryDelay)
                .ConfigureAwait(false);

            TrySend(message);
        }

        private static bool TrySend(string message)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.Headers.Add("Content-Type", "application/json");

                    string url =
                        $"https://api.telegram.org/bot{TelegramSettingsProvider.BotToken}/sendMessage";

                    string json =
                        "{" +
                        $"\"chat_id\":\"{TelegramSettingsProvider.ChatId}\"," +
                        $"\"text\":\"{Escape(message)}\"," +
                        "\"parse_mode\":\"Markdown\"" +
                        "}";

                    client.UploadString(url, "POST", json);
                }

                return true;
            }
            catch (WebException)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static string Escape(string text)
        {
            return text
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "");
        }
    }
}
