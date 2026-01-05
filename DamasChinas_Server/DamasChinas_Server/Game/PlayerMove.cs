using DamasChinas_Server.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DamasChinas_Server.Game
{

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
                throw new RepositoryValidationException(MessageCode.InvalidMove);
                
            }

            Player = player;
            _path = new ReadOnlyCollection<HexCoordinate>(coordinates);
        }
    }
}
