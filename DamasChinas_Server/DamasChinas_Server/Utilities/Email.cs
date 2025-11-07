using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace DamasChinas_Server.Utilidades
{
    internal static class Email
    {
        private const string SmtpHost = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const bool EnableSsl = true;

        private const string SenderEmail = "damaschinas4u@gmail.com";
        private const string SenderPassword = "prfd slyq tppc mlni"; // pendiente de implementar el recurso *++++++++++++++++++++++++++++++++++++++++

        private const string ErrorSendingMail = "Error al enviar correo: ";
        private const string WelcomeSubject = "Bienvenido a Damas Chinas";
        private const string WelcomeBodyTemplate =
            "Hola {0},<br><br>" +
            "Tu usuario es <b>{1}</b>.<br>" +
            "Ya puedes iniciar sesión en la plataforma de Damas Chinas y disfrutar del juego.<br><br>" +
            "¡Nos alegra tenerte con nosotros!<br><br>" +
            "Atentamente,<br>Equipo Damas Chinas";

        /// <summary>
        /// Envía un correo electrónico genérico mediante el servidor SMTP configurado.
        /// </summary>
        public static async Task<bool> SendAsync(string receiver, string subject, string body, bool html = true)
        {
            try
            {
                using (SmtpClient smtp = new SmtpClient(SmtpHost)
                {
                    Port = SmtpPort,
                    Credentials = new NetworkCredential(SenderEmail, SenderPassword),
                    EnableSsl = EnableSsl
                })
                using (MailMessage message = new MailMessage())
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
            catch (Exception ex)
            {
                throw new Exception(ErrorSendingMail + ex.Message, ex);
            }
        }

        /// <summary>
        /// Envía un correo de bienvenida utilizando la plantilla estándar.
        /// </summary>
        public static async Task EnviarBienvenidaAsync(UserInfo user)
        {
            string subject = WelcomeSubject;
            string body = string.Format(WelcomeBodyTemplate, user.FullName, user.Username);

            await SendAsync(user.Email, subject, body, html: true);
        }
    }
}
