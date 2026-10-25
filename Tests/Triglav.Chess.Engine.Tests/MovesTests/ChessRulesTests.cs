using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Extensions;

namespace Triglav.Chess.Engine.Tests.MovesTests
{
    [TestClass]
    public class ChessRulesTests
    {
        // Test coordinates use White's perspective; mirror ranks for Black.
        private static String Square(String square, Colours colour) => colour == Colours.White
            ? square : $"{square[0]}{9 - (square[1] - '0')}";

        private static String[] Destinations(Position position, String from, Boolean legal = false) => legal
            ? [.. position.GetMoves().Select(x => x.MoveStruct.Move.ToString()).Where(x => x.StartsWith(from + "-")).Select(x => x[3..])]
            : [.. position.GetMovesNoCheckValidation().ToArray().Select(x => x.ToString()).Where(x => x.StartsWith(from + "-")).Select(x => x[3..])];

        private static void AssertMoves(Position position, String from, params String[] expected) =>
            CollectionAssert.AreEquivalent(expected, Destinations(position, from), $"Moves from {from}, {position.OnMove}: {String.Join(", ", Destinations(position, from))}");

        private static Position WithKings(Colours colour, Boolean kingMoved = true)
        {
            Position position = new() { OnMove = colour };
            position.Add(colour, kingMoved ? Pieces.KingMoved : Pieces.King, Square("e1", colour));
            position.Add(colour.GetOpposite(), Pieces.KingMoved, Square("h8", colour));
            return position;
        }

        [TestMethod]
        public void StartingPosition_HasExactlyTheSpecifiedPieces()
        {
            Position position = Position.GetStartingPosition();
            Pieces[] backRank = [ Pieces.Rook, Pieces.Knight, Pieces.Bishop, Pieces.Queen,
                Pieces.King, Pieces.Bishop, Pieces.Knight, Pieces.Rook ];
            Assert.AreEqual(32, position.Count);
            Assert.AreEqual(Colours.White, position.OnMove);
            foreach (Colours colour in new[] { Colours.White, Colours.Black })
            {
                for (Int32 file = 0; file < 8; file++)
                {
                    String back = Square($"{(Char)('a' + file)}1", colour);
                    String pawn = Square($"{(Char)('a' + file)}2", colour);
                    Assert.AreEqual(backRank[file], position.GetPiece(back), back);
                    Assert.AreEqual(colour, position.GetPieceColour(back), back);
                    Assert.AreEqual(Pieces.Pawn, position.GetPiece(pawn), pawn);
                    Assert.AreEqual(colour, position.GetPieceColour(pawn), pawn);
                }
            }

            for (Char file = 'a'; file <= 'h'; file++)
            {
                for (Int32 rank = 3; rank <= 6; rank++)
                {
                    Assert.IsFalse(position.IsFieldTaken($"{file}{rank}"));
                }
            }
        }

        [TestMethod]
        [DataRow(Colours.White, Pieces.KingMoved)]
        [DataRow(Colours.Black, Pieces.KingMoved)]
        [DataRow(Colours.White, Pieces.QueenMoved)]
        [DataRow(Colours.Black, Pieces.QueenMoved)]
        [DataRow(Colours.White, Pieces.Knight)]
        [DataRow(Colours.Black, Pieces.Knight)]
        [DataRow(Colours.White, Pieces.Bishop)]
        [DataRow(Colours.Black, Pieces.Bishop)]
        [DataRow(Colours.White, Pieces.Rook)]
        [DataRow(Colours.Black, Pieces.Rook)]
        public void NormalMovement_ExactDestinationsFromEverySquare(Colours colour, Pieces piece)
        {
            // Independent geometric expectations also check edges and reject extra moves.
            for (Int32 file = 0; file < 8; file++)
            {
                for (Int32 rank = 1; rank <= 8; rank++)
                {
                    String from = $"{(Char)('a' + file)}{rank}";
                    Position position = new() { OnMove = colour };
                    position.Add(colour, piece, from);
                    List<String> expected = [];
                    for (Int32 toFile = 0; toFile < 8; toFile++)
                    {
                        for (Int32 toRank = 1; toRank <= 8; toRank++)
                        {
                            Int32 dx = Math.Abs(toFile - file), dy = Math.Abs(toRank - rank);
                            Boolean allowed = piece switch
                            {
                                Pieces.KingMoved => Math.Max(dx, dy) == 1,
                                Pieces.QueenMoved => dx == 1 && dy == 1,
                                Pieces.Knight => dx * dy == 2,
                                Pieces.Bishop => dx == 2 && dy == 2,
                                Pieces.Rook => dx == 0 != (dy == 0),
                                _ => false
                            };
                            if (allowed)
                            {
                                expected.Add($"{(Char)('a' + toFile)}{toRank}");
                            }
                        }
                    }

                    AssertMoves(position, from, [.. expected]);
                }
            }
        }

