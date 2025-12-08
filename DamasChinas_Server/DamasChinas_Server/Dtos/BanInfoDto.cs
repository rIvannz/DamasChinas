using System;
using System.Runtime.Serialization;

namespace DamasChinas_Server.Dtos
{
    [DataContract]
    public sealed class BanInfoDto
    {
        [DataMember]
        public bool IsBanned { get; set; }

        [DataMember]
        public bool IsPermanent { get; set; }

        [DataMember]
        public DateTime? BanUntilUtc { get; set; }

        [DataMember]
        public int TotalReports { get; set; }
    }
}
