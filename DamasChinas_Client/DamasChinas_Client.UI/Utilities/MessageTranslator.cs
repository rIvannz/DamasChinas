using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace DamasChinas_Client.UI.Utilities
{
   
    public static class MessageTranslator
    {
     

        private static readonly IReadOnlyDictionary<string, string> CodeToResourceKey =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
              
                { "Success", MessageKeys.Success },

             
                { "LoginInvalidCredentials", MessageKeys.LoginInvalidCredentials },
                { "UserDuplicateEmail",      MessageKeys.UserDuplicateEmail },
                { "UserNotFound",            MessageKeys.UserNotFound },
                { "UserValidationError",     MessageKeys.UserValidationError },

                { "VerificationCodeNotFound", MessageKeys.VerificationCodeNotFound },
                { "VerificationCodeExpired",  MessageKeys.VerificationCodeExpired },
                { "VerificationCodeInvalid",  MessageKeys.InvalidVerificationCode },
                { "VerificationCodeSendError", MessageKeys.VerificationCodeSendError },

            
                { "MatchCreationFailed", MessageKeys.MatchCreationFailed },
                { "LobbyNotFound",       MessageKeys.LobbyNotFound },
                { "LobbyInactive",       MessageKeys.LobbyInactive },
                { "LobbyUserBanned",     MessageKeys.LobbyUserBanned },
                { "LobbyClosed",         MessageKeys.LobbyClosed },

           
                { "ServerUnavailable", MessageKeys.ServerUnavailable },
                { "NetworkLatency",    MessageKeys.NetworkLatency },
                { "UnknownError",      MessageKeys.UnknownError },

                
                { "EmptyCredentials",    MessageKeys.EmptyCredentials },
                { "PasswordsDontMatch",  MessageKeys.PasswordsDontMatch },
                { "InvalidPassword",     MessageKeys.InvalidPassword },
                { "UsernameEmpty",       MessageKeys.UsernameEmpty },
                { "UserProfileNotFound", MessageKeys.UserProfileNotFound },
                { "FriendsLoadError",    MessageKeys.FriendsLoadError },
                { "InvalidEmail",        MessageKeys.InvalidEmail },
                { "FieldLengthExceeded", MessageKeys.FieldLengthExceeded },
                { "ChatOpenError",       MessageKeys.ChatOpenError },
                { "NavigationError",     MessageKeys.NavigationError },
                { "SoundVolumeInvalid",  MessageKeys.SoundVolumeInvalid },
                { "OperationInterrupted",MessageKeys.OperationInterrupted },
                { "CodeSendingError",    MessageKeys.VerificationCodeSendError },
                { "CodeSentSuccessfully",MessageKeys.CodeSentSuccessfully },
                { "ChatUnavailable",     MessageKeys.ChatUnavailable },
                { "UsernameExists",      MessageKeys.UsernameExists },

          
                { "InvalidNameEmpty",      MessageKeys.InvalidNameEmpty },
                { "InvalidNameLength",     MessageKeys.InvalidNameLength },
                { "InvalidNameCharacters", MessageKeys.InvalidNameCharacters },

               
                { "InvalidUsernameEmpty",      MessageKeys.InvalidUsernameEmpty },
                { "InvalidUsernameLength",     MessageKeys.InvalidUsernameLength },
                { "InvalidUsernameCharacters", MessageKeys.InvalidUsernameCharacters },

             
                { "InvalidPasswordEmpty",     MessageKeys.InvalidPasswordEmpty },
                { "InvalidPasswordLength",    MessageKeys.InvalidPasswordLength },
                { "InvalidPasswordUppercase", MessageKeys.InvalidPasswordUppercase },
                { "InvalidPasswordLowercase", MessageKeys.InvalidPasswordLowercase },
                { "InvalidPasswordDigit",     MessageKeys.InvalidPasswordDigit },
                { "InvalidPasswordSpecial",   MessageKeys.InvalidPasswordSpecial },

                { "InvalidEmailEmpty",   MessageKeys.InvalidEmailEmpty },
                { "InvalidEmailTooLong", MessageKeys.InvalidEmailTooLong },
                { "InvalidEmailFormat",  MessageKeys.InvalidEmailFormat },

                { "SoundSettingsUpdated", MessageKeys.SoundSettingsUpdated },
                { "SoundSettingsError",   MessageKeys.SoundSettingsError },
                { "FriendRequestAlreadyPending", MessageKeys.FriendRequestAlreadyPending },
                { "AlreadyFriends", MessageKeys.AlreadyFriends },
                { "UserBlocked", MessageKeys.UserBlocked },
                { "DatabaseUnavailable", MessageKeys.DatabaseUnavailable },


            };

     
        public static string GetLocalizedMessage(string resourceKey)
        {
            try
            {
                object resource = Application.Current.TryFindResource(resourceKey);
                return resource != null ? resource.ToString() : resourceKey;
            }
            catch
            {
                return resourceKey;
            }
        }

        public static string GetLocalizedMessage(Enum code)
        {
            try
            {
                string key = GetResourceKey(code);
                object resource = Application.Current.TryFindResource(key);
                return resource != null ? resource.ToString() : $"[{code}]";
            }
            catch
            {
                return code != null ? code.ToString() : MessageKeys.UnknownError;
            }
        }



        private static string GetResourceKey(Enum code)
        {
            if (code == null)
            {
                return MessageKeys.UnknownError;
            }

            string name = code.ToString();

            string resourceKey;
            if (CodeToResourceKey.TryGetValue(name, out resourceKey))
            {
                return resourceKey;
            }

            return MessageKeys.UnknownError;
        }

        
    }
}


