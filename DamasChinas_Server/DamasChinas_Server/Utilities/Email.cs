using System;
using System.Diagnostics;
using System.Net.Mail;
using System.Threading.Tasks;
using DamasChinas_Server.Dtos;

namespace DamasChinas_Server.Utilities
{
    public class Email
    {
        private readonly IEmailSender _sender;

        private const string WelcomeSubject = "Bienvenido a Damas Chinas";
        private const string WelcomeBodyTemplate =
            "Hola {0},<br><br>" +
            "Tu usuario es <b>{1}</b>.<br>" +
            "Ya puedes iniciar sesión en la plataforma de Damas Chinas y disfrutar del juego.<br><br>" +
            "¡Nos alegra tenerte con nosotros!<br><br>" +
            "Atentamente,<br>Equipo Damas Chinas";

        public Email(IEmailSender sender)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public async Task<bool> SendAsync(string receiver, string subject, string body, bool html = true)
        {
            try
            {
                bool result = await _sender.SendAsync(receiver, subject, body, html);
                Debug.WriteLine("[TRACE] Email sent successfully.");
                return result;
            }
            catch (SmtpException smtpEx)
            {
                Debug.WriteLine($"[ERROR] SMTP error sending email: {smtpEx.StatusCode} - {smtpEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Unexpected error sending email: {ex.Message}");
                throw;
            }
        }

        public async Task SendWelcomeAsync(UserInfo user)
        {
            string subject = WelcomeSubject;
            string body = string.Format(WelcomeBodyTemplate, user.FullName, user.Username);
            await SendAsync(user.Email, subject, body, true);
        }
    }
}
