using DamasChinas_Server.Dtos;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace DamasChinas_Server.Utilidades
{
<<<<<<< HEAD
    public class Email
=======
    internal static class Email
>>>>>>> 096d4752533982c9398c1b7e38f37cff37a7bfd7
    {
        private const string SmtpHost = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const bool EnableSsl = true;
        private const string SenderEmail = "damaschinas4u@gmail.com";
        private const string SenderPassword = "prfd slyq tppc mlni";
        private const string WelcomeSubject = "Bienvenido a Damas Chinas";
        private const string WelcomeBodyTemplate =
            "Hola {0},<br><br>" +
            "Tu usuario es <b>{1}</b>.<br>" +
            "Ya puedes iniciar sesión en la plataforma de Damas Chinas y disfrutar del juego.<br><br>" +
            "¡Nos alegra tenerte con nosotros!<br><br>" +
            "Atentamente,<br>Equipo Damas Chinas";

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

                System.Diagnostics.Debug.WriteLine("[TRACE] Email sent successfully.");
                return true;
            }
            catch (SmtpException smtpEx)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] SMTP error sending email: {smtpEx.StatusCode} - {smtpEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Unexpected error sending email: {ex.Message}");
                throw;
            }
        }


        public static async Task SendWelcomeAsync(UserInfo user)
        {
            string subject = WelcomeSubject;
            string body = string.Format(WelcomeBodyTemplate, user.FullName, user.Username);

            await SendAsync(user.Email, subject, body, html: true);
        }
    }
}