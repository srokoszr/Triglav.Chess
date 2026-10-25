using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Triglav.Chess.Engine.Comparison;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Model;
using Triglav.Chess.Interop;
using Triglav.Chess.Model.Structs;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Tests.AlgorithmComparisonTests
{
    [TestClass]
    [DoNotParallelize]
    public class BenchmarkScenarioComparisonTests
    {
        private enum Backend
        {
            MinMax,
            PositionTree,
            PositionTreeWithCopying,
        }

        private readonly record struct SearchResult(Backend Algorithm, Evaluation Evaluation, MoveWithPieceStruct Move);

        public static IEnumerable<Object[]> Cases
        {
            get
            {
                String configuredDepth = Environment.GetEnvironmentVariable("TRIGLAV_COMPARISON_MAX_FULL_MOVES");
                Int32 maximumDepth = 5;
                if (configuredDepth != null && (!Int32.TryParse(configuredDepth, out maximumDepth) || maximumDepth < 1 || maximumDepth > 5))
                {
                    throw new ArgumentOutOfRangeException(nameof(configuredDepth), "Comparison depth must be between 1 and 5 full moves.");
                }
                return AlgorithmBenchmarkCase.All
                    .Where(scenario => scenario.Depth <= maximumDepth)
                    .OrderBy(static scenario => scenario.Depth)
                    .ThenBy(static scenario => scenario.Kind == "Start")
                    .Select(static scenario => new Object[] { scenario });
            }
        }

        [Ignore]//it's not valid for all alogirth to always return same result
        [TestMethod]
        [DynamicData(nameof(Cases))]
        public void AllAlgorithmsReturnTheSameResult(AlgorithmBenchmarkCase scenario)
        {
            Assert.IsTrue(scenario.Depth >= 1 && scenario.Depth <= 5 && scenario.Plies <= NativeInterface.MaximumComparisonPlies,
                "Correctness comparisons must not exceed 5 full moves / 10 plies.");
            Position position = scenario.CreatePosition();
            PositionData initialState = position.State;
            EvaluationModifiersData modifiers = new EvaluationModifiers().EvaluationModifiersData;
            List<SearchResult> results = new();
            List<String> differences = new();
            foreach (Backend backend in Enum.GetValues<Backend>())
            {
                SearchResult result = Search(position, modifiers, scenario, backend);
                results.Add(result);
                Assert.AreEqual(initialState, position.State, $"{scenario}: {backend} modified the input position.");
                Assert.IsTrue(result.Move.Move.IsValid && position.GetMoves().Any(candidate => candidate.MoveStruct.Move == result.Move.Move),
                    $"{scenario}: {backend} returned an illegal move: {result.Move}.");
            }
            Console.WriteLine("ALGORITHM_RESULT_JSON " + JsonSerializer.Serialize(new
            {
                Scenario = scenario.ToString(),
                FullMoveDepth = scenario.Depth,
                scenario.Plies,
                Fen = position.FEN_Notation,
                HistoryPositions = scenario.CreatePriorPositions().Length,
                Reference = nameof(Backend.PositionTreeWithCopying),
                SearchPolicy = "FixedDepthNoExtensions",
                Results = results.Select(static result => new
                {
                    Algorithm = result.Algorithm.ToString(),
                    Move = result.Move.Move.ToString(),
                    Piece = result.Move.Piece.ToString(),
                    Result = result.Evaluation.EvaluationResult.ToString(),
                    ResultCode = result.Evaluation.Result,
                    result.Evaluation.InNMoves,
                    result.Evaluation.EvaluationValue,
                }).ToArray(),
            }));
            // Compare every pair and collect every differing field, so a failure
            // preserves the complete evidence instead of stopping at the first mismatch.
            for (Int32 left = 0; left < results.Count; left++)
            {
                for (Int32 right = left + 1; right < results.Count; right++)
                {
                    Compare(results[left], results[right], differences);
                }
            }
            if (differences.Count > 0)
            {
                String snapshots = String.Join(Environment.NewLine, results.Select(static result =>
                    $"{result.Algorithm}: move={result.Move.Move}, piece={result.Move.Piece}, result={result.Evaluation.EvaluationResult}, InNMoves={result.Evaluation.InNMoves}, EvaluationValue={result.Evaluation.EvaluationValue}"));
                Assert.Fail($"{scenario}: nominal depth {scenario.Depth} full moves / {scenario.Plies} plies."
                    + Environment.NewLine + snapshots + Environment.NewLine + String.Join(Environment.NewLine, differences));
            }
        }

        private static void Compare(SearchResult left, SearchResult right, List<String> differences)
        {
            String pair = $"{left.Algorithm} vs {right.Algorithm}";
            Add("Result", left.Evaluation.Result, right.Evaluation.Result);
            // Nonterminal InNMoves is internal search metadata: MinMax leaves it
            // at zero, while PositionTree stores the evaluated node depth there.
            // It represents result distance only when at least one side is terminal.
            if (left.Evaluation.EvaluationValue == Evaluation.EvaluationNaN
                || right.Evaluation.EvaluationValue == Evaluation.EvaluationNaN)
            {
                Add("InNMoves", left.Evaluation.InNMoves, right.Evaluation.InNMoves);
            }
            Add("EvaluationValue", left.Evaluation.EvaluationValue, right.Evaluation.EvaluationValue);
            Add("Move.From", left.Move.Move.From, right.Move.Move.From);
            Add("Move.To", left.Move.Move.To, right.Move.Move.To);
            Add("Move.Piece", left.Move.Piece, right.Move.Piece);

            void Add<T>(String field, T expected, T actual)
            {
                if (!EqualityComparer<T>.Default.Equals(expected, actual))
                {
                    differences.Add($"{pair}: {field}: {expected} != {actual}.");
                }
            }
        }

        private static unsafe SearchResult Search(Position position, EvaluationModifiersData modifiers, AlgorithmBenchmarkCase scenario, Backend backend)
        {
            using PositionStackWrapper history = PositionStackWrapper.New(scenario.HistoryStartingColour);
            PositionData[] priorPositions = scenario.CreatePriorPositions();
            foreach (PositionData prior in priorPositions)
            {
                history.Push(prior);
            }
            SearchResult result;
            if (backend == Backend.MinMax)
            {
                MinMaxResult native = NativeInterface.MinMaxGetTopMoveFixedDepth(position.State, modifiers, scenario.Plies, position.OnMove, history.NativeStack, out Int32 deepestPly);
                Assert.IsTrue(deepestPly <= scenario.Plies,
                    $"{scenario}: {backend} searched {deepestPly} plies beyond the requested {scenario.Plies}-ply horizon.");
                result = new SearchResult(backend, native.Evaluation, native.Move);
            }
            else
            {
                PositionTree tree = backend == Backend.PositionTree
                    ? PositionTreeNativeInterface.MinMaxBuild(position.State, modifiers, scenario.Depth, position.OnMove, history.NativeStack)
                    : PositionTreeNativeInterface.MinMaxBuildWithCopying(position.State, modifiers, scenario.Depth, position.OnMove, history.NativeStack);
                try
                {
                    Assert.IsTrue(tree.Root != null && tree.Root->FirstChild != null, $"{scenario}: {backend} returned no root move.");
                    MoveWithPieceStruct move = Position.ReconstructMove(tree.Root->Position, tree.Root->FirstChild->Position).Move;
                    result = new SearchResult(backend, tree.Root->BestMoveEvaluation, move);
                }
                finally
                {
                    PositionTreeNativeInterface.Free(tree);
                }
            }
            Assert.IsTrue(history.Count >= priorPositions.Length && history.Count <= priorPositions.Length + 1, $"{scenario}: {backend} left {history.Count} search nodes in history.");
            CollectionAssert.AreEqual(priorPositions, history.Array.Take(priorPositions.Length).Select(static prior => prior.State).ToArray(),
                $"{scenario}: {backend} changed the supplied game history.");
            return result;
        }
    }
}
