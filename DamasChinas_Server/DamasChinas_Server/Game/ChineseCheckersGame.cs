using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DamasChinas_Server.Game
{
    public class ChineseCheckersGame
    {
        private const int MinimumPlayers = 2;
        private const int MaximumPlayers = 6;
        private const int SingleStepLength = 2;
        private const int FirstTurnIndex = 0;
        private const int FirstDestinationIndex = 1;
        private const int LastIndexOffset = 1;
        private const int NextTurnOffset = 1;

        private readonly Dictionary<PlayerColor, Player> _players;
        private readonly List<PlayerColor> _turnOrder;
        private readonly Dictionary<PlayerColor, PlayerColor> _targetZones;

        public ChineseCheckersBoard Board { get; }
        public PlayerColor CurrentTurn { get; private set; }
        public PlayerColor? Winner { get; private set; }

        public ChineseCheckersGame(IEnumerable<Player> players)
        {
            if (players == null)
            {
                throw new ArgumentNullException(nameof(players));
            }

            var playerList = players.ToList();
            if (playerList.Count < MinimumPlayers || playerList.Count > MaximumPlayers)
            {
                throw new ArgumentException(
                    $"The match requires between {MinimumPlayers} and {MaximumPlayers} players.",
                    nameof(players));
            }

            _players = playerList.ToDictionary(player => player.Color);
            if (_players.Count != playerList.Count)
            {
                throw new ArgumentException("Each player must have a unique color.", nameof(players));
            }

            Board = new ChineseCheckersBoard();
            _turnOrder = _players.Keys.OrderBy(color => color).ToList();
            CurrentTurn = _turnOrder[FirstTurnIndex];
            _targetZones = CreateTargetZones();

            InitializePieces();
        }


        private static Dictionary<PlayerColor, PlayerColor> CreateTargetZones()
        {
            return new Dictionary<PlayerColor, PlayerColor>
            {
                { PlayerColor.Red,    PlayerColor.Green },
                { PlayerColor.Green,  PlayerColor.Red },
                { PlayerColor.Blue,   PlayerColor.Yellow },
                { PlayerColor.Yellow, PlayerColor.Blue },
                { PlayerColor.Orange, PlayerColor.Purple },
                { PlayerColor.Purple, PlayerColor.Orange }
            };
        }

        private void InitializePieces()
        {
            foreach (PlayerColor color in _players.Keys)
            {
                foreach (HexCell cell in Board.GetZoneCells(color))
                {
                    cell.PlacePiece(new Piece(color));
                }
            }
        }



        public MoveResult TryApplyMove(PlayerMove move)
        {
            if (move == null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            if (Winner.HasValue)
            {
                return MoveResult.Error("The match has already finished.");
            }

            if (!_players.ContainsKey(move.Player))
            {
                return MoveResult.Error("The player is not part of this match.");
            }

            if (move.Player != CurrentTurn)
            {
                return MoveResult.Error("It is not the player's turn.");
            }

            if (!ValidateMove(move, out string errorMessage))
            {
                return MoveResult.Error(errorMessage);
            }

            ExecuteMove(move);

            if (HasWon(move.Player))
            {
                Winner = move.Player;
                return MoveResult.Success(Winner);
            }

            AdvanceTurn();
            return MoveResult.Success();
        }

        public bool HasWon(PlayerColor player)
        {
            if (!_targetZones.TryGetValue(player, out PlayerColor targetZone))
            {
                return false;
            }

            return Board.GetZoneCells(targetZone)
                .All(cell => cell.IsOccupied && cell.Piece.Color == player);
        }

        private bool ValidateMove(PlayerMove move, out string errorMessage)
        {
            errorMessage = null;

 
            if (!Board.TryGetCell(move.Origin, out HexCell origin))
            {
                errorMessage = "The origin cell does not exist on the board.";
                return false;
            }

            if (!origin.IsOccupied || origin.Piece.Color != move.Player)
            {
                errorMessage = "The origin cell does not contain one of the player's pieces.";
                return false;
            }

            var visited = new HashSet<HexCoordinate> { move.Origin };
            bool performedJump = false;
            HexCoordinate current = move.Origin;

            for (int index = FirstDestinationIndex; index < move.Path.Count; index++)
            {
                HexCoordinate destination = move.Path[index];

      
                if (!visited.Add(destination))
                {
                    errorMessage = "The path cannot visit the same cell twice.";
                    return false;
                }

                if (!Board.TryGetCell(destination, out HexCell destCell))
                {
                    errorMessage = "One of the destination cells is outside the board.";
                    return false;
                }

                if (destCell.IsOccupied)
                {
                    errorMessage = "The path ends on an occupied cell.";
                    return false;
                }

        
                if (!TryValidateStep(
                    move,
                    current,
                    destination,
                    index,
                    out errorMessage,
                    out bool isJump,
                    out HexCoordinate middle))
                {
                    return false;
                }

                if (isJump)
                {
                    if (!TryValidateJump(middle, out errorMessage))
                    {
                        return false;
                    }

                    performedJump = true;
                }

                current = destination;
            }

            return ValidateFinalJumpRule(move, performedJump, out errorMessage);
        }

        private static bool TryValidateStep(
            PlayerMove move,
            HexCoordinate current,
            HexCoordinate destination,
            int index,
            out string errorMessage,
            out bool isJump,
            out HexCoordinate middle)
        {
            errorMessage = null;
            isJump = ChineseCheckersBoard.IsJumpMove(current, destination, out middle);
            bool isAdjacent = ChineseCheckersBoard.IsAdjacentMove(current, destination);

            if (!isAdjacent && !isJump)
            {
                errorMessage = "The move is neither an adjacent step nor a valid jump.";
                return false;
            }

            if (isAdjacent)
            {
                if (move.Path.Count > SingleStepLength)
                {
                    errorMessage = "Multi-step moves must be performed exclusively through jumps.";
                    return false;
                }

                if (index != move.Path.Count - LastIndexOffset)
                {
                    errorMessage = "Adjacent moves can only consist of a single step.";
                    return false;
                }
            }

            return true;
        }

        private bool TryValidateJump(
            HexCoordinate middle,
            out string errorMessage)
        {
            errorMessage = null;

            if (!Board.TryGetCell(middle, out HexCell jumpCell) || !jumpCell.IsOccupied)
            {
                errorMessage = "There is no piece to jump over in the intermediate cell.";
                return false;
            }

            return true;
        }

        private static bool ValidateFinalJumpRule(
            PlayerMove move,
            bool performedJump,
            out string errorMessage)
        {
            errorMessage = null;

            if (move.Path.Count > SingleStepLength && !performedJump)
            {
                errorMessage = "Multi-step moves must contain at least one jump.";
                return false;
            }

            return true;
        }



        private void ExecuteMove(PlayerMove move)
        {
            HexCell originCell = Board.GetCell(move.Origin);
            Piece piece = originCell.RemovePiece();
            HexCell destinationCell = Board.GetCell(move.Destination);
            destinationCell.PlacePiece(piece);
        }

        private void AdvanceTurn()
        {
            int currentIndex = _turnOrder.IndexOf(CurrentTurn);
            CurrentTurn = _turnOrder[(currentIndex + NextTurnOffset) % _turnOrder.Count];
        }
    }
}
