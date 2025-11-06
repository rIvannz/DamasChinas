using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DamasChinas_Client.UI.AccountManagerServiceProxy;


namespace DamasChinas_Client.UI.Utilities
{

    public static class MessageTranslator
    {
        
        public static string GetLocalizedMessage(MessageCode code)
        {
            try
            {
                string key = GetResourceKey(code);
                object resource = Application.Current.TryFindResource(key);

                
                return resource != null ? resource.ToString() : string.Format("[{0}]", code);
            }
            catch (Exception ex)
            {
       
                System.Diagnostics.Debug.WriteLine(string.Format("[WARN] Missing localization for {0}: {1}", code, ex.Message));
                return code.ToString();
            }
        }

       
        private static string GetResourceKey(MessageCode code)
        {
            switch (code)
            {
                case MessageCode.Success:
                    return "success";

                case MessageCode.LoginInvalidCredentials:
                    return "loginErrorMessage";

                case MessageCode.UserDuplicateEmail:
                    return "emailExists";

                case MessageCode.UserNotFound:
                    return "userNotFound";

                case MessageCode.MatchCreationFailed:
                    return "matchCreationFailed";

                case MessageCode.ServerUnavailable:
                    return "databaseError";

                case MessageCode.NetworkLatency:
                    return "networkLatency";

                case MessageCode.UnknownError:
                    return "unexpectedError";

                default:
                    return "unexpectedError";
            }
        }
    }
}


