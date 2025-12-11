using System;

namespace DamasChinas_Client.UI.Utilities
{
    public static class MessageKeys
    {
       
        public const string Success = "msg_Success";

    
        public const string LoginInvalidCredentials = "msg_LoginInvalidCredentials";
        public const string UserDuplicateEmail = "msg_UserDuplicateEmail";
        public const string UserNotFound = "msg_UserNotFound";
        public const string UserValidationError = "msg_UserValidationError";

        public const string VerificationCodeNotFound = "msg_VerificationCodeNotFound";
        public const string VerificationCodeExpired = "msg_VerificationCodeExpired";
        public const string InvalidVerificationCode = "msg_InvalidVerificationCode";
        public const string VerificationCodeSendError = "msg_CodeSendingError";

        public const string MatchCreationFailed = "msg_MatchCreationFailed";
        public const string LobbyNotFound = "msg_LobbyNotFound";
        public const string LobbyInactive = "msg_LobbyInactive";
        public const string LobbyUserBanned = "msg_LobbyUserBanned";
        public const string LobbyClosed = "msg_LobbyClosed";


   
        public const string FriendUserNotFound = "msg_FriendUserNotFound";
        public const string FriendRequestAccepted = "msg_FriendRequestAccepted";
        public const string FriendRequestRejected = "msg_FriendRequestRejected";
        public const string NoPendingRequests = "msg_NoPendingRequests";
        public const string FriendRequestSentOk = "friendRequestSentOk";
        public const string FriendRequestReceived = "friendRequestReceived";
        public const string UserBlockedYou = "userBlockedYou";
        public const string UserUnblockedYou = "userUnblockedYou";
        public const string ConfirmRemoveFriend = "confirmRemoveFriend";
        public const string FriendRemovedSuccess = "friendRemovedSuccess";
        public const string ConfirmBlockUser = "confirmBlockUser";
        public const string UserBlockedSuccess = "userBlockedSuccess";
        public const string FriendRequestAlreadyPending = "msg_FriendRequestAlreadyPending";
        public const string AlreadyFriends = "msg_AlreadyFriends";



        public const string ServerUnavailable = "msg_ServerUnavailable";
        public const string NetworkLatency = "msg_NetworkLatency";
        public const string UnknownError = "msg_UnknownError";

     
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
        public const string RankingUnavailable = "msg_RankingUnavailable";

     
       
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

     

        public const string PlayerLeftMatch = "playerLeftMatch";
        public const string StatusTurn = "status_Turn";
        public const string StatusWaiting = "status_Waiting";
        public const string StatusDisconnected = "status_Disconnected";


        public const string InvalidMove = "msg_InvalidMove";
        public const string NotYourTurn = "msg_NotYourTurn";
        public const string CellOccupied = "msg_CellOccupied";
        public const string MoveNotAllowed = "msg_MoveNotAllowed";

        public const string GameFinishedTitle = "gameFinishedTitle";
        public const string GameWinnerLabel = "gameWinnerLabel";

        public const string PrivateLobby = "private";
        public const string PublicLobby = "public";

        public const string NoLobbySelected = "noLobbySelected";
        public const string InvalidCodeWarning = "invalidCodeWarning";

        public const string JoiningLobbyError = "joiningLobbyError";
        public const string LobbyCodeUnknown = "msg_LobbyCodeUnknown";
        public const string ReportReasonLobby = "msg_ReportReasonLobby";


        public const string YouWereKicked = "msg_YouWereKicked";
        public const string PlayerKicked = "msg_PlayerKicked";
        public const string PlayerReported = "msg_PlayerReported";
        public const string OnlyHostCanKick = "msg_OnlyHostCanKick";
        public const string OnlyHostCanStart = "msg_OnlyHostCanStart";
        public const string LobbyCode = "lobbyCode";
        public const string PlayersCount = "PlayersCount";
        public const string HostDisconnected = "msg_HostDisconnected";

        public const string ErrorStartingGame = "msg_ErrorStartingGame";
        public const string InvalidNameEmpty = "msg_InvalidNameEmpty";
        public const string InvalidNameLength = "msg_InvalidNameLength";
        public const string InvalidNameCharacters = "msg_InvalidNameCharacters";

        public const string InvalidUsernameEmpty = "msg_InvalidUsernameEmpty";
        public const string InvalidUsernameLength = "msg_InvalidUsernameLength";
        public const string InvalidUsernameCharacters = "msg_InvalidUsernameCharacters";
        public const string InvalidPasswordEmpty = "msg_InvalidPasswordEmpty";
        public const string InvalidPasswordLength = "msg_InvalidPasswordLength";
        public const string InvalidPasswordUppercase = "msg_InvalidPasswordUppercase";
        public const string InvalidPasswordLowercase = "msg_InvalidPasswordLowercase";
        public const string InvalidPasswordDigit = "msg_InvalidPasswordDigit";
        public const string InvalidPasswordSpecial = "msg_InvalidPasswordSpecial";
        public const string InvalidEmailEmpty = "msg_InvalidEmailEmpty";
        public const string InvalidEmailTooLong = "msg_InvalidEmailTooLong";
        public const string InvalidEmailFormat = "msg_InvalidEmailFormat";

        public const string GuestProfile = "msg_GuestProfile";
        public const string GuestNoEmail = "msg_GuestNoEmail";
        public const string GuestProfileUnavailable = "msg_GuestProfileUnavailable";
        public const string GuestAccountWarning = "msg_GuestAccountWarning";
        public const string GuestProfileTitle = "guestProfileTitle";
        public const string SessionNotInitialized = "msg_SessionNotInitialized";
        public const string InvalidUsername = "msg_InvalidUsernameArgument";





        public const string BanTemp10m = "ban_temp_10m";
        public const string BanTemp1h = "ban_temp_1h";
        public const string BanPermanent = "ban_permanent";
        public const string UserBlocked = "msg_UserBlocked";

        public const string LanguageChangeSuccess = "msg_LanguageChangeSuccess";
        public const string SelectLanguageFirst = "msg_SelectLanguageFirst";

        public const string SoundSettingsUpdated = "msg_SoundSettingsUpdated";
        public const string LanguageChangeError = "msg_LanguageChangeError";
        public const string SoundSettingsError = "msg_SoundSettingsError";
    }
}

