using System.Runtime.Serialization;


namespace DamasChinas_Server.Dtos
{
    [DataContract]
    public sealed class LobbyInvitationDto
    {
        [DataMember]
        public int LobbyCode { get; set; }

        [DataMember]
        public string HostUsername { get; set; }

        [DataMember]
        public int MaxPlayers { get; set; }
    }
}