        [TestMethod]
        [DataRow(Colours.White)]
        [DataRow(Colours.Black)]
        public void King_FirstMoveJumpsOverPieces_ThenLosesSpecialMove(Colours colour)
        {
            Position position = WithKings(colour, false);
            foreach (String square in new[] { "d1", "d2", "e2", "f2", "f1" })
            {
                position.Add(colour, Pieces.Pawn, Square(square, colour));
            }

            String[] special = ["c1", "c2", "c3", "d3", "e3", "f3", "g3", "g2", "g1"];
            AssertMoves(position, Square("e1", colour), [.. special.Select(x => Square(x, colour))]);
            position.Move($"{Square("e1", colour)}-{Square("e3", colour)}");
            Assert.AreEqual(Pieces.KingMoved, position.GetPiece(Square("e3", colour)));
            position.OnMove = colour;
            AssertMoves(position, Square("e3", colour), [.. new[] { "d3", "f3", "d4", "e4", "f4" }.Select(x => Square(x, colour))]);
        }

        [TestMethod]
        [DataRow(Colours.White)]
        [DataRow(Colours.Black)]
        public void King_FirstMoveHasExactUnblockedDestinations(Colours colour)
        {
            Position position = WithKings(colour, false);
            AssertMoves(position, Square("e1", colour), [.. new[] { "d1", "d2", "e2", "f2", "f1",
                "c1", "c2", "c3", "d3", "e3", "f3", "g3", "g2", "g1" }.Select(x => Square(x, colour))]);
        }

        [TestMethod]
        [DataRow(Colours.White)]
        [DataRow(Colours.Black)]
        public void Queen_WaitsForKing_ThenGetsOneSpecialMove(Colours colour)
        {
            Position position = WithKings(colour, false);
            position.Add(colour, Pieces.Queen, Square("d1", colour));
            AssertMoves(position, Square("d1", colour));
            position.Move($"{Square("e1", colour)}-{Square("e2", colour)}");
            position.OnMove = colour;
            AssertMoves(position, Square("d1", colour), [.. new[] { "c2", "e2", "b1", "b3", "d3", "f3", "f1" }
                .Where(x => x != "e2").Select(x => Square(x, colour))]);
            position.Move($"{Square("d1", colour)}-{Square("d3", colour)}");
            Assert.AreEqual(Pieces.QueenMoved, position.GetPiece(Square("d3", colour)));
            position.OnMove = colour;
            AssertMoves(position, Square("d3", colour), [.. new[] { "c2", "c4", "e4" }.Select(x => Square(x, colour))]);
        }

        [TestMethod]
        [DataRow(Colours.White)]
        [DataRow(Colours.Black)]
        public void Queen_FirstMoveJumpsOverOccupiedSquares(Colours colour)
        {
            Position position = WithKings(colour);
            position.Add(colour, Pieces.Queen, Square("d1", colour));
            foreach (String square in new[] { "c1", "c2", "d2", "e2" })
            {
                position.Add(colour, Pieces.Pawn, Square(square, colour));
            }

            AssertMoves(position, Square("d1", colour), [.. new[] { "b1", "b3", "d3", "f3", "f1" }.Select(x => Square(x, colour))]);
        }

        [TestMethod]
        [DataRow(Colours.White, Pieces.Knight)]
        [DataRow(Colours.Black, Pieces.Knight)]
        [DataRow(Colours.White, Pieces.Bishop)]
        [DataRow(Colours.Black, Pieces.Bishop)]
        public void KnightAndBishop_JumpOverSurroundingPieces(Colours colour, Pieces piece)
        {
            Position position = new() { OnMove = colour };
            position.Add(colour, piece, "d4");
            foreach (String square in new[] { "c3", "c4", "c5", "d3", "d5", "e3", "e4", "e5" })
            {
                position.Add(colour, Pieces.Pawn, square);
            }

            String[] expected = piece == Pieces.Knight
                ? ["b3", "b5", "c2", "c6", "e2", "e6", "f3", "f5"]
                : ["b2", "b6", "f2", "f6"];
            AssertMoves(position, "d4", expected);
        }

