using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Model.Serializable;
using Triglav.Chess.Engine.Persistence;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Tests.PersistenceTests
{
    [TestClass]
    public class PersistenceTests
    {
        private String directory;

        [TestInitialize]
        public void CreateDirectory()
        {
            this.directory = Path.Combine(Path.GetTempPath(), "TriglavPersistence-" + Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(this.directory);
        }

        [TestCleanup]
        public void RemoveDirectory() => Directory.Delete(this.directory, recursive: true);

        private String FilePath(String name) => Path.Combine(this.directory, name);

        private static void AssertWriteFailure(Action write)
        {
            try
            {
                write();
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                return;
            }
            Assert.Fail("Writing to an exclusively locked destination should fail.");
        }

        private static void AssertPosition(Position expected, Position actual)
        {
            Assert.AreEqual(expected.State, actual.State, "Piece state, king locations and hash must round-trip.");
            Assert.AreEqual(expected.OnMove, actual.OnMove);
            Assert.AreEqual(expected.HalfmoveClock, actual.HalfmoveClock);
            Assert.AreEqual(expected.FullMoveCounter, actual.FullMoveCounter);
            CollectionAssert.AreEquivalent(expected.GetMoves().Select(static x => x.MoveStruct.Move.ToString()).ToArray(), actual.GetMoves().Select(static x => x.MoveStruct.Move.ToString()).ToArray());
        }

        [TestMethod]
        [DataRow(".TgPosition", "w")]
        [DataRow(".TgPosition", "b")]
        [DataRow(".FEN", "w")]
        [DataRow(".FEN", "b")]
        public void CompletePositionRoundTrip(String extension, String side)
        {
            // Home queens have asymmetric rights; extra queens include both unused and used first moves.
            Position position = Position.ReadFEN($"3qk3/8/1q1Q4/8/8/1Q1q4/8/3QK3 {side} KQ b6,b3 17 42");
            String path = this.FilePath("position" + extension);
            PositionFile.Save(path, position);
            Position loaded = PositionFile.Load(path);
            AssertPosition(position, loaded);
            Assert.AreEqual(Pieces.QueenMoved, loaded.GetPiece("b6"));
            Assert.AreEqual(Pieces.QueenMoved, loaded.GetPiece("b3"));
            Assert.AreEqual(Pieces.Queen, loaded.GetPiece("d6"));
            Assert.AreEqual(Pieces.Queen, loaded.GetPiece("d3"));
            Assert.AreEqual(Pieces.Queen, loaded.GetPiece("d1"));
            Assert.AreEqual(Pieces.QueenMoved, loaded.GetPiece("d8"));
            Assert.IsFalse(loaded.HasWhiteKingMoved);
            Assert.IsTrue(loaded.HasBlackKingMoved);
            if (extension == ".FEN")
            {
                String[] fields = File.ReadAllText(path).Trim().Split(' ');
                Assert.HasCount(6, fields);
                CollectionAssert.AreEquivalent(new[] { "b6", "b3" }, fields[3].Split(','));
            }
        }

        [TestMethod]
        [DataRow("KQ", false, true, Pieces.Queen, Pieces.QueenMoved)]
        [DataRow("kq", true, false, Pieces.QueenMoved, Pieces.Queen)]
        public void FenRightsUseCorrectColour(String rights, Boolean whiteMoved, Boolean blackMoved, Pieces whiteQueen, Pieces blackQueen)
        {
            Position position = Position.ReadFEN($"3qk3/8/8/8/8/8/8/3QK3 w {rights} - 0 1");
            Assert.AreEqual(whiteMoved, position.HasWhiteKingMoved);
            Assert.AreEqual(blackMoved, position.HasBlackKingMoved);
            Assert.AreEqual(whiteQueen, position.GetPiece("d1"));
            Assert.AreEqual(blackQueen, position.GetPiece("d8"));
            AssertPosition(position, Position.ReadFEN(position.FEN_Notation));
        }

        [TestMethod]
        [DataRow(".TgPosition", Colours.White, "a7", "a7-a8", "a8")]
        [DataRow(".FEN", Colours.White, "a7", "a7-a8", "a8")]
        [DataRow(".TgPosition", Colours.Black, "a2", "a2-a1", "a1")]
        [DataRow(".FEN", Colours.Black, "a2", "a2-a1", "a1")]
        public void PromotionAndSubsequentFirstMoveSurviveSave(String extension, Colours side, String square, String promotion, String target)
        {
            Position position = Position.GetEmptyWithKings();
            position.Add(side, Pieces.Pawn, square);
            position.OnMove = side;
            position.Move(promotion);
            String path = this.FilePath("promotion" + extension);
            PositionFile.Save(path, position);
            Position loaded = PositionFile.Load(path);
            AssertPosition(position, loaded);
            Assert.AreEqual(Pieces.Queen, loaded.GetPiece(target));
            // Consume the promoted queen's first move and save the resulting state as well.
            position.OnMove = side;
            position.Move(side == Colours.White ? "a8-c8" : "a1-c1");
            PositionFile.Save(path, position);
            loaded = PositionFile.Load(path);
            AssertPosition(position, loaded);
            Assert.AreEqual(Pieces.QueenMoved, loaded.GetPiece(side == Colours.White ? "c8" : "c1"));
        }

        [TestMethod]
        public void CloneAndBinarySnapshotsPreserveCountersIndependently()
        {
            Position position = Position.GetStartingPosition();
            position.Move("g1-f3");
            position.Move("g8-f6");
            Assert.AreEqual(2, position.HalfmoveClock);
            Assert.AreEqual(2, position.FullMoveCounter);
            Position clone = position.Clone();
            AssertPosition(position, clone);
            AssertPosition(position, (Position)((ICloneable)position).Clone());
            using MemoryStream stream = new();
            position.Save(stream);
            stream.Position = 0;
            AssertPosition(position, Position.Deserialize(stream));
            position.Move("e2-e4");
            Assert.AreEqual(2, clone.HalfmoveClock);
            Assert.AreEqual(Colours.White, clone.OnMove);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void LegacyTextUnderBinaryExtensionStillLoads(Boolean bom)
        {
            Position original = Position.GetStartingPosition();
            String path = this.FilePath("legacy.TgPosition");
            File.WriteAllText(path, original.FEN_Notation, new UTF8Encoding(bom));
            AssertPosition(original, PositionFile.Load(path));
            PositionFile.Save(path, original);
            Assert.AreNotEqual((Byte)'r', File.ReadAllBytes(path)[0]);
            AssertPosition(original, PositionFile.Load(path));
        }

        [TestMethod]
        public void OldBinaryWithoutCountersUsesDefaults()
        {
            Position original = Position.GetStartingPosition();
            PositionDataSerializable data = original.PositionDataSerializable;
            String path = this.FilePath("old.TgPosition");
            using (FileStream stream = File.Create(path))
            {
                Serializer.Serialize(stream, new LegacyPosition
                {
                    TakenFields = data.TakenFields,
                    PieceColours = data.PieceColours,
                    Pieces0 = data.Pieces_0,
                    Pieces1 = data.Pieces_1,
                    Pieces2 = data.Pieces_2,
                    OnMove = data.OnMove,
                });
            }
            AssertPosition(original, PositionFile.Load(path));
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("4k3/8/8/8/8/8/8/4K3 w - - 0")]
        [DataRow("4k3/8/8/8/8/8/8/4K3 w - - 0 1 extra")]
        [DataRow("4k3/8/8/8/8/8/8/4K3 x - - 0 1")]
        [DataRow("4k3/8/8/8/8/8/8/4K3 w - - -1 1")]
        [DataRow("4k3/8/8/8/8/8/8/4K3 w - - 0 0")]
        [DataRow("4k3/8/8/8/8/8/8/4X3 w - - 0 1")]
        [DataRow("4k3/8/8/8/8/8/8/5K3 w - - 0 1")]
        [DataRow("8/8/8/8/8/8/8/4K3 w - - 0 1")]
        [DataRow("4k3/8/8/8/8/8/8/4K3 w KK - 0 1")]
        [DataRow("4k3/8/8/8/8/8/8/4K3 w Q - 0 1")]
        [DataRow("4k3/8/8/8/8/8/8/4K3 w - e3 0 1")]
        [DataRow("4k3/8/8/8/8/8/8/4K3 w - z9 0 1")]
        [DataRow("4k3/8/8/8/8/8/8/3QK3 w Q d1 0 1")]
        [DataRow("4k3/8/8/8/8/8/1Q6/4K3 w - b2,b2 0 1")]
        public void MalformedFenRejected(String fen)
        {
            String path = this.FilePath("bad.FEN");
            File.WriteAllText(path, fen);
            _ = Assert.ThrowsExactly<InvalidDataException>(() => PositionFile.Load(path));
        }

        [TestMethod]
        public void CorruptBinaryAndUnknownExtensionsRejectedWithoutChangingFiles()
        {
            String path = this.FilePath("bad.TgPosition");
            File.WriteAllBytes(path, [0xFF]);
            _ = Assert.ThrowsExactly<InvalidDataException>(() => PositionFile.Load(path));
            using (FileStream stream = File.Create(path))
            {
                Serializer.Serialize(stream, new PositionDataSerializable());
            }
            _ = Assert.ThrowsExactly<InvalidDataException>(() => PositionFile.Load(path));
            String unknown = this.FilePath("existing.txt");
            File.WriteAllText(unknown, "keep");
            _ = Assert.ThrowsExactly<NotSupportedException>(() => PositionFile.Save(unknown, Position.GetStartingPosition()));
            _ = Assert.ThrowsExactly<NotSupportedException>(() => PositionFile.Load(unknown));
            Assert.AreEqual("keep", File.ReadAllText(unknown));
        }

        [TestMethod]
        public void InterruptedWriteKeepsOriginalAndRemovesTemporaryFile()
        {
            String path = this.FilePath("existing.TgPosition");
            File.WriteAllText(path, "original");
            _ = Assert.ThrowsExactly<IOException>(() => AtomicFile.Write(path, stream =>
            {
                stream.Write(Encoding.UTF8.GetBytes("partial replacement"));
                throw new IOException("Simulated interrupted serialization.");
            }));
            Assert.AreEqual("original", File.ReadAllText(path));
            Assert.HasCount(1, Directory.GetFiles(this.directory));
        }

        [TestMethod]
        public void LockedDestinationKeepsOriginalAndRemovesTemporaryFile()
        {
            String path = this.FilePath("locked.TgPosition");
            Position original = Position.GetStartingPosition();
            PositionFile.Save(path, original);
            Byte[] bytes = File.ReadAllBytes(path);
            using (FileStream locked = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                AssertWriteFailure(() => PositionFile.Save(path, Position.GetEmptyWithKings()));
            }
            CollectionAssert.AreEqual(bytes, File.ReadAllBytes(path));
            Assert.HasCount(1, Directory.GetFiles(this.directory));
        }

        [TestMethod]
        [DataRow(GameResult.Unknown)]
        [DataRow(GameResult.WhiteWinMate)]
        [DataRow(GameResult.BlackWinMate)]
        public void RepeatedGameSavesReplaceRatherThanAppendAndPreserveResults(GameResult result)
        {
            Position position = Position.ReadFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w QKkq - 12 9");
            GameDataSerializable game = new()
            {
                StartingPosition = position.PositionDataSerializable
            };
            String path = this.FilePath("game.TgGame");
            GameFile.Save(path, game);
            Assert.HasCount(0, GameFile.Load(path).Moves);
            position.Move("g1-f3");
            game.AddMove((MoveStruct)"g1-f3", position.PositionDataSerializable);
            game.Result = result;
            GameFile.Save(path, game);
            GameFile.Save(path, game);
            GameDataSerializable loaded = GameFile.Load(path);
            Assert.HasCount(1, loaded.Moves);
            Assert.AreEqual(result, loaded.Result);
            Assert.AreEqual(12, loaded.StartingPosition.HalfmoveClock);
            AssertPosition(position, Position.GetPosition(loaded.Moves[0].ResultingPosition));
        }

        [TestMethod]
        public void PgnFailureDoesNotPreventBinarySaveOrModifyPreviousExport()
        {
            Position position = Position.GetStartingPosition();
            GameDataSerializable game = new()
            {
                StartingPosition = position.PositionDataSerializable
            };
            position.Move("e2-e4");
            game.AddMove((MoveStruct)"e2-e4", position.PositionDataSerializable);
            String path = this.FilePath("game.TgGame");
            GameFile.Save(path, game);
            String pgn = this.FilePath("export.PGN");
            GameFile.ExportPgn(pgn, game);
            Assert.AreEqual(game.PGN, File.ReadAllText(pgn));
            Byte[] bytes = File.ReadAllBytes(pgn);
            using (FileStream locked = File.Open(pgn, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                AssertWriteFailure(() => GameFile.ExportPgn(pgn, game));
                GameFile.Save(path, game);
            }
            CollectionAssert.AreEqual(bytes, File.ReadAllBytes(pgn));
            Assert.HasCount(1, GameFile.Load(path).Moves);
            Assert.AreEqual(GameResult.Unknown, game.Result);
        }

        [TestMethod]
        public void RecoveryPgnUpdatesSameFileAfterMovesAndResults()
        {
            Position position = Position.GetStartingPosition();
            GameDataSerializable game = new()
            {
                StartingPosition = position.PositionDataSerializable
            };
            PgnRecoveryFile recovery = new(this.directory);
            recovery.Save(game);
            Assert.AreEqual(this.directory, Path.GetDirectoryName(recovery.FilePath));
            Assert.AreEqual(game.PGN, File.ReadAllText(recovery.FilePath));
            position.Move("e2-e4");
            game.AddMove((MoveStruct)"e2-e4", position.PositionDataSerializable);
            recovery.Save(game);
            Assert.AreEqual(game.PGN, File.ReadAllText(recovery.FilePath));
            game.Result = GameResult.BlackWinResign;
            recovery.Save(game);
            Assert.AreEqual(game.PGN, File.ReadAllText(recovery.FilePath));
            Assert.HasCount(1, Directory.GetFiles(this.directory));
        }

        [TestMethod]
        public void RecoveryFilesAreUniqueAndSurviveBinarySaveFailure()
        {
            GameDataSerializable game = new()
            {
                StartingPosition = Position.GetStartingPosition().PositionDataSerializable
            };
            PgnRecoveryFile first = new(this.directory);
            PgnRecoveryFile second = new(this.directory);
            Assert.AreNotEqual(first.FilePath, second.FilePath);
            _ = Assert.Throws<IOException>(() => GameFile.Save(this.FilePath("missing/game.TgGame"), game));
            first.Save(game);
            second.Save(game);
            Assert.AreEqual(game.PGN, File.ReadAllText(first.FilePath));
            Assert.AreEqual(game.PGN, File.ReadAllText(second.FilePath));
            Assert.HasCount(2, Directory.GetFiles(this.directory));
        }

        [TestMethod]
        public void InvalidGameCannotOverwriteGoodSave()
        {
            GameDataSerializable game = new()
            {
                StartingPosition = Position.GetStartingPosition().PositionDataSerializable
            };
            String path = this.FilePath("game.TgGame");
            GameFile.Save(path, game);
            game.Moves.Add(new MoveSerializable { From = 256, To = 1, ResultingPosition = game.StartingPosition });
            _ = Assert.ThrowsExactly<InvalidDataException>(() => GameFile.Save(path, game));
            Assert.HasCount(0, GameFile.Load(path).Moves);
        }

        [ProtoContract]
        public class LegacyPosition
        {
            [ProtoMember(1)]
            public UInt64 TakenFields
            {
                get; set;
            }
            [ProtoMember(2)]
            public UInt64 PieceColours
            {
                get; set;
            }
            [ProtoMember(3)]
            public UInt64 Pieces0
            {
                get; set;
            }
            [ProtoMember(4)]
            public UInt64 Pieces1
            {
                get; set;
            }
            [ProtoMember(5)]
            public UInt64 Pieces2
            {
                get; set;
            }
            [ProtoMember(6)]
            public Int32 OnMove
            {
                get; set;
            }
        }
    }
}
