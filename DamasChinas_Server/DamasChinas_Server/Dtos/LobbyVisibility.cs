using System.Runtime.Serialization;

namespace DamasChinas_Server.Dtos
{
    [DataContract]
    public enum LobbyVisibility
    {
        [EnumMember]
        Public = 0,

        [EnumMember]
        Private = 1
    }
}
