using System.Runtime.Serialization;

namespace DamasChinas_Server.Dtos
{
    [DataContract]
    public sealed class JoinLobbyRequest
    {
        [DataMember]
        public int LobbyCode { get; set; }

        [DataMember]
        public string Username { get; set; }
    }
}
