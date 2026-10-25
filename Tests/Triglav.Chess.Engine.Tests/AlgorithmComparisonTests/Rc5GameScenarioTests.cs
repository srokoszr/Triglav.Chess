using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Triglav.Chess.Engine.Comparison;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Tests.AlgorithmComparisonTests
{
    [TestClass]
    [DoNotParallelize]
    public class Rc5GameScenarioTests
    {
        [TestMethod]
        [DataRow(Rc5GamePositions.FirstRandomPly, "b2-b4", Colours.Black)]
        [DataRow(Rc5GamePositions.SecondRandomPly, "g3-g4", Colours.Black)]
        [DataRow(Rc5GamePositions.RepetitionStartPly, "c3-c5", Colours.White)]
        public void GameScenarioReplaysTheOriginalPositionAndPriorHistory(Int32 ply, String move, Colours side)
        {
            Position before = Rc5GamePositions.CreatePosition(ply - 1);
            Assert.IsTrue(before.GetMoves().Any(candidate => candidate.MoveStruct.Move == (MoveStruct)move));
            before.Move(move);
            Position position = Rc5GamePositions.CreatePosition(ply);
            Assert.AreEqual(before.FEN_Notation, position.FEN_Notation);
            Assert.AreEqual(side, position.OnMove);
            PositionData[] prior = Rc5GamePositions.CreatePriorPositions(ply);
            Assert.HasCount(ply, prior);
            Assert.AreEqual(Position.GetStartingPosition().State, prior[0]);
            Assert.AreEqual(Rc5GamePositions.CreatePosition(ply - 1).State, prior[^1]);
            AlgorithmBenchmarkCase[] scenarios = [.. AlgorithmBenchmarkCase.All.Where(scenario => scenario.GamePly == ply)];
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4, 5 }, scenarios.Select(static scenario => scenario.Depth).ToArray());
            foreach (AlgorithmBenchmarkCase scenario in scenarios)
            {
                Assert.AreEqual(position.FEN_Notation, scenario.CreatePosition().FEN_Notation);
                Assert.AreEqual(Colours.White, scenario.HistoryStartingColour);
                CollectionAssert.AreEqual(prior, scenario.CreatePriorPositions());
            }
        }

        [TestMethod]
        public void RepetitionScenarioIsTheFirstOccurrenceNotTheFinishedDraw()
        {
            Position first = Rc5GamePositions.CreatePosition(Rc5GamePositions.RepetitionStartPly);
            using PositionStackWrapper history = PositionStackWrapper.New();
            foreach (PositionData prior in Rc5GamePositions.CreatePriorPositions(Rc5GamePositions.RepetitionStartPly))
            {
                history.Push(prior);
            }
            Assert.AreEqual(116, history.Count);
            Assert.IsFalse(history.Array.Any(prior => prior.OnMove == first.OnMove && prior.State.Equals(first.State)),
                "58...Rc5 must be the first occurrence, with no identical earlier position for White to move.");
            Assert.IsFalse(history.IsThreefoldRepetition(first));
            Assert.IsFalse(PositionEvaluator.GetDefault().EvaluateTerminalPosition(first, history, 0).IsTerminal);
            Assert.AreEqual(first.State, Rc5GamePositions.CreatePosition(120).State);
            Assert.AreEqual(first.State, Rc5GamePositions.CreatePosition(124).State);
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4, 5 }, AlgorithmBenchmarkCase.All
                .Where(static scenario => scenario.GamePly == Rc5GamePositions.RepetitionStartPly)
                .Select(static scenario => scenario.Depth).ToArray());
        }
    }
}