        [TestMethod]
        [DataRow(Colours.White, Pieces.KingMoved, "e4", "c4")]
        [DataRow(Colours.Black, Pieces.KingMoved, "e4", "c4")]
        [DataRow(Colours.White, Pieces.QueenMoved, "e5", "c3")]
        [DataRow(Colours.Black, Pieces.QueenMoved, "e5", "c3")]
        [DataRow(Colours.White, Pieces.Knight, "f5", "b3")]
        [DataRow(Colours.Black, Pieces.Knight, "f5", "b3")]
        [DataRow(Colours.White, Pieces.Bishop, "f6", "b2")]
        [DataRow(Colours.Black, Pieces.Bishop, "f6", "b2")]
        public void LandingSquare_AllowsEnemyCaptureButNotFriendlyPiece(Colours colour, Pieces piece,
            String friendly, String enemy)
        {
            Position position = new() { OnMove = colour };
            position.Add(colour, piece, "d4");
            position.Add(colour, Pieces.Pawn, friendly);
            position.Add(colour.GetOpposite(), Pieces.Pawn, enemy);
            String[] moves = Destinations(position, "d4");
            CollectionAssert.DoesNotContain(moves, friendly);
            CollectionAssert.Contains(moves, enemy);
            position.Move($"d4-{enemy}");
            Assert.IsFalse(position.IsFieldTaken("d4"));
            Assert.AreEqual(colour, position.GetPieceColour(enemy));
            Assert.AreEqual(piece, position.GetPiece(enemy));
        }

        [TestMethod]
        [DataRow(Colours.White)]
        [DataRow(Colours.Black)]
        public void Rook_StopsAtFriendlyAndEnemyBlockers(Colours colour)
        {
            Position position = new() { OnMove = colour };
            position.Add(colour, Pieces.Rook, "d4");
            position.Add(colour, Pieces.Pawn, "d6");
            position.Add(colour, Pieces.Pawn, "b4");
            position.Add(colour.GetOpposite(), Pieces.Pawn, "f4");
            position.Add(colour.GetOpposite(), Pieces.Pawn, "d2");
            AssertMoves(position, "d4", "d5", "c4", "e4", "f4", "d3", "d2");
        }

        [TestMethod]
        [DataRow(Colours.White)]
        [DataRow(Colours.Black)]
        public void Pawn_BothDiagonalCapturesAndBlockedDoubleStep(Colours colour)
        {
            Position position = WithKings(colour);
            position.Add(colour, Pieces.Pawn, Square("d2", colour));
            position.Add(colour.GetOpposite(), Pieces.Pawn, Square("c3", colour));
            position.Add(colour.GetOpposite(), Pieces.Pawn, Square("e3", colour));
            position.Add(colour.GetOpposite(), Pieces.Pawn, Square("d4", colour));
            AssertMoves(position, Square("d2", colour), [.. new[] { "d3", "c3", "e3" }.Select(x => Square(x, colour))]);
            position.Add(colour, Pieces.Pawn, Square("d3", colour));
            AssertMoves(position, Square("d2", colour), [.. new[] { "c3", "e3" }.Select(x => Square(x, colour))]);
            position.SetEmpty(Square("c3", colour));
            position.SetEmpty(Square("e3", colour));
            position.Add(colour, Pieces.Pawn, Square("c3", colour));
            position.Add(colour, Pieces.Pawn, Square("e3", colour));
            AssertMoves(position, Square("d2", colour));
        }

        [TestMethod]
        [DataRow(Colours.White)]
        [DataRow(Colours.Black)]
        public void Pawn_DoubleStepIsUnavailableAfterFirstMove(Colours colour)
        {
            Position position = WithKings(colour);
            position.Add(colour, Pieces.Pawn, Square("d2", colour));
            position.Move($"{Square("d2", colour)}-{Square("d3", colour)}");
            position.OnMove = colour;
            AssertMoves(position, Square("d3", colour), Square("d4", colour));
        }


