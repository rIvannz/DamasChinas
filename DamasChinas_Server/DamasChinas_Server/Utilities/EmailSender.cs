using DamasChinas_Server.Utilidades;
using System;

namespace DamasChinas_Server.Utilities
{
    internal static class EmailSender
    {
        private static Email _emailService;

        /// <summary>
        /// Debe llamarse una sola vez al iniciar el servidor.
        /// </summary>
        public static void Configure(IEmailSender sender)
        {
            _emailService = new Email(sender);
        }

        public static void SendVerificationEmail(string email, string code)
        {
            if (_emailService == null)
            {
                throw new InvalidOperationException("EmailSender no ha sido configurado. Llama Configure() primero.");
            }

            try
            {
                var subject = "Código de verificación";
                var body =
                    $"Tu código de verificación es: <b>{code}</b><br>" +
                    "Este código expirará en 5 minutos.";

                _emailService.SendAsync(email, subject, body, html: true)
                             .GetAwaiter()
                             .GetResult();

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


