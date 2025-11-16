using System;
using System.Windows;

// YA NO debes importar un MessageCode específico aquí
// porque vienen de diferentes proxies

namespace DamasChinas_Client.UI.Utilities
{
    public static class MessageTranslator
    {
        // ============================================================
        // 1. Traducción usando claves directas (UI)
        // ============================================================
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

        // ============================================================
        // 2. Traducción basada en MessageCode (proveniente del servidor)
        // ============================================================
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
                return code.ToString();
            }
        }

        // ============================================================
        // 3. Tabla completa MessageCode y clave en el diccionario
        // ============================================================
        private static string GetResourceKey(Enum code)
        {
            switch (code.ToString())
            {
                // Éxito
                case "Success": return "msg_Success";

                // Login / Usuarios
                case "LoginInvalidCredentials": return "msg_LoginInvalidCredentials";
                case "UserDuplicateEmail": return "msg_UserDuplicateEmail";
                case "UserNotFound": return "msg_UserNotFound";

                // Lobby / Partidas
                case "MatchCreationFailed": return "msg_MatchCreationFailed";

                // Backend
                case "ServerUnavailable": return "msg_ServerUnavailable";
                case "NetworkLatency": return "msg_NetworkLatency";
                case "UnknownError": return "msg_UnknownError";

                // Validaciones universales
                case "EmptyCredentials": return "msg_EmptyCredentials";
                case "PasswordsDontMatch": return "msg_PasswordsDontMatch";
                case "InvalidPassword": return "msg_InvalidPassword";
                case "UsernameEmpty": return "msg_UsernameEmpty";
                case "UserProfileNotFound": return "msg_UserProfileNotFound";
                case "FriendsLoadError": return "msg_FriendsLoadError";
                case "InvalidEmail": return "msg_InvalidEmail";
                case "FieldLengthExceeded": return "msg_FieldLengthExceeded";
                case "ChatOpenError": return "msg_ChatOpenError";
                case "NavigationError": return "msg_NavigationError";
                case "SoundVolumeInvalid": return "msg_SoundVolumeInvalid";
                case "OperationInterrupted": return "msg_OperationInterrupted";
                case "CodeSendingError": return "msg_CodeSendingError";
                case "CodeSentSuccessfully": return "msg_CodeSentSuccessfully";
                case "ChatUnavailable": return "msg_ChatUnavailable";

                // Sonido
                case "SoundSettingsUpdated": return "msg_SoundSettingsUpdated";
                case "SoundSettingsError": return "msg_SoundSettingsError";

                default:
                    return "msg_UnknownError";
            }
        }
    }
}
