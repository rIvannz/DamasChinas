using DamasChinas_Server.Utilidades;
using System;

namespace DamasChinas_Server.Utilities
{
    internal static class EmailSender
    {
        /// <summary>
        /// Envía el correo con el código de verificación.
        /// </summary>
        public static void SendVerificationEmail(string email, string code)
        {
            try
            {
                var subject = "Código de verificación";
                var body =
                    $"Tu código de verificación es: <b>{code}</b><br>" +
                    "Este código expirará en 5 minutos.";

                Email.SendAsync(email, subject, body, html: true).GetAwaiter().GetResult();

                System.Diagnostics.Debug.WriteLine($"[TRACE] Verification email sent to: {email}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[ERROR] Failed to send verification email to {email}: {ex.Message}");
                throw;
            }
        }
    }
}