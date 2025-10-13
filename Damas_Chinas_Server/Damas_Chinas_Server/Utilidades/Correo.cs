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
        private static readonly string remitente = "damaschinas4u@gmail.com";
        private static readonly string contrasenaApp = "prfd slyq tppc mlni"; // la protección la hare espués

        /// <summary>
        /// Envía un correo usando Gmail
        /// </summary>
        public static async Task<bool> EnviarAsync(string destinatario, string asunto, string cuerpo, bool html = true)
        {
            try
            {
                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(remitente, contrasenaApp),
                    EnableSsl = true
                })
                using (MailMessage mensaje = new MailMessage())
                {
                    mensaje.From = new MailAddress(remitente);
                    mensaje.To.Add(destinatario);
                    mensaje.Subject = asunto;
                    mensaje.Body = cuerpo;
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
