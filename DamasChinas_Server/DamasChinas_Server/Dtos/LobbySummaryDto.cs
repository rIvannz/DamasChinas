using System.Runtime.Serialization;

namespace DamasChinas_Server.Dtos
{
    [DataContract]
    public sealed class LobbySummaryDto
    {
        [DataMember]
        public int LobbyCode { get; set; }

        [DataMember]
        public string HostUsername { get; set; }

        [DataMember]
        public int CurrentPlayers { get; set; }

        [DataMember]
        public int MaxPlayers { get; set; }

        [DataMember]
        public LobbyVisibility Visibility { get; set; }

        [DataMember]
        public bool IsActive { get; set; }
    }
}
