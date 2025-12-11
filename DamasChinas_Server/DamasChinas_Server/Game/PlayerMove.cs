using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DamasChinas_Server.Game
{
    /// <summary>
    /// Represents a player's move request.
    /// </summary>
    public class PlayerMove
    {
        private const int MinimumMoveLength = 2;

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
         
                return _path[0];
            }
        }

        public HexCoordinate Destination
        {
            get
            {
                return _path[_path.Count - 1];
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
                throw new ArgumentException(
                    "A move must contain at least an origin and a destination.",
                    nameof(path));
            }

            Player = player;
            _path = new ReadOnlyCollection<HexCoordinate>(coordinates);
        }
    }
}
