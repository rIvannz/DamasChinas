using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DamasChinas_Server.Common;

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
                throw new ArgumentNullException(nameof(players), MessageCode.UserValidationError.ToString());
            }

            var playerList = players.ToList();
            if (playerList.Count < MinimumPlayers || playerList.Count > MaximumPlayers)
            {
                throw new ArgumentException(MessageCode.InvalidPlayerCount.ToString(), nameof(players));
            }

            _players = playerList.ToDictionary(player => player.Color);
            if (_players.Count != playerList.Count)
            {
                throw new ArgumentException(MessageCode.PlayerColorDuplicate.ToString(), nameof(players));
            }

            Board = new ChineseCheckersBoard();
            _turnOrder = _players.Keys.OrderBy(color => color).ToList();
            CurrentTurn = _turnOrder[FirstTurnIndex];
            _targetZones = CreateTargetZones();

            InitializePieces();
        }

        public IReadOnlyCollection<Player> Players
        {
            get
            {
                return _players.Values.ToList().AsReadOnly();
            }
        }

        public MoveResult TryApplyMove(PlayerMove move)
        {
            if (move == null)
            {
                throw new ArgumentNullException(nameof(move), MessageCode.UserValidationError.ToString());
            }

            if (Winner.HasValue)
            {
                return MoveResult.Error(MessageCode.MatchFinished.ToString());
            }

            if (!_players.ContainsKey(move.Player))
            {
                return MoveResult.Error(MessageCode.PlayerNotInMatch.ToString());
            }

            if (move.Player != CurrentTurn)
            {
                return MoveResult.Error(MessageCode.NotPlayersTurn.ToString());
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

        public IReadOnlyDictionary<HexCoordinate, PlayerColor?> GetBoardState()
        {
            var snapshot = Board.Cells.ToDictionary(
                cell => cell.Coordinate,
                cell => cell.IsOccupied ? (PlayerColor?)cell.Piece.Color : null);

            return new ReadOnlyDictionary<HexCoordinate, PlayerColor?>(snapshot);
        }

        public PlayerColor GetTargetZone(PlayerColor player)
        {
            if (!_targetZones.TryGetValue(player, out PlayerColor targetZone))
            {
                throw new ArgumentException(MessageCode.PlayerTargetZoneMissing.ToString(), nameof(player));
            }

            return targetZone;
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

        private bool ValidateMove(PlayerMove move, out string errorMessage)
        {
            errorMessage = null;

            if (!TryValidateOrigin(move, out errorMessage, out _))
            {
                return false;
            }

            var visited = new HashSet<HexCoordinate> { move.Origin };
            bool performedJump = false;
            HexCoordinate current = move.Origin;

            for (int index = FirstDestinationIndex; index < move.Path.Count; index++)
            {
                HexCoordinate destination = move.Path[index];

                if (!TryValidateDestinationCell(destination, visited, out errorMessage, out _))
                {
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

                if (isJump && !TryValidateJump(middle, out errorMessage))
                {
                    return false;
                }

                if (isJump)
                {
                    performedJump = true;
                }

                current = destination;
            }

            return ValidateFinalJumpRule(move, performedJump, out errorMessage);
        }

        private bool TryValidateOrigin(
            PlayerMove move,
            out string errorMessage,
            out HexCell originCell)
        {
            errorMessage = null;
            originCell = null;

            if (!Board.TryGetCell(move.Origin, out originCell))
            {
                errorMessage = MessageCode.OriginCellInvalid.ToString();
                return false;
            }

            if (!originCell.IsOccupied || originCell.Piece.Color != move.Player)
            {
                errorMessage = MessageCode.OriginCellNotPlayersPiece.ToString();
                return false;
            }

            return true;
        }

        private bool TryValidateDestinationCell(
            HexCoordinate destination,
            HashSet<HexCoordinate> visited,
            out string errorMessage,
            out HexCell destinationCell)
        {
            errorMessage = null;
            destinationCell = null;

            if (!visited.Add(destination))
            {
                errorMessage = MessageCode.PathCellRepeated.ToString();
                return false;
            }

            if (!Board.TryGetCell(destination, out destinationCell))
            {
                errorMessage = MessageCode.DestinationOutsideBoard.ToString();
                return false;
            }

            if (destinationCell.IsOccupied)
            {
                errorMessage = MessageCode.DestinationCellOccupied.ToString();
                return false;
            }

            return true;
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
                errorMessage = MessageCode.InvalidStep.ToString();
                return false;
            }

            if (isAdjacent)
            {
                if (move.Path.Count > SingleStepLength)
                {
                    errorMessage = MessageCode.MultistepRequiresJump.ToString();
                    return false;
                }

                if (index != move.Path.Count - LastIndexOffset)
                {
                    errorMessage = MessageCode.AdjacentMoveSingleStep.ToString();
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
                errorMessage = MessageCode.NoPieceToJump.ToString();
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
                errorMessage = MessageCode.MultistepMustHaveJump.ToString();
                return false;
            }

            return true;
        }

        private void ExecuteMove(PlayerMove move)
        {
            Piece piece = Board.GetCell(move.Origin).RemovePiece();
            Board.GetCell(move.Destination).PlacePiece(piece);
        }

        private void AdvanceTurn()
        {
            int currentIndex = _turnOrder.IndexOf(CurrentTurn);
            CurrentTurn = _turnOrder[(currentIndex + NextTurnOffset) % _turnOrder.Count];
        }
    }
}
