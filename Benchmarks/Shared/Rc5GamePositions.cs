using System;
using System.IO;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Model.Serializable;
using Triglav.Chess.Engine.Notation;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Comparison
{
    public static class Rc5GamePositions
    {
        // Frozen sample from plies 20..100 (inclusive), Python Random(20261017).
        public const Int32 FirstRandomPly = 23;
        public const Int32 SecondRandomPly = 55;
        public const Int32 RepetitionStartPly = 116;

        private static readonly Lazy<PgnImportResult> Imported = new(Import);

        private static PgnImportResult Import()
        {
            using Stream stream = typeof(Rc5GamePositions).Assembly.GetManifestResourceStream("BlackRc5Threefold.PGN")
                ?? throw new InvalidOperationException("The Rc5 game fixture is missing.");
            return PgnReader.Read(stream, new PgnImportOptions { AllowLegacyTriglav = true });
        }

        public static Position CreatePosition(Int32 ply)
        {
            GameDataSerializable game = Imported.Value.Game;
            ArgumentOutOfRangeException.ThrowIfNegative(ply);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(ply, game.Moves.Count);
            return Position.GetPosition(ply == 0 ? game.StartingPosition : game.Moves[ply - 1].ResultingPosition);
        }

        public static PositionData[] CreatePriorPositions(Int32 ply)
        {
            _ = CreatePosition(ply);
            PositionData[] prior = new PositionData[ply];
            for (Int32 index = 0; index < prior.Length; index++)
            {
                prior[index] = CreatePosition(index).State;
            }
            return prior;
        }
    }
}
