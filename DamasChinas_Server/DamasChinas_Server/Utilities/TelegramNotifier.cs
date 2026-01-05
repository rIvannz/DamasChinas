using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DamasChinas_Server.Utilidades
{
    internal static class TelegramNotifier
    {
        public const string DbDownHeader = "🚨 BASE DE DATOS NO DISPONIBLE";
        public const string TimeFormat = "yyyy-MM-dd HH:mm:ss";
        public const string DbDownTemplate = "BASE DE DATOS NO DISPONIBLE\n" + " {0}\n\n" + " {1}";
        private const string BotToken = "8537988330:AAH9w2ufz-tcH1kMT1rFjXuCQp2sVRtLlWA";
        private const string ChatId = "5356225517";

        private static readonly TimeSpan RetryDelay = TimeSpan.FromMinutes(3);

        public static void Send(string message)
        {
            Task.Run(() => TrySendAsync(message));
        }

        private static async Task TrySendAsync(string message)
        {
            if (TrySend(message))
                return;

            await Task.Delay(RetryDelay).ConfigureAwait(false);

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
                        $"https://api.telegram.org/bot{BotToken}/sendMessage";

                    string json =
                        "{" +
                        $"\"chat_id\":\"{ChatId}\"," +
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
