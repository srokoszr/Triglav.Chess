using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Triglav.Chess.Engine.Comparison;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Model;
using Triglav.Chess.Interop;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Benchmarks
{
    public readonly record struct SearchComparisonResult(Evaluation Evaluation, MoveWithPieceStruct Move);

    [Config(typeof(AlgorithmBenchmarkConfig))]
    [MemoryDiagnoser]
    public unsafe class SearchComparisonBenchmark
    {
        public IEnumerable<AlgorithmBenchmarkCase> Scenarios => AlgorithmBenchmarkCase.All;

        [ParamsSource(nameof(Scenarios))]
        public AlgorithmBenchmarkCase Scenario { get; set; }

        private PositionData position;
        private Colours onMove;
        private EvaluationModifiersData modifiers;
        private PositionStackWrapper stack;
        private PositionData[] priorPositions;

        [GlobalSetup]
        public void Setup()
        {
            this.Initialize();
            Position initial = this.Scenario.CreatePosition();
            try
            {
                this.Validate(initial, this.MinMax());
                this.Validate(initial, this.PositionTree());
                this.Validate(initial, this.PositionTreeWithCopying());
            }
            catch
            {
                this.stack.Dispose();
                throw;
            }
        }

        internal void Initialize()
        {
            AlgorithmBenchmarkPolicy.EnsureRelease();
            if (this.Scenario.Depth < 1 || this.Scenario.Depth > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Scenario));
            }
            Position initial = this.Scenario.CreatePosition();
            this.position = initial.State;
            this.onMove = initial.OnMove;
            this.modifiers = new EvaluationModifiers().EvaluationModifiersData;
            this.stack = PositionStackWrapper.New(this.Scenario.HistoryStartingColour);
            this.priorPositions = this.Scenario.CreatePriorPositions();
        }

        private void Validate(Position initial, SearchComparisonResult result)
        {
            if (!result.Move.Move.IsValid || !initial.GetMoves().Any(candidate => candidate.MoveStruct.Move == result.Move.Move))
            {
                throw new InvalidOperationException("A comparison backend returned an illegal move.");
            }
            if (this.stack.Count < this.priorPositions.Length || this.stack.Count > this.priorPositions.Length + 1)
            {
                throw new InvalidOperationException("A comparison backend left search nodes in the history stack.");
            }
            if (!this.stack.Array.Take(this.priorPositions.Length).Select(static prior => prior.State).SequenceEqual(this.priorPositions))
            {
                throw new InvalidOperationException("A comparison backend changed the supplied game history.");
            }
        }

        internal void ValidateResult(SearchComparisonResult result) => this.Validate(this.Scenario.CreatePosition(), result);

        private void ResetHistory()
        {
            while (this.stack.Count > 0)
            {
                _ = this.stack.Pop();
            }
            foreach (PositionData prior in this.priorPositions)
            {
                this.stack.Push(prior);
            }
        }

        [Benchmark(Baseline = true)]
        public SearchComparisonResult MinMax()
        {
            this.ResetHistory();
            MinMaxResult result = NativeInterface.MinMaxGetTopMoveFixedDepth(this.position, this.modifiers,
                this.Scenario.Plies, this.onMove, this.stack.NativeStack, out Int32 deepestPly);
            this.ValidateDepth(deepestPly);
            return new SearchComparisonResult(result.Evaluation, result.Move);
        }

        [Benchmark]
        public SearchComparisonResult PositionTree() => this.SearchTree(copying: false);

        private void ValidateDepth(Int32 deepestPly)
        {
            if (deepestPly > this.Scenario.Plies)
            {
                throw new InvalidOperationException("Search exceeded the fixed benchmark horizon.");
            }
        }

        [Benchmark]
        public SearchComparisonResult PositionTreeWithCopying() => this.SearchTree(copying: true);

        private SearchComparisonResult SearchTree(Boolean copying)
        {
            this.ResetHistory();
            Int32 depth = this.Scenario.Depth;
            PositionTree tree = copying
                ? PositionTreeNativeInterface.MinMaxBuildWithCopying(this.position, this.modifiers, depth, this.onMove, this.stack.NativeStack)
                : PositionTreeNativeInterface.MinMaxBuild(this.position, this.modifiers, depth, this.onMove, this.stack.NativeStack);
            try
            {
                if (tree.Root == null || tree.Root->FirstChild == null)
                {
                    throw new InvalidOperationException("Tree search returned no root move for a nonterminal scenario.");
                }
                MoveWithPieceStruct move = Position.ReconstructMove(tree.Root->Position, tree.Root->FirstChild->Position).Move;
                return new SearchComparisonResult(tree.Root->BestMoveEvaluation, move);
            }
            finally
            {
                PositionTreeNativeInterface.Free(tree);
            }
        }

        [GlobalCleanup]
        public void Cleanup() => this.stack.Dispose();

        // Keep move/evaluation evidence outside timed calls. Different search policies
        // can disagree, so do not require result equality to run the benchmarks.
        public static void PrintChecks()
        {
            SearchComparisonBenchmark benchmark = new();
            List<Object> results = new();
            foreach (AlgorithmBenchmarkCase scenario in benchmark.Scenarios)
            {
                benchmark.Scenario = scenario;
                TextWriter output = Console.Out;
                try
                {
                    Console.SetOut(TextWriter.Null);
                    benchmark.Setup();
                }
                finally
                {
                    Console.SetOut(output);
                }
                try
                {
                    results.Add(new
                    {
                        Scenario = scenario.ToString(),
                        SearchPolicy = "FixedDepthNoExtensions",
                        NominalFullMoves = scenario.Depth,
                        NominalPlies = scenario.Plies,
                        TreeDepthArgument = scenario.Depth,
                        SourceGamePly = scenario.GamePly,
                        HistoryPositions = scenario.CreatePriorPositions().Length,
                        Fen = scenario.CreatePosition().FEN_Notation,
                        PieceCount = scenario.CreatePosition().Count,
                        LegalMoveCount = scenario.CreatePosition().GetMoves().Count(),
                        MinMax = Describe(benchmark.MinMax()),
                        PositionTree = Describe(benchmark.PositionTree()),
                        PositionTreeWithCopying = Describe(benchmark.PositionTreeWithCopying()),
                    });
                }
                finally
                {
                    benchmark.Cleanup();
                }
            }
            Console.WriteLine(JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));

            static Object Describe(SearchComparisonResult result) => new
            {
                Move = result.Move.Move.ToString(),
                Piece = result.Move.Piece.ToString(),
                Result = result.Evaluation.EvaluationResult.ToString(),
                result.Evaluation.InNMoves,
                result.Evaluation.EvaluationValue,
            };
        }
    }
}
