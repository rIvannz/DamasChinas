using DamasChinas_Server.Dtos;
using DamasChinas_Server.Utilities.Email;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace DamasChinas_Server.Utilidades
{
    public static class Email
    {
        private static string SenderEmail;
        private static string SenderPassword;
        private static string SmtpHost;
        private static int SmtpPort;
        private static bool EnableSsl;

        private static readonly Dictionary<string, string> Values =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static string VerificationSubject;
        private static string VerificationBody;
        private static string WelcomeSubject;
        private static string WelcomeBodyTemplate;
        private static string InvitationSubject;
        private static string InvitationBody;

        public static string VerificationSubjectValue => VerificationSubject;
        public static string VerificationBodyValue => VerificationBody;
        public static string InvitationSubjectValue => InvitationSubject;
        public static string InvitationBodyValue => InvitationBody;

        static Email()
        {
            LoadConfig();
        }

        private static void LoadConfig()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emailSettings.txt");

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("No se encontró emailSettings.txt", path);
            }

            Values.Clear();

            var lines = File.ReadAllLines(path);

            foreach (var raw in lines)
            {
                string line = raw.Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (!line.Contains("="))
                    continue;

                var parts = line.Split(new[] { '=' }, 2);

                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    Values[key] = value;
                }
            }

            SenderEmail = GetRequired("SenderEmail");
            SenderPassword = GetRequired("SenderPassword");
            SmtpHost = GetRequired("SmtpHost");
            SmtpPort = int.Parse(GetRequired("SmtpPort"));
            EnableSsl = bool.Parse(GetRequired("EnableSsl"));

            WelcomeSubject = GetAny("WelcomeSubject_en", "WelcomeSubject", "WelcomeSubject_es");
            WelcomeBodyTemplate = GetAny("WelcomeBodyTemplate_en", "WelcomeBodyTemplate", "WelcomeBodyTemplate_es");
            VerificationSubject = GetAny("VerificationSubject_en", "VerificationSubject", "VerificationSubject_es");
            VerificationBody = GetAny("VerificationBody_en", "VerificationBody", "VerificationBody_es");
            InvitationSubject = GetAny("InvitationSubject_en", "InvitationSubject", "InvitationSubject_es");
            InvitationBody = GetAny("InvitationBody_en", "InvitationBody", "InvitationBody_es");
        }

        private static string GetRequired(string key)
        {
            if (!Values.TryGetValue(key, out string value) || string.IsNullOrWhiteSpace(value))
            {
                throw new KeyNotFoundException($"Missing required email setting: {key}");
            }

            return value;
        }

        private static string GetAny(params string[] keys)
        {
            foreach (var key in keys)
            {
                if (Values.TryGetValue(key, out string value) && !string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }

        private static string GetSuffix(EmailLanguage language)
        {
            switch (language)
            {
                case EmailLanguage.Spanish:
                    return "es";
                case EmailLanguage.Portuguese:
                    return "pt";
                case EmailLanguage.French:
                    return "fr";
                default:
                    return "en";
            }
        }

        private static string GetLocalized(string baseKey, string cultureCode)
        {
            EmailLanguage lang = EmailLanguageMapper.FromCultureCode(cultureCode);
            string suffix = GetSuffix(lang);

            string localizedKey = $"{baseKey}_{suffix}";

            if (Values.TryGetValue(localizedKey, out string localizedValue) &&
                !string.IsNullOrWhiteSpace(localizedValue))
            {
                return localizedValue;
            }

            if (Values.TryGetValue($"{baseKey}_en", out string fallbackEn) &&
                !string.IsNullOrWhiteSpace(fallbackEn))
            {
                return fallbackEn;
            }

            if (Values.TryGetValue(baseKey, out string fallbackLegacy) &&
                !string.IsNullOrWhiteSpace(fallbackLegacy))
            {
                return fallbackLegacy;
            }

            return string.Empty;
        }

        public static string GetVerificationSubject(string cultureCode)
        {
            return GetLocalized("VerificationSubject", cultureCode);
        }

        public static string GetVerificationBody(string cultureCode)
        {
            return GetLocalized("VerificationBody", cultureCode);
        }

        public static string GetInvitationSubject(string cultureCode)
        {
            return GetLocalized("InvitationSubject", cultureCode);
        }

        public static string GetInvitationBody(string cultureCode)
        {
            return GetLocalized("InvitationBody", cultureCode);
        }

        public static string GetWelcomeSubject(string cultureCode)
        {
            return GetLocalized("WelcomeSubject", cultureCode);
        }

        public static string GetWelcomeBodyTemplate(string cultureCode)
        {
            return GetLocalized("WelcomeBodyTemplate", cultureCode);
        }

        public static async Task<bool> SendAsync(string receiver, string subject, string body, bool html = true)
        {
            using (var smtp = new SmtpClient(SmtpHost)
            {
                Port = SmtpPort,
                Credentials = new NetworkCredential(SenderEmail, SenderPassword),
                EnableSsl = EnableSsl
            })
            using (var message = new MailMessage())
            {
                message.From = new MailAddress(SenderEmail);
                message.To.Add(receiver);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = html;

                await smtp.SendMailAsync(message);
            }

            return true;
        }

        public static async Task SendWelcomeAsync(UserInfo user)
        {
            string subject = WelcomeSubject;
            string body = string.Format(WelcomeBodyTemplate, user.FullName, user.Username);

            await SendAsync(user.Email, subject, body, true);
        }

        public static async Task SendWelcomeAsync(UserInfo user, string cultureCode)
        {
            string subject = GetWelcomeSubject(cultureCode);
            string template = GetWelcomeBodyTemplate(cultureCode);
            string body = string.Format(template, user.FullName, user.Username);

            await SendAsync(user.Email, subject, body, true);
        }
    }
}
