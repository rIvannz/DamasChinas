using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamasChinas_Server.Common
{
    public enum MessageCode
    {
        // ========================
        // Éxitos
        // ========================
        Success = 0,

        // ========================
        // Autenticación / Usuarios
        // ========================
        LoginInvalidCredentials = 1001,
        UserDuplicateEmail = 1002,
        UserNotFound = 1003,

        // ========================
        // Partidas / Lobby
        // ========================
        MatchCreationFailed = 1100,

        // ========================
        // Backend / Servidor
        // ========================
        ServerUnavailable = 2001,
        NetworkLatency = 2100,
        UnknownError = 9999,

        // ========================
        // Validaciones universales
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
        SoundVolumeInvalid = 3011,
        OperationInterrupted = 3012,
        CodeSendingError = 3013,
        CodeSentSuccessfully = 3014,
        ChatUnavailable = 3015,

        // ========================
        // Sonido
        // ========================
        SoundSettingsUpdated = 4001,
        SoundSettingsError = 4002,
    }
}



