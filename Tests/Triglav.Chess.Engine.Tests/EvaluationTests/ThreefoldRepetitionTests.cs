using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Model.Serializable;
using Triglav.Chess.Engine.Persistence;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Tests.EvaluationTests
{
    [TestClass]
    public class ThreefoldRepetitionTests
    {
        [TestMethod]
        public void BlackRc5GameEndsAtThirdOccurrenceWithCompleteHistory()
        {
            using Stream stream = typeof(ThreefoldRepetitionTests).Assembly.GetManifestResourceStream("BlackRc5Threefold.PGN");
            using StreamReader reader = new(stream);
            String text = Regex.Replace(reader.ReadToEnd(), @"(?m)^\[.*\]\s*", String.Empty);
            String[] notation = Regex.Replace(text, @"\d+\.", String.Empty).Split((Char[])null, StringSplitOptions.RemoveEmptyEntries);
            Position position = Position.GetStartingPosition();
            using PositionStackWrapper history = PositionStackWrapper.New();
            PositionEvaluator evaluator = PositionEvaluator.GetDefault();
            GameDataSerializable game = new()
            {
                StartingPosition = position.PositionDataSerializable
            };
            for (Int32 ply = 0; ply < notation.Length; ply++)
            {
                MoveSimulation[] candidates = [.. position.GetMoves().Where(candidate => position.Clone().MoveDescriptive(candidate.MoveStruct.Move).AlgebraicNotation == notation[ply])];
                Assert.HasCount(1, candidates, $"PGN ply {ply + 1}: {notation[ply]}");
                Assert.IsFalse(evaluator.EvaluateTerminalPosition(position, history, 0).IsTerminal, $"Game must continue before ply {ply + 1}.");
                history.Push(position.State);
                position.Move(candidates[0].MoveStruct);
                game.AddMove(candidates[0].MoveStruct.Move, position.PositionDataSerializable);
                if (ply == 119)
                {
                    Algorithm algorithm = new(AlgorithmDepthLevel.Tourist, false);
                    (EvaluationWrapper evaluation, MoveWithPieceStruct nextMove) = algorithm.GetMove(position, history);
                    Assert.IsTrue(nextMove.Move.IsValid, "The second occurrence still permits another move.");
                    Assert.IsTrue(position.GetMoves().Any(candidate => candidate.MoveStruct.Move == nextMove.Move));
                    Assert.AreEqual(120, history.Count, "Searching must preserve the populated game history.");
                }
            }
            Assert.AreEqual(124, history.Count);
            Assert.AreEqual(Colours.White, position.OnMove);
            Assert.AreEqual((MoveStruct)"c4-c5", game.Moves[^1].Move);
            Assert.AreEqual(position.State, history.Array[116].State, "First occurrence after 58...Rc5.");
            Assert.AreEqual(position.State, history.Array[120].State, "Second occurrence after 60...Rc5.");
            EvaluationWrapper terminal = evaluator.EvaluateTerminalPosition(position, history, (Byte)history.Count);
            Assert.IsTrue(terminal.IsDraw, "62...Rc5 must end the game before another engine move is requested.");
            Assert.HasCount(124, game.Moves);
            Int32 originalCount = history.Count;
            foreach (Boolean humanMode in new[] { false, true })
            {
                Algorithm algorithm = new(AlgorithmDepthLevel.Tourist, humanMode);
                (EvaluationWrapper evaluation, MoveWithPieceStruct move) = algorithm.GetMove(position, history);
                Assert.IsTrue(evaluation.IsDraw);
                Assert.IsFalse(move.Move.IsValid, "A terminal root has no next move; callers must finish the game.");
                Assert.AreEqual(originalCount, history.Count, "Terminal detection must not change history.");
            }
            game.Result = GameResult.DrawThreefoldRepetition;
            String directory = Path.Combine(Path.GetTempPath(), "TriglavRepetition-" + Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(directory);
            try
            {
                String path = Path.Combine(directory, "game.TgGame");
                GameFile.Save(path, game);
                PgnRecoveryFile recovery = new(directory);
                recovery.Save(game);
                GameDataSerializable loaded = GameFile.Load(path);
                Assert.AreEqual(GameResult.DrawThreefoldRepetition, loaded.Result);
                Assert.HasCount(124, loaded.Moves);
                Assert.IsTrue(loaded.Moves.All(saved => saved.Move.IsValid));
                String pgn = File.ReadAllText(recovery.FilePath);
                StringAssert.Contains(pgn, "62. Kg3 Rc5");
                StringAssert.Contains(pgn, "1/2-1/2");
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        }
        [TestMethod]
        [DataRow(Colours.White)]
        [DataRow(Colours.Black)]
        public void InitialPositionCountsOnceForEitherStartingColour(Colours startingColour)
        {
            Position position = Position.GetStartingPosition();
            position.OnMove = startingColour;
            using PositionStackWrapper history = PositionStackWrapper.New();
            String[] cycle = startingColour == Colours.White
                ? ["g1-f3", "g8-f6", "f3-g1", "f6-g8"]
                : ["g8-f6", "g1-f3", "f6-g8", "f3-g1"];
            Assert.IsFalse(history.IsThreefoldRepetition(position));
            for (Int32 repeat = 0; repeat < 2; repeat++)
            {
                foreach (String move in cycle)
                {
                    history.Push(position.State);
                    position.Move(move);
                }
                Assert.AreEqual(repeat == 1, history.IsThreefoldRepetition(position));
            }
            Assert.AreEqual(8, history.Count);
        }

        [TestMethod]
        public void SameBoardWithDifferentFirstMoveRightsIsNotSamePosition()
        {
            Position position = Position.GetEmptyWithKingsNotMoved();
            position.Add(Colours.White, Pieces.Rook, "a1");
            position.Add(Colours.Black, Pieces.Rook, "a8");
            using PositionStackWrapper history = PositionStackWrapper.New();
            for (Int32 repeat = 0; repeat < 3; repeat++)
            {
                foreach (String move in new[] { "e1-e2", "e8-e7", "e2-e1", "e7-e8" })
                {
                    history.Push(position.State);
                    position.Move(move);
                }
                Assert.AreEqual(repeat == 2, history.IsThreefoldRepetition(position));
            }
        }

        [TestMethod]
        public void OppositeSideToMoveOccurrencesDoNotCount()
        {
            Position position = Position.GetStartingPosition();
            Position other = position.Clone();
            other.Move("e2-e4");
            using PositionStackWrapper history = PositionStackWrapper.New();
            history.Push(other.State);
            history.Push(position.State);
            history.Push(other.State);
            history.Push(position.State);
            Assert.IsFalse(history.IsThreefoldRepetition(position));
        }
    }
}
