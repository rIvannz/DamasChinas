using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamasChinas_Server.Game
{
    /// <summary>
    /// Represents the full Chinese Checkers board with a configurable radius.
    /// </summary>
    public class ChineseCheckersBoard
    {
        private const int DefaultBoardRadius = 4;
        private const int MinimumRadius = 1;
        private const int AdjacentDistance = 1;
        private const int JumpDistance = 2;
        private const int HalfDivisor = 2;
        private const int FirstDistance = 1;
        private const string CenterZoneName = "Center";

        private static readonly IReadOnlyList<(HexCoordinate Direction, PlayerColor Zone)> ZoneDefinitions =
                new List<(HexCoordinate, PlayerColor)>
                {
                                (new HexCoordinate(1, -1, 0), PlayerColor.Red),
                                (new HexCoordinate(-1, 1, 0), PlayerColor.Green),
                                (new HexCoordinate(0, -1, 1), PlayerColor.Blue),
                                (new HexCoordinate(0, 1, -1), PlayerColor.Yellow),
                                (new HexCoordinate(1, 0, -1), PlayerColor.Orange),
                                (new HexCoordinate(-1, 0, 1), PlayerColor.Purple)
                };

        private readonly Dictionary<HexCoordinate, HexCell> _cells;

        public ChineseCheckersBoard(int radius = DefaultBoardRadius)
        {
            if (radius < MinimumRadius)
            {
                throw new ArgumentOutOfRangeException(nameof(radius));
            }

            _cells = GenerateCompleteBoard(radius).ToDictionary(cell => cell.Coordinate);
        }

        public IEnumerable<HexCell> Cells
        {
            get
            {
                return _cells.Values;
            }
        }

        public bool TryGetCell(HexCoordinate coordinate, out HexCell cell)
        {
            return _cells.TryGetValue(coordinate, out cell);
        }

        public HexCell GetCell(HexCoordinate coordinate)
        {
            if (!_cells.TryGetValue(coordinate, out HexCell cell))
            {
                throw new ArgumentException("The coordinate is outside the board.", nameof(coordinate));
            }

            return cell;
        }

        public bool ContainsCoordinate(HexCoordinate coordinate)
        {
            return _cells.ContainsKey(coordinate);
        }

        public IEnumerable<HexCell> GetZoneCells(PlayerColor zone)
        {
            string zoneName = zone.ToString();
            return _cells.Values.Where(cell => string.Equals(cell.Zone, zoneName, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsAdjacentMove(HexCoordinate origin, HexCoordinate destination)
        {
            return origin.DistanceTo(destination) == AdjacentDistance;
        }

        public bool IsJumpMove(HexCoordinate origin, HexCoordinate destination, out HexCoordinate middle)
        {
            int distance = origin.DistanceTo(destination);
            if (distance != JumpDistance)
            {
                middle = default(HexCoordinate);
                return false;
            }

            HexCoordinate difference = destination - origin;
            if ((Math.Abs(difference.X) % HalfDivisor) != 0 ||
                    (Math.Abs(difference.Y) % HalfDivisor) != 0 ||
                    (Math.Abs(difference.Z) % HalfDivisor) != 0)
            {
                middle = default(HexCoordinate);
                return false;
            }

            middle = origin + new HexCoordinate(difference.X / HalfDivisor, difference.Y / HalfDivisor, difference.Z / HalfDivisor);
            return true;
        }

        public void MovePiece(HexCoordinate origin, HexCoordinate destination)
        {
            HexCell originCell = GetCell(origin);
            HexCell destinationCell = GetCell(destination);

            if (!originCell.IsOccupied)
            {
                throw new InvalidOperationException("There is no piece in the origin cell.");
            }

            if (destinationCell.IsOccupied)
            {
                throw new InvalidOperationException("The destination cell is occupied.");
            }

            Piece piece = originCell.RemovePiece();
            destinationCell.PlacePiece(piece);
        }

        public IEnumerable<HexCoordinate> GetNeighbors(HexCoordinate origin)
        {
            foreach (HexCoordinate direction in HexCoordinate.Directions)
            {
                HexCoordinate neighbor = origin + direction;
                if (ContainsCoordinate(neighbor))
                {
                    yield return neighbor;
                }
            }
        }

        private IEnumerable<HexCell> GenerateCompleteBoard(int radius)
        {
            var cells = new List<HexCell>();

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    int z = -x - y;
                    if (Math.Abs(z) <= radius)
                    {
                        cells.Add(new HexCell(new HexCoordinate(x, y, z), CenterZoneName));
                    }
                }
            }

            foreach (var (direction, zone) in ZoneDefinitions)
            {
                AddZoneTip(cells, radius, direction, zone);
            }

            return cells;
        }

        private void AddZoneTip(List<HexCell> cells, int radius, HexCoordinate direction, PlayerColor zone)
        {
            for (int distance = FirstDistance; distance <= radius; distance++)
            {
                int offsetX = direction.X * distance;
                int offsetY = direction.Y * distance;
                int offsetZ = direction.Z * distance;

                for (int x = -radius + distance; x <= radius - distance; x++)
                {
                    for (int y = -radius + distance; y <= radius - distance; y++)
                    {
                        int z = -x - y;
                        if (Math.Abs(z) <= radius - distance)
                        {
                            int newX = x + offsetX;
                            int newY = y + offsetY;
                            int newZ = z + offsetZ;
                            var coordinate = new HexCoordinate(newX, newY, newZ);

                            if (!cells.Exists(cell => cell.Coordinate == coordinate))
                            {
                                cells.Add(new HexCell(coordinate, zone.ToString()));
                            }
                        }
                    }
                }
            }
        }
    }
}
