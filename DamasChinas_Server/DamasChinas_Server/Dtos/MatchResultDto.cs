using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DamasChinas_Server.Dtos
{
    [DataContract]
    public class MatchResultDto
    {
        [DataMember]
        public int MatchId { get; set; }

        // Username → posición final
        [DataMember]
        public Dictionary<string, int> FinalPositions { get; set; }

        // Jugadores sancionados
        [DataMember]
        public List<BanInfoDto> BansApplied { get; set; }
    }
}
