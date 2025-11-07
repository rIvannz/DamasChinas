using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamasChinas_Server.Game
{
    /// <summary>
    /// Represents a player's move request.
    /// </summary>
    public class PlayerMove
    {
        private readonly IReadOnlyList<HexCoordinate> _path;

        public PlayerColor Player { get; }

        public IReadOnlyList<HexCoordinate> Path
        {
            get
            {
                return _path;
            }
        }

        public HexCoordinate Origin
        {
            get
            {
                return _path.First();
            }
        }

        public HexCoordinate Destination
        {
            get
            {
                return _path.Last();
            }
        }

        public PlayerMove(PlayerColor player, IEnumerable<HexCoordinate> path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var coordinates = path.ToList();
            if (coordinates.Count < MinimumMoveLength)
            {
                throw new ArgumentException("A move must contain at least an origin and a destination.", nameof(path));
            }

            Player = player;
            _path = new ReadOnlyCollection<HexCoordinate>(coordinates);
        }

        private const int MinimumMoveLength = 2;
    }
}

