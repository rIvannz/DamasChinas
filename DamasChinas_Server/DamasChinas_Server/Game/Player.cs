using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamasChinas_Server.Game
{
    /// <summary>
    /// Minimal model for a player connected to the server.
    /// </summary>
    public class Player
    {
        public string Id { get; }
        public string Name { get; }
        public PlayerColor Color { get; }

        public Player(string id, string name, PlayerColor color)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("The player identifier is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The player name is required.", nameof(name));
            }

            Id = id;
            Name = name;
            Color = color;
        }
    }
}
