namespace DamasChinas_Server.Common
{
    public enum MessageCode
    {

        Success = 0,
        AvatarUpdateSuccess = 5002,
        CodeSentSuccessfully = 3014,


        LoginInvalidCredentials = 1001,
        UserDuplicateEmail = 1002,
        UserNotFound = 1003,

        UserValidationError = 1201,
        VerificationCodeNotFound = 1202,
        VerificationCodeExpired = 1203,
        VerificationCodeInvalid = 1204,
        VerificationCodeSendError = 1205,

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

        FriendRequestAlreadyPending = 3200,
        AlreadyFriends = 3201,


        ServerUnavailable = 2001,
        InvalidMove = 2002,
        RankingUnavailable = 2003,

        NetworkLatency = 2100,
        UnknownError = 9999,


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


        InvalidNameEmpty = 3100,
        InvalidNameLength = 3101,
        InvalidNameCharacters = 3102,


        InvalidUsernameEmpty = 3110,
        InvalidUsernameLength = 3111,
        InvalidUsernameCharacters = 3112,
        UsernameExists = 3113,

        InvalidPasswordEmpty = 3120,
        InvalidPasswordLength = 3121,
        InvalidPasswordUppercase = 3122,
        InvalidPasswordLowercase = 3123,
        InvalidPasswordDigit = 3124,
        InvalidPasswordSpecial = 3125,

        InvalidEmailEmpty = 3130,
        InvalidEmailTooLong = 3131,
        InvalidEmailFormat = 3132,

        UserBlocked = 3202,
        NoPieceInOrigin = 4000,
        DestinationCellOccupied = 4001,
        CoordinateOutsideBoard = 4002,
        MatchFinished = 4003,
        PlayerNotInMatch = 4004,
        NotPlayersTurn = 4005,
        OriginCellInvalid = 4006,
        OriginCellNotPlayersPiece = 4007,
        PathCellRepeated = 4008,
        DestinationOutsideBoard = 4009,
        InvalidStep = 4011,
        MultistepRequiresJump = 4012,
        AdjacentMoveSingleStep = 4013,
        NoPieceToJump = 4014,
        MultistepMustHaveJump = 4015,
        InvalidPlayerCount = 4016,
        PlayerColorDuplicate = 4017,
        PlayerTargetZoneMissing = 4018,
        CellAlreadyOccupied = 4019,
        CellHasNoPiece = 4020,
        InvalidCubeCoordinate = 4021,
        InvalidHalfCoordinate = 4022,
        PlayerIdRequired = 4023,
        PlayerNameRequired = 4024,
        RepositoryUsernameEmpty = 4027,
        RepositoryUserNotFound = 4028,






        AvatarUpdateFailed = 5001
    }
}
