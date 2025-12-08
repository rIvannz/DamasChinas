namespace DamasChinas_Server.Common
{
    public enum MessageCode
    {
        // ========================
        // Éxito
        // ========================
        Success = 0,
        AvatarUpdateSuccess = 5002,
        CodeSentSuccessfully = 3014,

        // ========================
        // Autenticación
        // ========================
        LoginInvalidCredentials = 1001,
        UserDuplicateEmail = 1002,
        UserNotFound = 1003,

        UserValidationError = 1201,
        VerificationCodeNotFound = 1202,
        VerificationCodeExpired = 1203,
        VerificationCodeInvalid = 1204,
        VerificationCodeSendError = 1205,

        // ========================
        // Lobby
        // ========================
        MatchCreationFailed = 1100,
        LobbyNotFound = 1101,
        LobbyInactive = 1102,
        LobbyUserBanned = 1103,
        LobbyClosed = 1104,
        LobbyFull = 1105,
        LobbyAlreadyInLobby = 1106,
        LobbyInvalidMaxPlayers = 1107,
        LobbyNotHost = 1108,
        LobbyGameAlreadyStarted = 1109,
        LobbyMinPlayersNotReached = 1110,
        LobbyPlayerAlreadyReported = 1111,
        LobbyStartFailed = 1112,
        LobbyInvitationFailed = 1113,
        LobbyInvitationTargetNotOnline = 1114,
        LobbyKicked = 1115,


        // ========================
        // Servidor
        // ========================
        ServerUnavailable = 2001,
        InvalidMove = 2002,

        NetworkLatency = 2100,
        UnknownError = 9999,

        // ========================
        // Validaciones generales
        // ========================
        EmptyCredentials = 3001,
        PasswordsDontMatch = 3002,
        InvalidPassword = 3003,
        UsernameEmpty = 3004,
        UserProfileNotFound = 3005,
        FriendsLoadError = 3006,
        InvalidEmail = 3007,
        FieldLengthExceeded = 3008,
        ChatOpenError = 3009,
        NavigationError = 3010,

        // ========================
        // Validaciones específicas
        // ========================

        // Nombre
        InvalidNameEmpty = 3100,
        InvalidNameLength = 3101,
        InvalidNameCharacters = 3102,

        // Username
        InvalidUsernameEmpty = 3110,
        InvalidUsernameLength = 3111,
        InvalidUsernameCharacters = 3112,
        UsernameExists = 3113,

        // Password
        InvalidPasswordEmpty = 3120,
        InvalidPasswordLength = 3121,
        InvalidPasswordUppercase = 3122,
        InvalidPasswordLowercase = 3123,
        InvalidPasswordDigit = 3124,
        InvalidPasswordSpecial = 3125,

        // Email
        InvalidEmailEmpty = 3130,
        InvalidEmailTooLong = 3131,
        InvalidEmailFormat = 3132,

        // ========================
        // Avatar
        // ========================
        AvatarUpdateFailed = 5001
    }
}
