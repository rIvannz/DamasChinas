using DamasChinas_Server.Game; 
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DamasChinas_Server.Dtos
{
    [DataContract]
    public class HexCoordinateDto
    {
        [DataMember]
        public int X { get; set; }
        [DataMember]
        public int Y { get; set; }
        [DataMember]
        public int Z { get; set; }
    }

    [DataContract]
    public class MoveRequestDto
    {
        [DataMember]
        public int LobbyCode { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public HexCoordinateDto Origin { get; set; }
        [DataMember]
        public HexCoordinateDto Destination { get; set; }
    }

    [DataContract]
    public class MatchStateDto
    {
        [DataMember]
        public bool IsActive { get; set; }
        [DataMember]
        public string CurrentTurnPlayer { get; set; } 
        [DataMember]
        public Dictionary<string, HexCoordinateDto[]> BoardPieces { get; set; } 
    }

    [DataContract]
    public class TurnChangeDto
    {
        [DataMember]
        public string PreviousPlayer { get; set; }

        [DataMember]
        public string NextPlayer { get; set; }

        [DataMember]
        public HexCoordinateDto MoveOrigin { get; set; }

        [DataMember]
        public HexCoordinateDto MoveDestination { get; set; }


        [DataMember]
        public MatchStateDto BoardState { get; set; }
    }

}