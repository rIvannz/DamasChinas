using System;
using DamasChinas_Server.Common;

namespace DamasChinas_Server.Game
{
    public class HexCell
    {
        private Piece _piece;

        public HexCoordinate Coordinate { get; }

        public string Zone { get; }

        public bool IsValid { get; set; } = true;

        public bool IsOccupied
        {
            get
            {
                return _piece != null;
            }
        }

        public Piece Piece
        {
            get
            {
                return _piece;
            }
        }

        public HexCell(HexCoordinate coordinate, string zone)
        {
            Coordinate = coordinate;
            Zone = zone ?? throw new ArgumentNullException(nameof(zone));
        }

        public void PlacePiece(Piece piece)
        {
            if (piece == null)
            {
                throw new ArgumentNullException(nameof(piece));
            }

            if (_piece != null)
            {
                throw new InvalidOperationException(MessageCode.CellAlreadyOccupied.ToString());
            }

            _piece = piece;
        }

        public Piece RemovePiece()
        {
            if (_piece == null)
            {
                throw new InvalidOperationException(MessageCode.CellHasNoPiece.ToString());
            }

            Piece piece = _piece;
            _piece = null;
            return piece;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Coordinate, Zone);
        }
    }
}
