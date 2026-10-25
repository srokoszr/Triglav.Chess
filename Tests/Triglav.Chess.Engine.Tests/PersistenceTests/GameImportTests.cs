using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Model.Serializable;
using Triglav.Chess.Engine.Notation;
using Triglav.Chess.Engine.Persistence;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Tests.PersistenceTests
{
    [TestClass]
    public class GameImportTests
    {
        private String directory;

        [TestInitialize]
        public void CreateDirectory()
        {
            this.directory = Path.Combine(Path.GetTempPath(), "TriglavImport-" + Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(this.directory);
        }

        [TestCleanup]
        public void RemoveDirectory() => Directory.Delete(this.directory, recursive: true);

        private String FilePath(String extension) => Path.Combine(this.directory, "game" + extension);

        private static GameDataSerializable Game(Boolean black = false, Int32 count = 2)
        {
            Position position = PositionFile.ParseFen($"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR {(black ? "b" : "w")} QKkq - 7 12");
            GameDataSerializable game = GameDataSerializable.Create(position, PlayerType.Player, PlayerType.Triglav);
            String[] moves = black ? ["g8-f6", "g1-f3"] : ["g1-f3", "g8-f6"];
            foreach (String text in moves.Take(count))
            {
                MoveStruct move = text;
                position.Move(move);
                game.AddMove(move, position.PositionDataSerializable);
            }
            return game;
        }

        [TestMethod]
        [DataRow(false, 0)]
        [DataRow(false, 2)]
        [DataRow(true, 0)]
        [DataRow(true, 2)]
        public void BothFormatsImportTheSameGameAndHistory(Boolean black, Int32 count)
        {
            GameDataSerializable game = Game(black, count);
            GameFile.Save(this.FilePath(".tGgAmE"), game);
            GameFile.ExportPgn(this.FilePath(".pGn"), game);
            GameImportResult native = GameFile.Import(this.FilePath(".tGgAmE"));
            GameImportResult pgn = GameFile.Import(this.FilePath(".pGn"));
            Assert.AreEqual(pgn.FinalPosition.FEN_Notation, native.FinalPosition.FEN_Notation);
            Assert.AreEqual(pgn.Game.Result, native.Game.Result);
            Assert.AreEqual(pgn.Game.WhitePlayer, native.Game.WhitePlayer);
            Assert.AreEqual(pgn.Game.BlackPlayer, native.Game.BlackPlayer);
            Assert.AreEqual(pgn.Game.Metadata.Black, native.Game.Metadata.Black);
            Assert.HasCount(count, native.Game.Moves);
            Assert.HasCount(0, native.Warnings);
            using PositionStackWrapper nativeHistory = native.CreateHistory();
            using PositionStackWrapper pgnHistory = pgn.CreateHistory();
            Assert.AreEqual(count, nativeHistory.Count);
            CollectionAssert.AreEqual(pgnHistory.Select(static p => p.State).ToArray(), nativeHistory.Select(static p => p.State).ToArray());
            if (count > 0)
            {
                Assert.AreEqual(black ? Colours.Black : Colours.White, nativeHistory.First().OnMove);
                for (Int32 index = 0; index < count; index++)
                {
                    Assert.AreEqual(game.Moves[index].Move, native.Game.Moves[index].Move);
                    Assert.AreEqual(Position.GetPosition(pgn.Game.Moves[index].ResultingPosition).FEN_Notation,
                        Position.GetPosition(native.Game.Moves[index].ResultingPosition).FEN_Notation);
                }
            }
        }

        [TestMethod]
        public void NativeImportDoesNotImposePgnMetadataRestrictions()
        {
            GameDataSerializable game = Game();
            game.Metadata.White = new String('W', 300) + "\nNative name";
            game.Metadata.ExtraTags["Result"] = "native metadata";
            GameFile.Save(this.FilePath(".TgGame"), game);
            GameImportResult result = GameFile.Import(this.FilePath(".TgGame"));
            Assert.AreEqual(game.Metadata.White, result.Game.Metadata.White);
            Assert.AreEqual("native metadata", result.Game.Metadata.ExtraTags["Result"]);
            Assert.HasCount(2, result.Game.Moves);
        }

        [TestMethod]
        public void LegacyNativeCountersAreReconstructedWithoutChangingTheFile()
        {
            GameDataSerializable game = Game();
            game.Metadata = null;
            game.StartingPosition.HalfmoveClock = 0;
            game.StartingPosition.FullMoveCounter = 1;
            foreach (MoveSerializable move in game.Moves)
            {
                move.ResultingPosition.HalfmoveClock = 0;
                move.ResultingPosition.FullMoveCounter = 1;
            }
            String path = this.FilePath(".TgGame");
            GameFile.Save(path, game);
            Byte[] before = File.ReadAllBytes(path);
            GameImportResult imported = GameFile.Import(path);
            Assert.IsNull(imported.Game.Metadata);
            Assert.AreEqual(2, imported.FinalPosition.HalfmoveClock);
            Assert.AreEqual(2, imported.FinalPosition.FullMoveCounter);
            Assert.AreEqual(1, imported.Game.Moves[0].ResultingPosition.HalfmoveClock);
            Assert.AreEqual(2, imported.Game.Moves[1].ResultingPosition.FullMoveCounter);
            Assert.HasCount(1, imported.Warnings);
            Assert.AreEqual(GameImportWarningCode.ReconstructedCounters, imported.Warnings[0].Code);
            CollectionAssert.AreEqual(before, File.ReadAllBytes(path));
            using PositionStackWrapper history = imported.CreateHistory();
            Assert.AreEqual(2, history.Count);
        }

        [TestMethod]
        public void NativeReplayRejectsIllegalMovesAndMismatchedSnapshots()
        {
            String path = this.FilePath(".TgGame");
            GameDataSerializable game = Game();
            game.Moves[0].To = 63;
            GameFile.Save(path, game);
            GameReplayException illegal = Assert.ThrowsExactly<GameReplayException>(() => GameFile.Import(path));
            Assert.AreEqual(GameReplayError.IllegalMove, illegal.Error);
            Assert.AreEqual(1, illegal.Ply);
            game = Game();
            game.Moves[0].ResultingPosition = game.StartingPosition;
            GameFile.Save(path, game);
            GameReplayException mismatch = Assert.ThrowsExactly<GameReplayException>(() => GameFile.Import(path));
            Assert.AreEqual(GameReplayError.SnapshotMismatch, mismatch.Error);
            Assert.AreEqual(1, mismatch.Ply);
        }

        [TestMethod]
        public void ImportChecksFormatAndLimitsForBothFormats()
        {
            _ = Assert.ThrowsExactly<NotSupportedException>(() => GameFile.Import(this.FilePath(".other")));
            _ = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => GameFile.Import(this.FilePath(".TgGame"), new GameImportOptions { MaxBytes = 0 }));
            GameDataSerializable game = Game();
            String native = this.FilePath(".TgGame");
            String pgn = this.FilePath(".PGN");
            GameFile.Save(native, game);
            GameFile.ExportPgn(pgn, game);
            _ = Assert.ThrowsExactly<InvalidDataException>(() => GameFile.Import(native, new GameImportOptions { MaxBytes = 1 }));
            _ = Assert.ThrowsExactly<InvalidDataException>(() => GameFile.Import(native, new GameImportOptions { MaxPlies = 1 }));
            _ = Assert.ThrowsExactly<PgnException>(() => GameFile.Import(pgn, new GameImportOptions { MaxBytes = 1 }));
            _ = Assert.ThrowsExactly<PgnException>(() => GameFile.Import(pgn, new GameImportOptions { MaxPlies = 1 }));
            File.WriteAllBytes(native, [255, 255, 255]);
            _ = Assert.ThrowsExactly<InvalidDataException>(() => GameFile.Import(native));
            File.WriteAllText(pgn, "not a game");
            _ = Assert.ThrowsExactly<PgnException>(() => GameFile.Import(pgn));
        }

        [TestMethod]
        public void CommonImportPassesExplicitPgnVariantOptionsAndWarnings()
        {
            String path = this.FilePath(".PGN");
            File.WriteAllText(path, "1. e4 *");
            _ = Assert.ThrowsExactly<PgnException>(() => GameFile.Import(path));
            GameImportResult result = GameFile.Import(path, new GameImportOptions { AssumeTriglavVariant = true });
            Assert.HasCount(1, result.Game.Moves);
            Assert.IsTrue(result.Warnings.Any(static warning => warning.PgnDiagnostic?.Code == PgnDiagnosticCode.UnmarkedVariant));
        }

        [TestMethod]
        public void HistoricalRepetitionHasTheSameHistoryInBothFormats()
        {
            using Stream stream = typeof(GameImportTests).Assembly.GetManifestResourceStream("BlackRc5Threefold.PGN");
            using StreamReader reader = new(stream);
            String pgnPath = this.FilePath(".PGN");
            File.WriteAllText(pgnPath, reader.ReadToEnd());
            GameImportResult pgn = GameFile.Import(pgnPath, new GameImportOptions { AllowLegacyPgn = true });
            Assert.IsTrue(pgn.Warnings.Any(static warning => warning.PgnDiagnostic?.Code == PgnDiagnosticCode.LegacyRepair));
            String nativePath = this.FilePath(".TgGame");
            GameFile.Save(nativePath, pgn.Game);
            GameImportResult native = GameFile.Import(nativePath);
            Assert.AreEqual(GameResult.DrawThreefoldRepetition, native.Game.Result);
            Assert.AreEqual(pgn.FinalPosition.FEN_Notation, native.FinalPosition.FEN_Notation);
            using PositionStackWrapper history = native.CreateHistory();
            Assert.AreEqual(124, history.Count);
            Assert.IsTrue(history.IsThreefoldRepetition(native.FinalPosition));
            MoveStruct extra = native.FinalPosition.GetMoves().First().MoveStruct.Move;
            Position after = native.FinalPosition.Clone();
            after.Move(extra);
            native.Game.AddMove(extra, after.PositionDataSerializable);
            GameFile.Save(nativePath, native.Game);
            GameReplayException error = Assert.ThrowsExactly<GameReplayException>(() => GameFile.Import(nativePath));
            Assert.AreEqual(GameReplayError.MovesAfterEnd, error.Error);
            Assert.AreEqual(125, error.Ply);
        }
    }
}
