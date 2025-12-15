using DamasChinas_Server.Utilidades;
using System;

namespace DamasChinas_Server.Utilities
{
    public static class EmailSender
    {
        /// <summary>
        /// Envía el correo con el código de verificación.
        /// </summary>
        public static void SendVerificationEmail(string email, string code)
        {
            try
            {
                string subject = Email.VerificationSubjectValue;
                string body = string.Format(Email.VerificationBodyValue, code);

                Email.SendAsync(email, subject, body, html: true)
                     .GetAwaiter()
                     .GetResult();

                System.Diagnostics.Debug.WriteLine($"[TRACE] Verification email sent");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[ERROR] Failed to send verification email");
                throw;
            }
        }

        public static void SendInvitationEmail(string friendEmail, string friendUsername, string hostUsername, int lobbyCode)
        {
            try
            {
                string subject = Email.InvitationSubjectValue;
                string body = string.Format(
                    Email.InvitationBodyValue,
                    friendUsername,
                    hostUsername,
                    lobbyCode
                );

                Email.SendAsync(friendEmail, subject, body, html: true)
                     .GetAwaiter()
                     .GetResult();

                System.Diagnostics.Debug.WriteLine("[TRACE] Lobby invitation email sent");
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("[ERROR] Failed to send lobby invitation email");
                throw;
            }

        }
        public static void SendInvitationGameEmail(string friendEmail, string friendUsername, string hostUsername, int lobbyCode)
        {
            try
            {
                string subject = Email.InvitationSubjectValue;
                string body = string.Format(
                    Email.InvitationBodyValue,
                    friendUsername,
                    hostUsername,
                    lobbyCode
                );

                Email.SendAsync(friendEmail, subject, body, html: true)
                     .GetAwaiter()
                     .GetResult();

                System.Diagnostics.Debug.WriteLine("[TRACE] Lobby invitation email sent");
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("[ERROR] Failed to send lobby invitation email");
                throw;
            }

        }
    }
}