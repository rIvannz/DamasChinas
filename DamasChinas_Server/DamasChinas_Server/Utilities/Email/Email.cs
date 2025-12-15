using DamasChinas_Server.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace DamasChinas_Server.Utilidades
{
    public class Email
    {
        private static string SenderEmail;
        private static string SenderPassword;
        private static string SmtpHost;
        private static int SmtpPort;
        private static bool EnableSsl;
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
            System.Diagnostics.Debug.WriteLine("LOAD PATH = " + path);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("No se encontró emailSettings.config", path);
            }

            var values = new Dictionary<string, string>();
            var lines = File.ReadAllLines(path);

            foreach (var raw in lines)
            {
                string line = raw.Trim();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (!line.Contains("="))
                {
                    continue;
                }

                var parts = line.Split(new[] { '=' }, 2);

                if (parts.Length == 2)
                {
                    values[parts[0].Trim()] = parts[1].Trim();
                }
            }

            foreach (var key in values.Keys)
            {
                System.Diagnostics.Debug.WriteLine("DICTIONARY KEY: " + key);
            }

            System.Diagnostics.Debug.WriteLine("TOTAL KEYS = " + values.Count);

            // Esto imprime la ruta EXACTA que está cargando.
            System.Diagnostics.Debug.WriteLine("LOADING FILE FROM: " + path);


            SenderEmail = values["SenderEmail"];
            SenderPassword = values["SenderPassword"];
            SmtpHost = values["SmtpHost"];
            SmtpPort = int.Parse(values["SmtpPort"]);
            EnableSsl = bool.Parse(values["EnableSsl"]);
            WelcomeSubject = values["WelcomeSubject"];
            WelcomeBodyTemplate = values["WelcomeBodyTemplate"];
            VerificationSubject = values["VerificationSubject"];
            VerificationBody = values["VerificationBody"];

        }

        public static async Task<bool> SendAsync(string receiver, string subject, string body, bool html = true)
        {
            try
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
            catch
            {
                throw;
            }
        }

        public static async Task SendWelcomeAsync(UserInfo user)
        {
            string subject = WelcomeSubject;
            string body = string.Format(WelcomeBodyTemplate, user.FullName, user.Username);

            await SendAsync(user.Email, subject, body, true);
        }
    }
}
