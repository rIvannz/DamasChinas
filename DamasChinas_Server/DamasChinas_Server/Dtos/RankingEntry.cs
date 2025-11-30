using System.Runtime.Serialization;

namespace DamasChinas_Server.Dtos
{
    [DataContract]
    public class RankingEntry
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string AvatarFile { get; set; }

        [DataMember]
        public int MatchesPlayed { get; set; }

        [DataMember]
        public int Wins { get; set; }

        [DataMember]
        public int Loses { get; set; }

        [DataMember]
        public double WinRate { get; set; }
    }
}
