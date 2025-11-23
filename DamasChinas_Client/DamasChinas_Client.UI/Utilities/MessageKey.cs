using System;

namespace DamasChinas_Client.UI.Utilities
{
    public static class MessageKeys
    {
        // ========================
        // Éxitos
        // ========================
        public const string Success = "msg_Success";

        // ========================
        // Autenticación / Usuarios
        // ========================
        public const string LoginInvalidCredentials = "msg_LoginInvalidCredentials";
        public const string UserDuplicateEmail = "msg_UserDuplicateEmail";
        public const string UserNotFound = "msg_UserNotFound";
        public const string UserValidationError = "msg_UserValidationError";

        public const string VerificationCodeNotFound = "msg_VerificationCodeNotFound";
        public const string VerificationCodeExpired = "msg_VerificationCodeExpired";
        public const string InvalidVerificationCode = "msg_InvalidVerificationCode";
        public const string VerificationCodeSendError = "msg_CodeSendingError";

        // ========================
        // Partidas / Lobby
        // ========================
        public const string MatchCreationFailed = "msg_MatchCreationFailed";
        public const string LobbyNotFound = "msg_LobbyNotFound";
        public const string LobbyInactive = "msg_LobbyInactive";
        public const string LobbyUserBanned = "msg_LobbyUserBanned";
        public const string LobbyClosed = "msg_LobbyClosed";

        // ========================
        // Backend / Servidor
        // ========================
        public const string ServerUnavailable = "msg_ServerUnavailable";
        public const string NetworkLatency = "msg_NetworkLatency";
        public const string UnknownError = "msg_UnknownError";

        // ========================
        // Validaciones universales
        // ========================
        public const string EmptyCredentials = "msg_EmptyCredentials";
        public const string PasswordsDontMatch = "msg_PasswordsDontMatch";
        public const string InvalidPassword = "msg_InvalidPassword";
        public const string UsernameEmpty = "msg_UsernameEmpty";
        public const string UserProfileNotFound = "msg_UserProfileNotFound";
        public const string FriendsLoadError = "msg_FriendsLoadError";
        public const string InvalidEmail = "msg_InvalidEmail";
        public const string FieldLengthExceeded = "msg_FieldLengthExceeded";
        public const string ChatOpenError = "msg_ChatOpenError";
        public const string NavigationError = "msg_NavigationError";
        public const string SoundVolumeInvalid = "msg_SoundVolumeInvalid";
        public const string OperationInterrupted = "msg_OperationInterrupted";
        public const string CodeSentSuccessfully = "msg_CodeSentSuccessfully";
        public const string ChatUnavailable = "msg_ChatUnavailable";
        public const string UsernameExists = "msg_UsernameExists";

        // ========================
        // Validaciones específicas – Nombre
        // ========================
        public const string InvalidNameEmpty = "msg_InvalidNameEmpty";
        public const string InvalidNameLength = "msg_InvalidNameLength";
        public const string InvalidNameCharacters = "msg_InvalidNameCharacters";

        // ========================
        // Validaciones específicas – Username
        // ========================
        public const string InvalidUsernameEmpty = "msg_InvalidUsernameEmpty";
        public const string InvalidUsernameLength = "msg_InvalidUsernameLength";
        public const string InvalidUsernameCharacters = "msg_InvalidUsernameCharacters";

        // ========================
        // Validaciones específicas – Password
        // ========================
        public const string InvalidPasswordEmpty = "msg_InvalidPasswordEmpty";
        public const string InvalidPasswordLength = "msg_InvalidPasswordLength";
        public const string InvalidPasswordUppercase = "msg_InvalidPasswordUppercase";
        public const string InvalidPasswordLowercase = "msg_InvalidPasswordLowercase";
        public const string InvalidPasswordDigit = "msg_InvalidPasswordDigit";
        public const string InvalidPasswordSpecial = "msg_InvalidPasswordSpecial";

        // ========================
        // Validaciones específicas – Email
        // ========================
        public const string InvalidEmailEmpty = "msg_InvalidEmailEmpty";
        public const string InvalidEmailTooLong = "msg_InvalidEmailTooLong";
        public const string InvalidEmailFormat = "msg_InvalidEmailFormat";

        // ========================
        // Sonido
        // ========================
        public const string SoundSettingsUpdated = "msg_SoundSettingsUpdated";
        public const string SoundSettingsError = "msg_SoundSettingsError";
    }
}
