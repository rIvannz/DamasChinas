using System.Runtime.Serialization;

namespace DamasChinas_Shared.Contracts.Dtos
{
    [DataContract]
    public sealed class ReportPlayerRequest
    {
        [DataMember]
        public int? CodigoLobby { get; set; }   // antes LobbyCode

        [DataMember]
        public int? IdPartida { get; set; }     // NUEVO

        [DataMember]
        public string ReporterUsername { get; set; }

        [DataMember]
        public string ReportedUsername { get; set; }

        [DataMember]
        public string Reason { get; set; }
    }
}
