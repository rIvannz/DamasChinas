using System;
using EmailConfig = DamasChinas_Server.Utilidades.Email;

namespace DamasChinas_Server.Utilities
{
    public static class EmailSender
    {
        public static void SendVerificationEmail(string email, string code)
        {
            try
            {
                string subject = EmailConfig.VerificationSubjectValue;
                string body = string.Format(EmailConfig.VerificationBodyValue, code);

                EmailConfig.SendAsync(email, subject, body, html: true)
                    .GetAwaiter()
                    .GetResult();
            }
            catch
            {
                throw;
            }
        }

        public static void SendVerificationEmail(string email, string code, string cultureCode)
        {
            try
            {
                string subject = EmailConfig.GetVerificationSubject(cultureCode);
                string bodyTemplate = EmailConfig.GetVerificationBody(cultureCode);
                string body = string.Format(bodyTemplate, code);

                EmailConfig.SendAsync(email, subject, body, html: true)
                    .GetAwaiter()
                    .GetResult();
            }
            catch
            {
                throw;
            }
        }

        public static void SendInvitationEmail(string friendEmail, string friendUsername, string hostUsername, int lobbyCode)
        {
            try
            {
                string subject = EmailConfig.InvitationSubjectValue;
                string body = string.Format(
                    EmailConfig.InvitationBodyValue,
                    friendUsername,
                    hostUsername,
                    lobbyCode
                );

                EmailConfig.SendAsync(friendEmail, subject, body, html: true)
                    .GetAwaiter()
                    .GetResult();
            }
            catch
            {
                throw;
            }
        }

        public static void SendInvitationEmail(string friendEmail, string friendUsername, string hostUsername, int lobbyCode, string cultureCode)
        {
            try
            {
                string subject = EmailConfig.GetInvitationSubject(cultureCode);
                string bodyTemplate = EmailConfig.GetInvitationBody(cultureCode);
                string body = string.Format(bodyTemplate, friendUsername, hostUsername, lobbyCode);

                EmailConfig.SendAsync(friendEmail, subject, body, html: true)
                    .GetAwaiter()
                    .GetResult();
            }
            catch
            {
                throw;
            }
        }

        public static void SendInvitationGameEmail(string friendEmail, string friendUsername, string hostUsername, int lobbyCode)
        {
            try
            {
                string subject = EmailConfig.InvitationSubjectValue;
                string body = string.Format(
                    EmailConfig.InvitationBodyValue,
                    friendUsername,
                    hostUsername,
                    lobbyCode
                );

                EmailConfig.SendAsync(friendEmail, subject, body, html: true)
                    .GetAwaiter()
                    .GetResult();
            }
            catch
            {
                throw;
            }
        }

        public static void SendInvitationGameEmail(string friendEmail, string friendUsername, string hostUsername, int lobbyCode, string cultureCode)
        {
            try
            {
                string subject = EmailConfig.GetInvitationSubject(cultureCode);
                string bodyTemplate = EmailConfig.GetInvitationBody(cultureCode);
                string body = string.Format(bodyTemplate, friendUsername, hostUsername, lobbyCode);

                EmailConfig.SendAsync(friendEmail, subject, body, html: true)
                    .GetAwaiter()
                    .GetResult();
            }
            catch
            {
                throw;
            }
        }
    }
}
