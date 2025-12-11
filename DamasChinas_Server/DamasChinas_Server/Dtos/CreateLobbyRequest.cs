using System.Runtime.Serialization;

namespace DamasChinas_Server.Dtos
{
    [DataContract]
    public sealed class CreateLobbyRequest
    {
        [DataMember]
        public LobbyVisibility Visibility { get; set; }

        [DataMember]
        public int MaxPlayers { get; set; }
    }
}
