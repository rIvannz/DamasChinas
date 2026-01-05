using DamasChinas_Server.Common;
using log4net;
using System;
using System.Net.Mail;
using EmailConfig = DamasChinas_Server.Utilidades.Email;

namespace DamasChinas_Server.Utilities
{
    public static class EmailSender
    {
        public static void SendVerificationEmail(string email, string code)
        {
           
                string subject = EmailConfig.VerificationSubjectValue;
                string body = string.Format(EmailConfig.VerificationBodyValue, code);

                EmailConfig.SendAsync(email, subject, body, html: true)
                    .GetAwaiter()
                    .GetResult();
 
        }

        public static void SendVerificationEmail(string email, string code, string cultureCode)
        {
  
                string subject = EmailConfig.GetVerificationSubject(cultureCode);
                string bodyTemplate = EmailConfig.GetVerificationBody(cultureCode);
                string body = string.Format(bodyTemplate, code);

                EmailConfig.SendAsync(email, subject, body, html: true)
                    .GetAwaiter()
                    .GetResult();

        }


        public static void SendInvitationGameEmail( string friendEmail, string friendUsername, string hostUsername, int lobbyCode, string languageCode)
        {
            string subject = EmailConfig.GetInvitationSubject(languageCode);
            string bodyTemplate = EmailConfig.GetInvitationBody(languageCode);

            string body = string.Format(bodyTemplate, friendUsername, hostUsername, lobbyCode);

            EmailConfig.SendAsync(friendEmail, subject, body, html: true)
                .GetAwaiter()
                .GetResult();
        }




    }

}

