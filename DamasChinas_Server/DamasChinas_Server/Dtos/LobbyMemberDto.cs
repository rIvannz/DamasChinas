using System.Runtime.Serialization;

namespace DamasChinas_Server.Dtos
{
    [DataContract]
    public sealed class LobbyMemberDto
    {
        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string AvatarFile { get; set; }

        [DataMember]
        public bool IsHost { get; set; }
    }
}
