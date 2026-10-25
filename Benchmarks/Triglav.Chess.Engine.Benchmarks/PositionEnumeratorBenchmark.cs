using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Interop;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Benchmarks
{
    [MemoryDiagnoser(true)]
    //[SimpleJob(RunStrategy.Throughput, launchCount: 8, warmupCount: 1, iterationCount: 64, runtimeMoniker: RuntimeMoniker.Net90)]
    [SimpleJob(RunStrategy.Throughput, launchCount: 8, warmupCount: 1, iterationCount: 8, runtimeMoniker: RuntimeMoniker.Net10_0)]
    public class PositionEnumeratorBenchmark
    {
        private Position Position;
        private PositionData PositionData;

        public PositionEvaluator PositionEvaluator { get; set; }

        private MoveWithPieceStruct E2E4;
        private MoveWithPieceStruct E7E5;

        public EvaluationModifiersData EveluationModifiers { get; set; }

        [GlobalSetup]
        public unsafe void Setup()
        {
            this.Position = Position.GetStartingPosition();
            this.PositionData = this.Position.State;

            this.PositionEvaluator = PositionEvaluator.GetDefault();
            this.E2E4 = new MoveWithPieceStruct(MoveStruct.Parse("e2-e4"), (byte)Pieces.Pawn);
            this.E7E5 = new MoveWithPieceStruct(MoveStruct.Parse("e7-e5"), (byte)Pieces.Pawn);

            this.EveluationModifiers = this.PositionEvaluator.Modifiers.EvaluationModifiersData;
        }

        [Benchmark]
        public unsafe void EvaluateNonTerminalPosition()
        {
            var position = this.Position;

            var evaluation = this.PositionEvaluator.EvaluateNonTerminalPosition(position, 0);
        }

        [Benchmark]
        public unsafe void GetMovesNoCheckValidation()
        {
            var result = stackalloc Triglav.Chess.Model.Structs.Interoperable.MoveWithPieceStruct[MovesEnumerators.MaxMovesInPosition];
            var position = this.Position;

            var resultCount = position.OnMove == ColourConstants.White ? position.GetMovesNoCheckValidationWhite(result) : position.GetMovesNoCheckValidationBlack(result);
        }

        [Benchmark]
        public void GetAttackedFields()
        {
            var position = this.Position;
            var moves = position.GetAttackedFields(position.OnMove);
        }

        [Benchmark]
        public void GetAttackedFields_Precomputed()
        {
            var position = this.Position;
            var moves = position.GetAttackedFields_BitwiseLoop(position.OnMove);
        }

        [Benchmark]
        public unsafe void GetMoves()
        {
            var position = this.Position;

            MoveSimulation* moves = stackalloc MoveSimulation[MovesEnumerators.MaxMovesInPosition];

            var resultCount = position.GetMovesWhite(moves);

            for (int i = 0; i < resultCount; i++)
            {
                //var key = (*moves).Position.GetPositionKey();
            }
        }

        [Benchmark]
        public unsafe void GetMovesNoFlags()
        {
            var position = this.Position;

            MoveSimulation* moves = stackalloc MoveSimulation[MovesEnumerators.MaxMovesInPosition];

            var resultCount = PositionNativeInterface.GetMovesNoFlags(position.State, moves, position.OnMove);

            for (int i = 0; i < resultCount; i++)
            {
                //var key = (*moves).Position.GetPositionKey();
            }
        }
    }
}