        [TestMethod]
        [DataRow(Colours.White, Pieces.King)]
        [DataRow(Colours.Black, Pieces.King)]
        [DataRow(Colours.White, Pieces.Queen)]
        [DataRow(Colours.Black, Pieces.Queen)]
        public void SpecialFirstMove_CapturesEnemyButCannotLandOnFriendlyPiece(Colours colour, Pieces piece)
        {
            Position position = WithKings(colour, piece == Pieces.Queen);
            String from = piece == Pieces.King ? "e1" : "d1";
            String friendly = piece == Pieces.King ? "c3" : "b3";
            String enemy = piece == Pieces.King ? "g3" : "f3";
            if (piece == Pieces.Queen)
            {
                position.Add(colour, piece, Square(from, colour));
            }

            String[] expected = Destinations(position, Square(from, colour));
            position.Add(colour, Pieces.Pawn, Square(friendly, colour));
            position.Add(colour.GetOpposite(), Pieces.Pawn, Square(enemy, colour));
            AssertMoves(position, Square(from, colour), [.. expected.Where(x => x != Square(friendly, colour))]);
            position.Move($"{Square(from, colour)}-{Square(enemy, colour)}");
            Assert.AreEqual(colour, position.GetPieceColour(Square(enemy, colour)));
            Assert.AreEqual(piece == Pieces.King ? Pieces.KingMoved : Pieces.QueenMoved,
                position.GetPiece(Square(enemy, colour)));
        }

        [TestMethod]
        [DataRow(Colours.White, Colours.White)]
        [DataRow(Colours.White, Colours.Black)]
        [DataRow(Colours.Black, Colours.White)]
        [DataRow(Colours.Black, Colours.Black)]
        public void Pawn_CannotJumpOverEitherColour(Colours colour, Colours blockerColour)
        {
            Position position = WithKings(colour);
            position.Add(colour, Pieces.Pawn, Square("d2", colour));
            position.Add(blockerColour, Pieces.Pawn, Square("d3", colour));
            AssertMoves(position, Square("d2", colour));
        }

        [TestMethod]
        [DataRow(Colours.White)]
        [DataRow(Colours.Black)]
        public void PromotedPawn_SpecialMoveIsConsumedAfterMoving(Colours colour)
        {
            Position position = WithKings(colour);
            position.Add(colour, Pieces.Pawn, Square("d7", colour));
            position.Move($"{Square("d7", colour)}-{Square("d8", colour)}");
            position.OnMove = colour;
            CollectionAssert.Contains(Destinations(position, Square("d8", colour), legal: true), Square("f6", colour));
            position.Move($"{Square("d8", colour)}-{Square("f6", colour)}");
            position.OnMove = colour;
            Assert.AreEqual(Pieces.QueenMoved, position.GetPiece(Square("f6", colour)));
            AssertMoves(position, Square("f6", colour), [.. new[] { "e5", "g5", "e7", "g7" }.Select(x => Square(x, colour))]);
        }

        [TestMethod]
        [DataRow(Colours.White, false)]
        [DataRow(Colours.Black, false)]
        [DataRow(Colours.White, true)]
        [DataRow(Colours.Black, true)]
        public void PromotedPawn_HasQueenFirstMoveRegardlessOfKing(Colours colour, Boolean kingMoved)
        {
            Position position = WithKings(colour, kingMoved);
            position.Add(colour, Pieces.Pawn, Square("d7", colour));
            String promotion = $"{Square("d7", colour)}-{Square("d8", colour)}";
            Assert.IsTrue(position.GetMoves().Any(x => x.MoveStruct.Move.ToString() == promotion));
            position.Move(promotion);
            position.OnMove = colour;
            Assert.AreEqual(Pieces.Queen, position.GetPiece(Square("d8", colour)));
            // Forward is off the board; sideways and both backward diagonals remain possible.
            AssertMoves(position, Square("d8", colour), [.. new[] { "b8", "f8", "b6", "f6", "c7", "e7" }.Select(x => Square(x, colour))]);
            CollectionAssert.AreEquivalent(Destinations(position, Square("d8", colour)),
                Destinations(position, Square("d8", colour), legal: true));

        }
    }
}
