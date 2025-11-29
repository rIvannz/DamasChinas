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
        // Amigos
        // ========================
        public const string FriendUserNotFound = "msg_FriendUserNotFound";
        public const string FriendRequestAccepted = "msg_FriendRequestAccepted";
        public const string FriendRequestRejected = "msg_FriendRequestRejected";
        public const string NoPendingRequests = "msg_NoPendingRequests";
        public const string FriendRequestSentOk = "friendRequestSentOk";



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
        // UI / Menús (nuevos keys)
        // ========================
        public const string TutorialUnavailable = "msg_TutorialUnavailable";
        public const string JoinPartyOpenError = "msg_JoinPartyOpenError";
        public const string CreateLobbyError = "msg_CreateLobbyError";
        public const string ProfileOpenError = "msg_ProfileOpenError";
        public const string StatsUnavailable = "msg_StatsUnavailable";
        public const string FriendsOpenError = "msg_FriendsOpenError";
        public const string GuestFeatureOnly = "msg_GuestFeatureOnly";
        public const string GuestStatsUnavailable = "msg_GuestStatsUnavailable";
        public const string ProfileChangeError = "msg_ProfileChangeError";
        public const string FriendRemoved = "msg_FriendRemoved";
        public const string ChatComingSoon = "msg_ChatComingSoon";

        // ========================
        // Lobby / Join Party
        // ========================
        public const string PrivateLobby = "private";
        public const string PublicLobby = "public";

        public const string NoLobbySelected = "noLobbySelected";
        public const string InvalidCodeWarning = "invalidCodeWarning";

        public const string JoiningLobbyError = "joiningLobbyError";

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
        // Guest / Invitados
        // ========================
        public const string GuestProfile = "msg_GuestProfile";
        public const string GuestNoEmail = "msg_GuestNoEmail";
        public const string GuestProfileUnavailable = "msg_GuestProfileUnavailable";
        public const string GuestAccountWarning = "msg_GuestAccountWarning";
        public const string GuestProfileTitle = "guestProfileTitle";




        // ========================
        // Lenguaje
        // ========================
        public const string LanguageChangeSuccess = "msg_LanguageChangeSuccess";
        public const string SelectLanguageFirst = "msg_SelectLanguageFirst";


        // ========================
        // Sonido
        // ========================
        public const string SoundSettingsUpdated = "msg_SoundSettingsUpdated";
        public const string LanguageChangeError = "msg_LanguageChangeError";
        public const string SoundSettingsError = "msg_SoundSettingsError";
    }
}

