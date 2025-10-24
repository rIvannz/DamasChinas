using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Damas_Chinas_Server.Utilidades
{
    internal static class Correo
    {
        // Tu correo y contraseña de aplicación
        private static readonly string sender = "damaschinas4u@gmail.com";
        private static readonly string password = "prfd slyq tppc mlni"; // la protección la hare espués

        /// <summary>
        /// Envía un correo usando Gmail
        /// </summary>
        public static async Task<bool> SendAsync(string reciver, string subject, string body, bool html = true)
        {
            try
            {
                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(sender, password),
                    EnableSsl = true
                })
                using (MailMessage mensaje = new MailMessage())
                {
                    mensaje.From = new MailAddress(sender);
                    mensaje.To.Add(reciver);
                    mensaje.Subject = subject;
                    mensaje.Body = body;
                    mensaje.IsBodyHtml = html;

                    await smtp.SendMailAsync(mensaje);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al enviar correo: " + ex.Message, ex);
            }
        }
    }
}
