using System.Runtime.Serialization;

namespace DamasChinas_Server.Dtos
{
    [DataContract]
    public sealed class CreateLobbyRequest
    {
        [DataMember]
        public LobbyVisibility Visibility { get; set; }

        /// <summary>
        /// Debe ser 2, 4 o 6.
        /// </summary>
        [DataMember]
        public int MaxPlayers { get; set; }
    }
}
