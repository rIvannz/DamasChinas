using System;
using DamasChinas_Server.Common;

namespace DamasChinas_Server.Game
{
    public class Player
    {
        public string Id { get; }
        public string Name { get; }
        public PlayerColor Color { get; }

        public Player(string id, string name, PlayerColor color)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(MessageCode.PlayerIdRequired.ToString(), nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(MessageCode.PlayerNameRequired.ToString(), nameof(name));
            }

            Id = id;
            Name = name;
            Color = color;
        }
    }
}
