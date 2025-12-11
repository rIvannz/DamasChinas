using System.Runtime.Serialization;

namespace DamasChinas_Server.Dtos
{
    [DataContract]
    public sealed class LobbySnapshotDto
    {
        [DataMember]
        public int LobbyCode { get; set; }

        [DataMember]
        public LobbyVisibility Visibility { get; set; }

        [DataMember]
        public int MaxPlayers { get; set; }

        [DataMember]
        public LobbyMemberDto[] Members { get; set; }

        [DataMember]
        public bool IsGameStarted { get; set; }
    }
}
