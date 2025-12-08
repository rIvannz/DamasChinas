using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DamasChinas_Server.Dtos
{
    [DataContract]
    public sealed class FinishMatchRequest
    {
        // Lobby en el que se jugó la partida (para poder relacionar si hace falta)
        [DataMember]
        public int LobbyCode { get; set; }

        // Posiciones finales: Username → Posición (1,2,3,...)
        [DataMember]
        public Dictionary<string, int> FinalPositions { get; set; }

        // Lista de reportes generados en la partida
        // Reusamos tu DTO ReportPlayerRequest
        [DataMember]
        public List<ReportPlayerRequest> Reports { get; set; }

        public FinishMatchRequest()
        {
            FinalPositions = new Dictionary<string, int>();
            Reports = new List<ReportPlayerRequest>();
        }
    }
}
