using System.Runtime.Serialization;

namespace DamasChinas_Server.Dtos
{
    [DataContract]
    public sealed class ReportPlayerRequest
    {
        [DataMember]
        public int LobbyCode { get; set; }

        [DataMember]
        public string ReporterUsername { get; set; }

        [DataMember]
        public string ReportedUsername { get; set; }

        [DataMember]
        public string Reason { get; set; }
    }
}
