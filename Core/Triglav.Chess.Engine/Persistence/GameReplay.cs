using System;
using System.Collections.Generic;
using System.Linq;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Model.Serializable;
using Triglav.Chess.Engine.Notation;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Persistence
{
    public enum GameReplayError
    {
        IllegalMove,
        MovesAfterEnd,
        CounterLimit,
        SnapshotMismatch,
    }

    public sealed class GameReplayException : FormatException
    {
        internal GameReplayException(GameReplayError error, Int32 ply) : base($"{error} at ply {ply}.")
        {
            this.Error = error;
            this.Ply = ply;
        }

        public GameReplayError Error { get; }
        public Int32 Ply { get; }
    }

    internal sealed record GameReplayResult(Position FinalPosition, List<MoveSerializable> Moves, Boolean ReconstructedCounters);

    internal static class GameReplay
    {
        internal static GameReplayResult Replay(GameDataSerializable game, Action<Position, MoveStruct, Int32> beforeMove = null)
        {
            PositionFile.Validate(game.StartingPosition);
            ArgumentNullException.ThrowIfNull(game.Moves);
            Position position = Position.GetPosition(game.StartingPosition);
            using PositionStackWrapper history = PositionStackWrapper.New(position.OnMove);
            PositionEvaluator evaluator = PositionEvaluator.GetDefault();
            Boolean legacyCounters = game.Metadata == null && game.StartingPosition.HalfmoveClock == 0 && game.StartingPosition.FullMoveCounter == 1
                && game.Moves.All(static move => move?.ResultingPosition is { HalfmoveClock: 0, FullMoveCounter: 1 });
            Boolean reconstructed = false;
            List<MoveSerializable> moves = new(game.Moves.Count);
            for (Int32 index = 0; index < game.Moves.Count; index++)
            {
                MoveSerializable saved = game.Moves[index];
                if (saved == null || saved.From < 0 || saved.From > 63 || saved.To < 0 || saved.To > 63
                    || !saved.Move.IsValid || !position.GetMoves().Any(candidate => candidate.MoveStruct.Move == saved.Move))
                {
                    throw new GameReplayException(GameReplayError.IllegalMove, index + 1);
                }
                if (PgnProfile.Terminal(position, history, evaluator).HasValue)
                {
                    throw new GameReplayException(GameReplayError.MovesAfterEnd, index + 1);
                }
                if (position.HalfmoveClock == Int32.MaxValue || (position.OnMove == Colours.Black && position.FullMoveCounter == Int32.MaxValue))
                {
                    throw new GameReplayException(GameReplayError.CounterLimit, index + 1);
                }
                beforeMove?.Invoke(position, saved.Move, index);
                history.Push(position.State);
                position.Move(saved.Move);
                PositionFile.Validate(saved.ResultingPosition);
                Position recorded = Position.GetPosition(saved.ResultingPosition);
                if (!PgnProfile.SamePosition(position, recorded, counters: !legacyCounters))
                {
                    throw new GameReplayException(GameReplayError.SnapshotMismatch, index + 1);
                }
                reconstructed |= legacyCounters && (position.HalfmoveClock != recorded.HalfmoveClock || position.FullMoveCounter != recorded.FullMoveCounter);
                moves.Add(new MoveSerializable(saved.Move, position.PositionDataSerializable));
            }
            return new GameReplayResult(position, moves, reconstructed);
        }

        internal static PositionStackWrapper CreateHistory(GameDataSerializable game)
        {
            PositionStackWrapper history = PositionStackWrapper.New((Colours)game.StartingPosition.OnMove);
            try
            {
                if (game.Moves.Count > 0)
                {
                    history.Push(Position.GetPosition(game.StartingPosition).State);
                    for (Int32 i = 0; i < game.Moves.Count - 1; i++)
                    {
                        history.Push(Position.GetPosition(game.Moves[i].ResultingPosition).State);
                    }
                }
                return history;
            }
            catch
            {
                history.Dispose();
                throw;
            }
        }
    }
}
