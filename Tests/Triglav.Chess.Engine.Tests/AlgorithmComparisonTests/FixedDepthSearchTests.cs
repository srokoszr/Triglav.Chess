using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Triglav.Chess.Engine.Comparison;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Model;
using Triglav.Chess.Interop;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Tests.AlgorithmComparisonTests
{
    [TestClass]
    public class FixedDepthSearchTests
    {
        [DllImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_MinMaxGetTopMoveFixedDepth", CallingConvention = CallingConvention.Cdecl)]
        private static extern MinMaxResult WithoutDepthTracking(in MinMaxArguments arguments, IntPtr deepestPly);

        [TestMethod]
        [DataRow(false, 1)]
        [DataRow(true, 1)]
        [DataRow(false, 2)]
        [DataRow(true, 2)]
        public unsafe void FixedDepthPolicyDoesNotDependOnOutputPointer(Boolean black, Int32 plies)
        {
            Position position = new MinMaxComparisonCase("OptionalDepthOutput", plies, false, black, "RookEnding").CreatePosition();
            EvaluationModifiersData modifiers = new EvaluationModifiers().EvaluationModifiersData;
            using PositionStackWrapper trackedHistory = PositionStackWrapper.New(position.OnMove);
            using PositionStackWrapper untrackedHistory = PositionStackWrapper.New(position.OnMove);
            MinMaxResult tracked = NativeInterface.MinMaxGetTopMoveFixedDepth(position.State, modifiers, plies, position.OnMove, trackedHistory.NativeStack, out Int32 deepestPly);
            MinMaxArguments arguments = new(position.State, modifiers, plies, (Int32)position.OnMove, untrackedHistory.NativeStack);
            MinMaxResult untracked = WithoutDepthTracking(arguments, IntPtr.Zero);
            Assert.AreEqual(plies, deepestPly);
            Assert.AreEqual(tracked.Move, untracked.Move, "Removing diagnostics must not change the search policy.");
            Assert.AreEqual(tracked.Evaluation, untracked.Evaluation, "Removing diagnostics must not change the evaluation.");
        }

        [TestMethod]
        [DataRow(false, 1)]
        [DataRow(true, 1)]
        [DataRow(false, 2)]
        [DataRow(true, 2)]
        [DataRow(false, 4)]
        [DataRow(true, 4)]
        public unsafe void CapturesAndChecksDoNotExtendComparisonHorizon(Boolean black, Int32 plies)
        {
            Position position = new MinMaxComparisonCase("FixedDepth", plies, false, black, "RookEnding").CreatePosition();
            MoveSimulation[] moves = position.GetMoves();
            Assert.IsTrue(moves.Any(static move => (move.MoveFlags & MoveFlags.IsCapture) != 0), "Fixture must offer a capture.");
            Assert.IsTrue(moves.Any(static move => (move.MoveFlags & MoveFlags.IsCheck) != 0), "Fixture must offer a check.");
            VerifyHorizon(position, plies);
        }

        [TestMethod]
        [DataRow(false, 1)]
        [DataRow(true, 1)]
        [DataRow(false, 2)]
        [DataRow(true, 2)]
        [DataRow(false, 4)]
        [DataRow(true, 4)]
        public unsafe void QuietPositionDoesNotReceiveExtraDepthAllowance(Boolean black, Int32 plies)
        {
            Position position = new AlgorithmBenchmarkCase("PawnEnding", black, 1).CreatePosition();
            Assert.IsTrue(position.GetMoves().All(static move => move.MoveFlags == MoveFlags.None));
            VerifyHorizon(position, plies);
        }

        [TestMethod]
        public void ComparisonMatrixDoesNotExceedFiveFullMoves()
        {
            AlgorithmBenchmarkCase[] scenarios = AlgorithmBenchmarkCase.All.ToArray();
            Assert.AreEqual(38, scenarios.Length);
            Assert.IsTrue(scenarios.All(static scenario => scenario.Depth >= 1 && scenario.Depth <= 5 && scenario.Plies <= NativeInterface.MaximumComparisonPlies));
            Assert.IsTrue(MinMaxComparisonCase.All.All(static scenario => scenario.Depth >= 1 && scenario.Depth <= NativeInterface.MaximumComparisonPlies));
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(11)]
        [DataRow(Int32.MaxValue)]
        public unsafe void InvalidComparisonDepthIsRejectedBeforeNativeSearch(Int32 plies)
        {
            Position position = Position.GetStartingPosition();
            EvaluationModifiersData modifiers = new EvaluationModifiers().EvaluationModifiersData;
            using PositionStackWrapper history = PositionStackWrapper.New();
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => NativeInterface.MinMaxGetTopMoveFixedDepth(position.State, modifiers, plies, position.OnMove, history.NativeStack, out _));
            Assert.AreEqual(0, history.Count);
        }

        private static unsafe void VerifyHorizon(Position position, Int32 plies)
        {
            EvaluationModifiersData modifiers = new EvaluationModifiers().EvaluationModifiersData;
            using PositionStackWrapper history = PositionStackWrapper.New(position.OnMove);
            MinMaxResult result = NativeInterface.MinMaxGetTopMoveFixedDepth(position.State, modifiers, plies, position.OnMove, history.NativeStack, out Int32 deepestPly);
            Assert.AreEqual(plies, deepestPly, "MinMax must reach exactly the requested horizon in this nonterminal fixture.");
            Assert.IsTrue(position.GetMoves().Any(move => move.MoveStruct.Move == result.Move.Move));
        }
    }
}
