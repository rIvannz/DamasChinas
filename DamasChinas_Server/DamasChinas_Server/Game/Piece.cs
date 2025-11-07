using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamasChinas_Server.Game
{
    /// <summary>
    /// Represents a piece controlled by a player.
    /// </summary>
    public class Piece
    {
        public PlayerColor Color { get; }

        public Piece(PlayerColor color)
        {
            if (!Enum.IsDefined(typeof(PlayerColor), color))
            {
                throw new ArgumentOutOfRangeException(nameof(color));
            }

            Color = color;
        }
    }
}

