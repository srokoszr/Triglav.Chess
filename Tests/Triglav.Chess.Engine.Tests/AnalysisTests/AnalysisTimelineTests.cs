using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Triglav.Chess.App.GUI.Model;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Model.Serializable;
using Triglav.Chess.Engine.Persistence;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.Engine.Tests.AnalysisTests
{
    [TestClass]
    public class AnalysisTimelineTests
    {
        private String directory;
        private String path;

        [TestInitialize]
        public void Initialize()
        {
            this.directory = Path.Combine(Path.GetTempPath(), "TriglavAnalysis-" + Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(this.directory);
            this.path = Path.Combine(this.directory, "example.TgGame");
            using Stream example = typeof(AnalysisTimelineTests).Assembly.GetManifestResourceStream("AnalysisExample.TgGame");
            using FileStream destination = File.Create(this.path);
            example.CopyTo(destination);
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(this.directory, recursive: true);

        [TestMethod]
        public void SuppliedExampleReplaysEverySavedPositionForwardAndBackwardWithoutChangingFile()
        {
            Byte[] original = File.ReadAllBytes(this.path);
            GameImportResult imported = GameFile.Import(this.path);
            AnalysisTimeline timeline = new(imported);
            Assert.AreEqual(61, timeline.MoveCount);
            Assert.AreEqual(imported.Game.Moves.Count, timeline.MoveCount);
            Assert.IsFalse(timeline.CanPrevious);
            Assert.IsNull(timeline.SelectedMove);
            for (Int32 ply = 1; ply <= timeline.MoveCount; ply++)
            {
                timeline.Next();
                Assert.AreEqual(ply, timeline.SelectedPly);
                Assert.AreEqual(Position.GetPosition(imported.Game.Moves[ply - 1].ResultingPosition).FEN_Notation, timeline.SelectedPosition.FEN_Notation);
                Assert.AreEqual(imported.Game.Moves[ply - 1].Move, timeline.SelectedMove.Value);
                Assert.IsFalse(String.IsNullOrWhiteSpace(timeline.GetMoveText(ply - 1)));
            }
            Assert.IsFalse(timeline.CanNext);
            timeline.Next();
            Assert.AreEqual(timeline.MoveCount, timeline.SelectedPly);
            Assert.AreEqual(imported.FinalPosition.FEN_Notation, timeline.SelectedPosition.FEN_Notation);
            for (Int32 ply = timeline.MoveCount - 1; ply >= 0; ply--)
            {
                timeline.Previous();
                PositionDataSerializable expected = ply == 0 ? imported.Game.StartingPosition : imported.Game.Moves[ply - 1].ResultingPosition;
                Assert.AreEqual(Position.GetPosition(expected).FEN_Notation, timeline.SelectedPosition.FEN_Notation);
            }
            timeline.Previous();
            Assert.AreEqual(0, timeline.SelectedPly);
            Assert.IsNull(timeline.SelectedMove);
            CollectionAssert.AreEqual(original, File.ReadAllBytes(this.path));
        }

        [TestMethod]
        public void PgnGameUsesTheSameReplayTimelineAsNativeExample()
        {
            GameImportResult native = GameFile.Import(this.path);
            String pgnPath = Path.Combine(this.directory, "example.PGN");
            GameFile.ExportPgn(pgnPath, native.Game);
            AnalysisTimeline pgn = new(GameFile.Import(pgnPath));
            AnalysisTimeline binary = new(native);
            Assert.AreEqual(binary.MoveCount, pgn.MoveCount);
            for (Int32 ply = 0; ply <= binary.MoveCount; ply++)
            {
                pgn.Select(ply);
                binary.Select(ply);
                Assert.AreEqual(binary.SelectedPosition.FEN_Notation, pgn.SelectedPosition.FEN_Notation);
            }
        }

        [TestMethod]
        public void SelectingArbitraryMoveAndMutatingReturnedPositionDoesNotChangeTimeline()
        {
            GameImportResult imported = GameFile.Import(this.path);
            AnalysisTimeline timeline = new(imported);
            Int32 middle = timeline.MoveCount / 2;
            timeline.Select(middle);
            String expected = timeline.SelectedPosition.FEN_Notation;
            Position snapshot = timeline.SelectedPosition;
            snapshot.OnMove = snapshot.OnMove == Colours.White ? Colours.Black : Colours.White;
            Assert.AreEqual(expected, timeline.SelectedPosition.FEN_Notation);
            timeline.Select(timeline.MoveCount);
            timeline.Select(middle);
            Assert.AreEqual(expected, timeline.SelectedPosition.FEN_Notation);
            _ = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => timeline.Select(-1));
            _ = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => timeline.Select(timeline.MoveCount + 1));
            Assert.AreEqual(middle, timeline.SelectedPly);
        }

        [TestMethod]
        public void EmptyBlackToMoveGameKeepsInitialCountersAndDisablesNavigation()
        {
            Position initial = PositionFile.ParseFen("7k/6pp/8/8/8/8/PP6/K7 b - - 7 12");
            GameDataSerializable game = GameDataSerializable.Create(initial, PlayerType.Player, PlayerType.Player);
            AnalysisTimeline timeline = new(new GameImportResult(game, initial, Array.Empty<GameImportWarning>()));
            Assert.AreEqual(0, timeline.MoveCount);
            Assert.IsFalse(timeline.CanNext);
            Assert.IsFalse(timeline.CanPrevious);
            timeline.Next();
            timeline.Previous();
            Assert.AreEqual(initial.FEN_Notation, timeline.SelectedPosition.FEN_Notation);
        }

        [TestMethod]
        public void BlackStartingGameUsesCorrectMoveNumbersAndPreservesInitialPosition()
        {
            Position initial = PositionFile.ParseFen("7k/6pp/8/8/8/8/PP6/K7 b - - 7 12");
            GameDataSerializable game = GameDataSerializable.Create(initial, PlayerType.Player, PlayerType.Player);
            Position after = initial.Clone();
            after.Move("g7-g6");
            game.AddMove("g7-g6", after.PositionDataSerializable);
            after.Move("b2-b3");
            game.AddMove("b2-b3", after.PositionDataSerializable);
            AnalysisTimeline timeline = new(new GameImportResult(game, after, Array.Empty<GameImportWarning>()));
            StringAssert.StartsWith(timeline.GetMoveText(0), "12... ");
            StringAssert.StartsWith(timeline.GetMoveText(1), "13. ");
            Assert.AreEqual(initial.FEN_Notation, timeline.SelectedPosition.FEN_Notation);
            timeline.Select(2);
            Assert.AreEqual(after.FEN_Notation, timeline.SelectedPosition.FEN_Notation);
        }
    }
}
