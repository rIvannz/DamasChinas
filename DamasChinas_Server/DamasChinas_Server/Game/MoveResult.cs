using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamasChinas_Server.Game
{

    public class MoveResult
    {
        private MoveResult(bool succeeded, string errorMessage, PlayerColor? winner)
        {
            Succeeded = succeeded;
            ErrorMessage = errorMessage;
            Winner = winner;
        }

        public bool Succeeded { get; }

        public string ErrorMessage { get; }

        public PlayerColor? Winner { get; }

        public static MoveResult Success(PlayerColor? winner = null)
        {
            return new MoveResult(true, null, winner);
        }

        public static MoveResult Error(string message)
        {
            return new MoveResult(false, message, null);
        }
    }
}
