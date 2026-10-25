using System;
using System.Collections.Generic;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Comparison
{
    public sealed record AlgorithmBenchmarkCase(String Kind, Boolean Black, Int32 Depth, Int32 GamePly = 0)
    {
        public Int32 Plies => this.Depth * 2;

        public Colours HistoryStartingColour => this.GamePly > 0 || !this.Black ? Colours.White : Colours.Black;

        public PositionData[] CreatePriorPositions() => this.GamePly > 0 ? Rc5GamePositions.CreatePriorPositions(this.GamePly) : [];

        public override String ToString() => $"{this.Kind}{(this.Black ? "Black" : "White")}D{this.Depth}";

        public Position CreatePosition()
        {
            if (this.GamePly > 0)
            {
                return Rc5GamePositions.CreatePosition(this.GamePly);
            }
            if (this.Kind == "PawnEnding")
            {
                return Position.ReadFEN($"7k/6pp/8/8/8/8/PP6/K7 {(this.Black ? "b" : "w")} - - 0 1");
            }
            return new MinMaxComparisonCase(this.ToString(), this.Plies, this.Kind == "Start", this.Black, this.Kind).CreatePosition();
        }

        public static IEnumerable<AlgorithmBenchmarkCase> All
        {
            get
            {
                // Keep both sides only for the starting position. The synthetic
                // endgames are colour-symmetric, so mirroring them and changing
                // OnMove duplicates benchmark work without adding a new workload.
                foreach (Boolean black in new[] { false, true })
                {
                    for (Int32 depth = 1; depth <= 4; depth++)
                    {
                        yield return new AlgorithmBenchmarkCase("Start", black, depth);
                    }
                }
                for (Int32 depth = 1; depth <= 5; depth++)
                {
                    yield return new AlgorithmBenchmarkCase("MateInTwo", false, depth);
                    yield return new AlgorithmBenchmarkCase("PawnEnding", false, depth);
                    yield return new AlgorithmBenchmarkCase("RookEnding", false, depth);
                }
                for (Int32 depth = 1; depth <= 5; depth++)
                {
                    yield return new AlgorithmBenchmarkCase("Rc5RandomPly023", true, depth, Rc5GamePositions.FirstRandomPly);
                    yield return new AlgorithmBenchmarkCase("Rc5RandomPly055", true, depth, Rc5GamePositions.SecondRandomPly);
                    yield return new AlgorithmBenchmarkCase("Rc5RepetitionStart", false, depth, Rc5GamePositions.RepetitionStartPly);
                }
            }
        }
    }
}
