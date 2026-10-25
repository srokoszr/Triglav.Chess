using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Model.Serializable;
using Triglav.Chess.Engine.Notation;
using Triglav.Chess.Engine.Persistence;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Tests.PopularFormats
{
    [TestClass]
    public class PgnDocumentTests
    {
        private const String Profile = "[Variant \"Triglav-Medieval\"]\n[TriglavVariantVersion \"1\"]\n";

        private static GameDataSerializable Game(Position initial = null, params String[] moves)
        {
            Position position = initial?.Clone() ?? Position.GetStartingPosition();
            GameDataSerializable game = GameDataSerializable.Create(position, PlayerType.Player, PlayerType.Triglav, new DateTimeOffset(2026, 10, 17, 21, 0, 0, TimeSpan.FromHours(2)));
            foreach (String text in moves)
            {
                MoveStruct move = text;
                Assert.IsTrue(position.GetMoves().Any(candidate => candidate.MoveStruct.Move == move));
                position.Move(move);
                game.AddMove(move, position.PositionDataSerializable);
            }
            return game;
        }

        private static String Movetext(String pgn) => Regex.Replace(pgn[(pgn.IndexOf("\n\n", StringComparison.Ordinal) + 2)..], @"\s+", " ").Trim();
        private static PgnException Reject(String text, PgnDiagnosticCode expected, PgnImportOptions options = null)
        {
            PgnException error = Assert.ThrowsExactly<PgnException>(() => PgnReader.Read(text, options));
            Assert.AreEqual(expected, error.Diagnostic.Code);
            return error;
        }

        [TestMethod]
        public void EmptyExportHasCompleteHeadersAndTerminationMarker()
        {
            String expected = "[Event \"?\"]\n[Site \"?\"]\n[Date \"2026.10.17\"]\n[Round \"?\"]\n[White \"?\"]\n[Black \"Triglav\"]\n[Result \"*\"]\n"
                + Profile + "[TriglavResult \"unfinished\"]\n[WhiteType \"human\"]\n[BlackType \"program\"]\n[PlyCount \"0\"]\n\n*\n\n";
            Assert.AreEqual(expected, Game().PGN);
            Assert.HasCount(0, PgnReader.Read(expected).Game.Moves);
        }

        [TestMethod]
        [DataRow(GameResult.Unknown, "*")]
        [DataRow(GameResult.Error, "*")]
        [DataRow(GameResult.Draw, "1/2-1/2")]
        [DataRow(GameResult.DrawAgreed, "1/2-1/2")]
        [DataRow(GameResult.DrawThreefoldRepetition, "1/2-1/2")]
        [DataRow(GameResult.WhiteWin, "1-0")]
        [DataRow(GameResult.WhiteWinMate, "1-0")]
        [DataRow(GameResult.WhiteWinNakedMate, "1-0")]
        [DataRow(GameResult.WhiteWinStalemate, "1-0")]
        [DataRow(GameResult.WhiteWinResign, "1-0")]
        [DataRow(GameResult.BlackWin, "0-1")]
        [DataRow(GameResult.BlackWinMate, "0-1")]
        [DataRow(GameResult.BlackWinNakedMate, "0-1")]
        [DataRow(GameResult.BlackWinStalemate, "0-1")]
        [DataRow(GameResult.BlackWinResign, "0-1")]
        public void AllResultsUseMatchingAsciiTokens(GameResult result, String score)
        {
            GameDataSerializable game = Game();
            game.Result = result;
            String pgn = game.PGN;
            StringAssert.Contains(pgn, "[Result \"" + score + "\"]");
            Assert.AreEqual(score, Movetext(pgn));
            Assert.IsFalse(pgn.Contains('–'));
        }

        [TestMethod]
        [DataRow(1, "1. e4 *")]
        [DataRow(2, "1. e4 c5 *")]
        [DataRow(3, "1. e4 c5 2. Nf3 *")]
        [DataRow(4, "1. e4 c5 2. Nf3 Nc6 *")]
        public void EveryMoveIsWrittenOnce(Int32 count, String expected)
        {
            String[] moves = ["e2-e4", "c7-c5", "g1-f3", "b8-c6"];
            GameDataSerializable game = Game(null, moves[..count]);
            Assert.AreEqual(expected, Movetext(game.PGN));
            StringAssert.Contains(game.PGN, $"[PlyCount \"{count}\"]");
            Assert.AreEqual(game.PGN, PgnReader.Read(game.PGN).Game.PGN);
        }

        [TestMethod]
        public void BlackStartUsesInitialCountersAndSetupEvenOnStandardBoard()
        {
            Position initial = PositionFile.ParseFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b QKkq - 7 12");
            GameDataSerializable game = Game(initial, "g8-f6", "g1-f3");
            String pgn = game.PGN;
            Assert.AreEqual("12... Nf6 13. Nf3 *", Movetext(pgn));
            StringAssert.Contains(pgn, "[SetUp \"1\"]");
            StringAssert.Contains(pgn, "[PlyCount \"2\"]");
            PgnImportResult imported = PgnReader.Read(pgn);
            Assert.AreEqual(7, imported.Game.StartingPosition.HalfmoveClock);
            Assert.AreEqual(12, imported.Game.StartingPosition.FullMoveCounter);
            Assert.AreEqual(pgn, imported.Game.PGN);
            using PositionStackWrapper history = imported.CreateHistory();
            Assert.AreEqual(2, history.Count);
            Assert.AreEqual(Colours.Black, history.Array[0].OnMove);
            Assert.AreEqual(Colours.White, history.Array[1].OnMove);
        }

        [TestMethod]
        public void PromotionAndConsumedFirstMoveSurviveRoundTrip()
        {
            Position initial = PositionFile.ParseFen("4k2r/P7/8/8/8/8/8/R3K3 w - - 0 1");
            GameDataSerializable game = Game(initial, "a7-a8", "h8-h7", "a8-c8");
            Assert.AreEqual("1. a8=Q Rh7 2. Qc8 *", Movetext(game.PGN));
            PgnImportResult imported = PgnReader.Read(game.PGN);
            Assert.AreEqual(Pieces.QueenMoved, imported.FinalPosition.GetPiece("c8"));
            Assert.AreEqual(game.PGN, imported.Game.PGN);
        }

        [TestMethod]
        public void QueensWithDifferentRightsStillRequireDisambiguation()
        {
            Position position = Position.GetEmptyWithKings();
            position.Add(Colours.White, Pieces.Queen, "a8");
            position.Add(Colours.White, Pieces.QueenMoved, "c8");
            position.Add(Colours.Black, Pieces.Rook, "h8");
            GameDataSerializable game = Game(position, "a8-b7");
            Assert.AreEqual("1. Qab7 *", Movetext(game.PGN));
            Assert.AreEqual(game.PGN, PgnReader.Read(game.PGN).Game.PGN);
            _ = Reject(game.PGN.Replace("Qab7", "Qb7"), PgnDiagnosticCode.AmbiguousMove);
        }

        [TestMethod]
        public void MetadataAndProtocolAreIndependentOfUiCulture()
        {
            GameDataSerializable game = Game(null, "e2-e4");
            game.Metadata.White = "Żółć \"Jan\" \\ klub";
            game.Metadata.Event = "Grunwald";
            game.Metadata.Date = "2020.??.??";
            game.Metadata.ExtraTags["Annotator"] = "Łukasz";
            CultureInfo previous = CultureInfo.CurrentCulture;
            CultureInfo previousUi = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pl-PL");
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("pl-PL");
                String polish = game.PGN;
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                Assert.AreEqual(polish, game.PGN);
                PgnImportResult imported = PgnReader.Read(polish);
                Assert.AreEqual(game.Metadata.White, imported.Game.Metadata.White);
                Assert.AreEqual("Łukasz", imported.Game.Metadata.ExtraTags["Annotator"]);
                Assert.AreEqual(polish, imported.Game.PGN);
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
                CultureInfo.CurrentUICulture = previousUi;
            }
        }

        [TestMethod]
        public void HeaderInjectionAndReservedOverridesAreRejected()
        {
            GameDataSerializable game = Game();
            game.Metadata.Event = "bad\n[Result \"1-0\"]";
            _ = Assert.ThrowsExactly<PgnException>(() => game.PGN);
            game.Metadata.Event = "?";
            game.Metadata.ExtraTags["result"] = "1-0";
            _ = Assert.ThrowsExactly<PgnException>(() => game.PGN);
        }

        [TestMethod]
        public void SnapshotMismatchAndInvalidMoveAreRejectedBeforeReplacingFile()
        {
            String directory = Path.Combine(Path.GetTempPath(), "TriglavPgn-" + Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(directory);
            try
            {
                GameDataSerializable game = Game(null, "g1-f3");
                String path = Path.Combine(directory, "game.PGN");
                GameFile.ExportPgn(path, game);
                String original = File.ReadAllText(path);
                game.Moves[0].ResultingPosition.HalfmoveClock++;
                PgnException error = Assert.ThrowsExactly<PgnException>(() => GameFile.ExportPgn(path, game));
                Assert.AreEqual(PgnDiagnosticCode.SnapshotMismatch, error.Diagnostic.Code);
                Assert.AreEqual(original, File.ReadAllText(path));
                game.Moves[0] = new MoveSerializable(default, game.StartingPosition);
                _ = Assert.ThrowsExactly<InvalidDataException>(() => GameFile.ExportPgn(path, game));
                Assert.AreEqual(original, File.ReadAllText(path));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void BinaryMetadataAndGenericOutcomeRoundTrip()
        {
            GameDataSerializable game = Game(null, "e2-e4");
            game.Metadata.ExtraTags["Annotator"] = "Jan";
            game.Result = GameResult.WhiteWin;
            using MemoryStream stream = new();
            ProtoBuf.Serializer.Serialize(stream, game);
            stream.Position = 0;
            GameDataSerializable copy = ProtoBuf.Serializer.Deserialize<GameDataSerializable>(stream);
            Assert.AreEqual(game.PGN, copy.PGN);
            Assert.AreEqual("2026.10.17", copy.Metadata.Date);
            Assert.AreEqual(GameResult.WhiteWin, copy.Result);
        }

        [TestMethod]
        public void LegacyCounterSnapshotsCanBeReconstructedOnExport()
        {
            GameDataSerializable game = Game(null, "e2-e4", "e7-e5");
            game.Metadata = null;
            foreach (MoveSerializable move in game.Moves)
            {
                move.ResultingPosition.HalfmoveClock = 0;
                move.ResultingPosition.FullMoveCounter = 1;
            }
            PgnImportResult imported = PgnReader.Read(game.PGN);
            Assert.AreEqual(2, imported.FinalPosition.FullMoveCounter);
        }

        [TestMethod]
        [DataRow("1-0", GameResult.WhiteWin)]
        [DataRow("0-1", GameResult.BlackWin)]
        [DataRow("1/2-1/2", GameResult.Draw)]
        [DataRow("*", GameResult.Unknown)]
        public void ScoresWithoutReasonsDoNotInventTerminationCause(String score, GameResult expected)
        {
            PgnImportResult imported = PgnReader.Read(Profile + $"[Result \"{score}\"]\n\n1. e4 {score}");
            Assert.AreEqual(expected, imported.Game.Result);
            Assert.AreEqual(imported.Game.PGN, PgnReader.Read(imported.Game.PGN).Game.PGN);
        }

        [TestMethod]
        public void CommentsAndUnknownHeadersAreHandledExplicitly()
        {
            String text = Profile + "[Annotator \"Jan\"]\n\n1. e4 {note} c5 ;line note\n2. Nf3 *";
            PgnImportResult imported = PgnReader.Read(text);
            Assert.HasCount(3, imported.Game.Moves);
            Assert.IsTrue(imported.Warnings.Any(static warning => warning.Code == PgnDiagnosticCode.CommentsDiscarded));
            Assert.AreEqual("Jan", imported.Game.Metadata.ExtraTags["Annotator"]);
            StringAssert.Contains(imported.Game.PGN, "[Annotator \"Jan\"]");
        }

        [TestMethod]
        public void UnmarkedFilesRequireExplicitVariantChoice()
        {
            _ = Reject("1. e4 *", PgnDiagnosticCode.MissingVariant);
            PgnImportResult imported = PgnReader.Read("1. e4 *", new PgnImportOptions { AssumeTriglavVariant = true });
            Assert.IsTrue(imported.Warnings.Any(static warning => warning.Code == PgnDiagnosticCode.UnmarkedVariant));
            _ = Reject("[Variant \"Standard\"]\n\n*", PgnDiagnosticCode.UnknownVariant, new PgnImportOptions { AssumeTriglavVariant = true });
            _ = Reject("[Variant \"Triglav-Medieval\"]\n[TriglavVariantVersion \"2\"]\n\n*", PgnDiagnosticCode.UnsupportedVersion);
        }

        [TestMethod]
        [DataRow("1. e5 *", PgnDiagnosticCode.IllegalMove)]
        [DataRow("2. e4 *", PgnDiagnosticCode.InvalidNumber)]
        [DataRow("1... e4 *", PgnDiagnosticCode.InvalidNumber)]
        [DataRow("1. e4", PgnDiagnosticCode.InvalidResult)]
        [DataRow("1. e4 (1. d4) *", PgnDiagnosticCode.UnsupportedConstruct)]
        [DataRow("1. e4 $1 *", PgnDiagnosticCode.UnsupportedConstruct)]
        [DataRow("1. e4! *", PgnDiagnosticCode.UnsupportedConstruct)]
        [DataRow("1. O-O *", PgnDiagnosticCode.UnsupportedConstruct)]
        [DataRow("1. a8=N *", PgnDiagnosticCode.UnsupportedConstruct)]
        [DataRow("1. e4 {unclosed", PgnDiagnosticCode.Syntax)]
        [DataRow("* *", PgnDiagnosticCode.UnsupportedConstruct)]
        public void InvalidMovetextReportsStructuredErrors(String movetext, PgnDiagnosticCode expected)
        {
            PgnException error = Reject(Profile + "\n" + movetext, expected);
            Assert.IsGreaterThan(0, error.Diagnostic.Line);
            Assert.IsGreaterThan(0, error.Diagnostic.Column);
        }

        [TestMethod]
        public void IllegalMoveReportsPlyAndSourcePosition()
        {
            PgnException error = Reject(Profile + "\n1. e4 c5\n2. Nf3 e6\n3. Qh8 *", PgnDiagnosticCode.IllegalMove);
            Assert.AreEqual(5, error.Diagnostic.Ply);
            Assert.AreEqual(6, error.Diagnostic.Line);
            Assert.AreEqual("Qh8", error.Diagnostic.Token);
        }

        [TestMethod]
        public void InvalidHeadersResultsAndSetupAreRejected()
        {
            _ = Reject(Profile + "[Result \"1-0\"]\n\n0-1", PgnDiagnosticCode.MismatchedResult);
            _ = Reject(Profile + "[TriglavResult \"white-checkmate\"]\n\n1-0", PgnDiagnosticCode.MismatchedResult);
            _ = Reject(Profile + "[SetUp \"1\"]\n\n*", PgnDiagnosticCode.InvalidFen);
            _ = Reject(Profile + "[FEN \"bad\"]\n\n*", PgnDiagnosticCode.InvalidFen);
            _ = Reject(Profile + "[SetUp \"1\"]\n[FEN \"bad\"]\n\n*", PgnDiagnosticCode.InvalidFen);
            _ = Reject(Profile + "[Date \"2026.02.30\"]\n\n*", PgnDiagnosticCode.InvalidMetadata);
            _ = Reject(Profile + "[Event \"a\"]\n[Event \"b\"]\n\n*", PgnDiagnosticCode.DuplicateHeader);
            _ = Reject(Profile + "[PlyCount \"3\"]\n\n1. e4 *", PgnDiagnosticCode.InvalidMetadata);
            _ = Reject(Profile + "[Event \"unterminated", PgnDiagnosticCode.Syntax);
            _ = Reject(Profile + "[Event \"bad\nline\"]\n\n*", PgnDiagnosticCode.Syntax);
        }

        [TestMethod]
        public void ResourceLimitsEncodingAndBomAreHandled()
        {
            _ = Reject(Profile + "1. e4 c5 *", PgnDiagnosticCode.LimitExceeded, new PgnImportOptions { MaxPlies = 1 });
            _ = Reject(Profile + "*", PgnDiagnosticCode.LimitExceeded, new PgnImportOptions { MaxBytes = 1 });
            using MemoryStream bad = new([0xFF]);
            PgnException error = Assert.ThrowsExactly<PgnException>(() => PgnReader.Read(bad));
            Assert.AreEqual(PgnDiagnosticCode.InvalidEncoding, error.Diagnostic.Code);
            Assert.HasCount(0, PgnReader.Read("\uFEFF" + Profile + "*").Game.Moves);
            using MemoryStream limited = new(Encoding.UTF8.GetBytes(Profile));
            error = Assert.ThrowsExactly<PgnException>(() => PgnReader.Read(limited, new PgnImportOptions { MaxBytes = 8 }));
            Assert.AreEqual(PgnDiagnosticCode.LimitExceeded, error.Diagnostic.Code);
        }

        [TestMethod]
        public void CompleteHistoricalGameImportsWithHistoryAndCanonicalizes()
        {
            using Stream stream = typeof(PgnDocumentTests).Assembly.GetManifestResourceStream("BlackRc5Threefold.PGN");
            using StreamReader reader = new(stream);
            String original = reader.ReadToEnd();
            _ = Assert.ThrowsExactly<PgnException>(() => PgnReader.Read(original));
            PgnImportResult imported = PgnReader.Read(original, new PgnImportOptions { AllowLegacyTriglav = true });
            Assert.HasCount(124, imported.Game.Moves);
            Assert.AreEqual(GameResult.DrawThreefoldRepetition, imported.Game.Result);
            using PositionStackWrapper history = imported.CreateHistory();
            Assert.AreEqual(124, history.Count);
            Assert.IsTrue(history.IsThreefoldRepetition(imported.FinalPosition));
            Assert.IsTrue(imported.Warnings.Any(static warning => warning.Code == PgnDiagnosticCode.LegacyRepair));
            String canonical = imported.Game.PGN;
            PgnImportResult strict = PgnReader.Read(canonical);
            Assert.AreEqual(canonical, strict.Game.PGN);
            Assert.HasCount(124, strict.Game.Moves);
            Assert.AreEqual(imported.FinalPosition.FEN_Notation, strict.FinalPosition.FEN_Notation);
            Assert.AreEqual("1/2-1/2", Movetext(canonical).Split(' ')[^1]);
            Assert.IsTrue(canonical.Split("\n\n")[1].Split('\n').All(static line => line.Length <= 79));
            _ = Reject(original + "63. Kf4", PgnDiagnosticCode.MovesAfterEnd, new PgnImportOptions { AllowLegacyTriglav = true });
        }

        [TestMethod]
        [DataRow(Colours.White, false)]
        [DataRow(Colours.White, true)]
        [DataRow(Colours.Black, false)]
        [DataRow(Colours.Black, true)]
        public void ForcedMateAndStalemateReasonsRoundTrip(Colours losing, Boolean check)
        {
            Colours winning = losing == Colours.White ? Colours.Black : Colours.White;
            Position position = new()
            {
                OnMove = losing
            };
            position.Add(losing, Pieces.KingMoved, "a1");
            position.Add(losing, Pieces.Pawn, "h4");
            position.Add(winning, Pieces.Pawn, losing == Colours.White ? "h5" : "h3");
            position.Add(winning, Pieces.KingMoved, "c3");
            position.Add(winning, Pieces.Rook, "b8");
            position.Add(winning, Pieces.Rook, "g2");
            if (check)
            {
                position.Add(winning, Pieces.Rook, "a8");
            }
            GameDataSerializable game = Game(position);
            game.Result = (losing, check) switch
            {
                (Colours.White, true) => GameResult.BlackWinMate,
                (Colours.White, false) => GameResult.BlackWinStalemate,
                (Colours.Black, true) => GameResult.WhiteWinMate,
                _ => GameResult.WhiteWinStalemate,
            };
            Assert.AreEqual(game.Result, PgnReader.Read(game.PGN).Game.Result);
        }

        [TestMethod]
        [DataRow(Colours.White)]
        [DataRow(Colours.Black)]
        public void BareKingReasonsRoundTrip(Colours losing)
        {
            Colours winning = losing == Colours.White ? Colours.Black : Colours.White;
            Position position = new()
            {
                OnMove = losing
            };
            position.Add(losing, Pieces.KingMoved, "a1");
            position.Add(winning, Pieces.KingMoved, "h8");
            position.Add(winning, Pieces.Bishop, "f6");
            GameDataSerializable game = Game(position);
            game.Result = losing == Colours.White ? GameResult.BlackWinNakedMate : GameResult.WhiteWinNakedMate;
            Assert.AreEqual(game.Result, PgnReader.Read(game.PGN).Game.Result);
        }

        [TestMethod]
        [DataRow(GameResult.Unknown)]
        [DataRow(GameResult.Error)]
        [DataRow(GameResult.Draw)]
        [DataRow(GameResult.DrawAgreed)]
        [DataRow(GameResult.WhiteWinResign)]
        [DataRow(GameResult.BlackWinResign)]
        [DataRow(GameResult.WhiteWin)]
        [DataRow(GameResult.BlackWin)]
        public void NonForcedDetailedResultsRoundTrip(GameResult result)
        {
            GameDataSerializable game = Game(null, "e2-e4");
            game.Result = result;
            PgnImportResult imported = PgnReader.Read(game.PGN);
            Assert.AreEqual(result, imported.Game.Result);
            Assert.AreEqual(game.PGN, imported.Game.PGN);
        }

        [TestMethod]
        public void BareKingDrawWithCheckDoesNotClaimCheckmate()
        {
            Position initial = PositionFile.ParseFen("k7/n7/8/8/8/8/8/R6K w - - 0 1");
            GameDataSerializable game = Game(initial, "a1-a7");
            game.Result = GameResult.Draw;
            Assert.AreEqual("1. Rxa7+ 1/2-1/2", Movetext(game.PGN));
            Assert.AreEqual(GameResult.Draw, PgnReader.Read(game.PGN).Game.Result);
        }

        [TestMethod]
        public void ActualCheckmateUsesHashSuffix()
        {
            Position position = new()
            {
                OnMove = Colours.White
            };
            position.Add(Colours.Black, Pieces.KingMoved, "a1");
            position.Add(Colours.Black, Pieces.Pawn, "h4");
            position.Add(Colours.White, Pieces.Pawn, "h3");
            position.Add(Colours.White, Pieces.KingMoved, "c3");
            position.Add(Colours.White, Pieces.Rook, "b8");
            position.Add(Colours.White, Pieces.Rook, "g2");
            position.Add(Colours.White, Pieces.Rook, "h7");
            GameDataSerializable game = Game(position, "h7-a7");
            game.Result = GameResult.WhiteWinMate;
            Assert.AreEqual("1. Ra7# 1-0", Movetext(game.PGN));
            Assert.AreEqual(GameResult.WhiteWinMate, PgnReader.Read(game.PGN).Game.Result);
        }

        [TestMethod]
        public void MultipleQueenRightsAndCountersSurviveInitialFenRoundTrip()
        {
            Position position = PositionFile.ParseFen("3qk3/8/1q1Q4/8/8/1Q1q4/8/3QK3 b KQ b6,b3 17 42");
            PgnImportResult imported = PgnReader.Read(Game(position).PGN);
            Assert.AreEqual(position.FEN_Notation, imported.FinalPosition.FEN_Notation);
            Assert.AreEqual(position.State, imported.FinalPosition.State);
            using PositionStackWrapper history = imported.CreateHistory();
            Assert.AreEqual(0, history.Count);
        }

        [TestMethod]
        [DataRow("b", 0, 1)]
        [DataRow("w", 2, 1)]
        [DataRow("w", 0, 12)]
        public void SetupDetectsEachIndependentMetadataDifference(String side, Int32 half, Int32 full)
        {
            Position position = PositionFile.ParseFen($"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR {side} QKkq - {half} {full}");
            String pgn = Game(position).PGN;
            StringAssert.Contains(pgn, "[SetUp \"1\"]");
            Assert.AreEqual(position.FEN_Notation, PgnReader.Read(pgn).FinalPosition.FEN_Notation);
        }

        [TestMethod]
        public void RankAndFullSquareDisambiguationRoundTrip()
        {
            Position queens = Position.GetEmptyWithKings();
            foreach (String square in new[] { "a1", "c1", "a3" })
            {
                queens.Add(Colours.White, Pieces.QueenMoved, square);
            }
            queens.Add(Colours.Black, Pieces.Rook, "h8");
            GameDataSerializable game = Game(queens, "a1-b2");
            Assert.AreEqual("1. Qa1b2 *", Movetext(game.PGN));
            Assert.AreEqual(game.PGN, PgnReader.Read(game.PGN).Game.PGN);
            Position rooks = Position.GetEmptyWithKings();
            rooks.Add(Colours.White, Pieces.Rook, "a1");
            rooks.Add(Colours.White, Pieces.Rook, "a4");
            rooks.Add(Colours.Black, Pieces.Rook, "h8");
            game = Game(rooks, "a1-a3");
            Assert.AreEqual("1. R1a3 *", Movetext(game.PGN));
            Assert.AreEqual(game.PGN, PgnReader.Read(game.PGN).Game.PGN);
        }

        [TestMethod]
        public void CaptureNotationRoundTrip()
        {
            GameDataSerializable game = Game(null, "e2-e4", "d7-d5", "e4-d5");
            Assert.AreEqual("1. e4 d5 2. exd5 *", Movetext(game.PGN));
            Assert.AreEqual(game.PGN, PgnReader.Read(game.PGN).Game.PGN);
        }

        [TestMethod]
        public void StreamOwnershipAndHeaderDiagnosticsArePreserved()
        {
            using MemoryStream source = new(Encoding.UTF8.GetBytes(Game().PGN));
            _ = PgnReader.Read(source);
            Assert.IsTrue(source.CanRead);
            PgnException error = Reject(Profile + "[Date \"????.02.31\"]\n\n*", PgnDiagnosticCode.InvalidMetadata);
            Assert.AreEqual(3, error.Diagnostic.Line);
            _ = Reject(Profile + "[result \"*\"]\n\n*", PgnDiagnosticCode.InvalidMetadata);
            _ = Reject(Profile + "[Event \"" + new String('x', 256) + "\"]\n\n*", PgnDiagnosticCode.LimitExceeded);
            _ = Reject(Profile + "\n*\n" + Profile + "*", PgnDiagnosticCode.UnsupportedConstruct);
        }

        [TestMethod]
        public void LegacyResultRepairsAreOptInAndWarningsAreRetained()
        {
            String legacy = "[Date \"?\"]\n[Result 1–0]\n\n1. e4";
            PgnImportResult imported = PgnReader.Read(legacy, new PgnImportOptions { AllowLegacyTriglav = true });
            Assert.AreEqual(GameResult.WhiteWin, imported.Game.Result);
            Assert.IsGreaterThan(1, imported.Warnings.Count);
            StringAssert.Contains(imported.Game.PGN, "[Result \"1-0\"]");
            _ = Reject(Profile + "[Result \"1–0\"]\n\n1–0", PgnDiagnosticCode.IllegalMove);
            _ = Reject("[Result 1-0]\n\n0-1", PgnDiagnosticCode.MismatchedResult, new PgnImportOptions { AllowLegacyTriglav = true });
        }
    }
}
