using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Core.Algorithms;
using Triglav.Chess.Engine.KnownPieceLocations;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Extensions;
using Triglav.Chess.Model.Structs;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Tests.MinMaxTests
{
    [TestClass]
    public class ComplexPositionTests
    {
        [TestMethod]
        public void AvoidMateInOne_1()
        {
            var position = new Position()
            {
            { Colours.White, Pieces.Rook, "b1"},
            { Colours.White, Pieces.Rook, "h1"},
            { Colours.White, Pieces.Knight, "c3"},
            { Colours.White, Pieces.Knight, "f3"},
            { Colours.White, Pieces.King, KnownKingPositions.WhiteKingStartingPosition},
            { Colours.White, Pieces.Queen, KnownQueenPositions.WhiteQueenStartingPosition},
            { Colours.White, Pieces.Bishop, "f1"},
            { Colours.White, Pieces.Bishop, "c1"},

            { Colours.White, Pieces.Pawn, "a4"},
            { Colours.White, Pieces.Pawn, "c2"},
            { Colours.White, Pieces.Pawn, "d4"},
            { Colours.White, Pieces.Pawn, "f2"},
            { Colours.White, Pieces.Pawn, "g2"},
            { Colours.White, Pieces.Pawn, "h2"},

            { Colours.Black, Pieces.Rook, "b8"},
            { Colours.Black, Pieces.Rook, "h8"},
            { Colours.Black, Pieces.KingMoved, "a5"},
            { Colours.Black, Pieces.Queen, KnownQueenPositions.BlackQueenStartingPosition},
            { Colours.Black, Pieces.Bishop, "f8"},
            { Colours.Black, Pieces.Bishop, "a6"},

            { Colours.Black, Pieces.Pawn, "d7"},
            { Colours.Black, Pieces.Pawn, "f6"},
            { Colours.Black, Pieces.Pawn, "c7"},
            { Colours.Black, Pieces.Pawn, "f7"},
            { Colours.Black, Pieces.Pawn, "b7"},
            { Colours.Black, Pieces.Pawn, "g7"},
            { Colours.Black, Pieces.Pawn, "a7"},
            { Colours.Black, Pieces.Pawn, "h7"},
            };
            position.OnMove = Colours.Black;

            Console.WriteLine(position);

            var algorithm = new MinMaxWithAlphaBeta();
            var (evaluation, move) = algorithm.GetTopMove(position);

            Console.WriteLine(evaluation);
            Console.WriteLine(move);

            Assert.AreEqual("c7-c6", move.Move);

        }

        [TestMethod]
        public unsafe void AvoidMateInOne_1_GetMoves()
        {
            var position = new Position()
            {
            { Colours.White, Pieces.Rook, "b1"},
            { Colours.White, Pieces.Rook, "h1"},
            { Colours.White, Pieces.Knight, "c3"},
            { Colours.White, Pieces.Knight, "f3"},
            { Colours.White, Pieces.King, KnownKingPositions.WhiteKingStartingPosition},
            { Colours.White, Pieces.Queen, KnownQueenPositions.WhiteQueenStartingPosition},
            { Colours.White, Pieces.Bishop, "f1"},
            { Colours.White, Pieces.Bishop, "c1"},

            { Colours.White, Pieces.Pawn, "a4"},
            { Colours.White, Pieces.Pawn, "c2"},
            { Colours.White, Pieces.Pawn, "d4"},
            { Colours.White, Pieces.Pawn, "f2"},
            { Colours.White, Pieces.Pawn, "g2"},
            { Colours.White, Pieces.Pawn, "h2"},

            { Colours.Black, Pieces.Rook, "b8"},
            { Colours.Black, Pieces.Rook, "h8"},
            { Colours.Black, Pieces.KingMoved, "a5"},
            { Colours.Black, Pieces.Queen, KnownQueenPositions.BlackQueenStartingPosition},
            { Colours.Black, Pieces.Bishop, "f8"},
            { Colours.Black, Pieces.Bishop, "a6"},

            { Colours.Black, Pieces.Pawn, "d7"},
            { Colours.Black, Pieces.Pawn, "f6"},
            { Colours.Black, Pieces.Pawn, "c7"},
            { Colours.Black, Pieces.Pawn, "f7"},
            { Colours.Black, Pieces.Pawn, "b7"},
            { Colours.Black, Pieces.Pawn, "g7"},
            { Colours.Black, Pieces.Pawn, "a7"},
            { Colours.Black, Pieces.Pawn, "h7"},
            };
            position.OnMove = Colours.Black;

            Console.WriteLine(position);

            fixed (MoveSimulation* moves = stackalloc MoveSimulation[MovesEnumerators.MaxMovesInPosition])
            {
                var movesLength = position.GetMoves(moves);
                for (int i = 0; i < movesLength; i++)
                {
                    var move = *(moves + i);
                    Console.WriteLine(move.MoveStruct);
                    Console.WriteLine(new Position(move.Position, position.OnMove.GetOpposite()).ToString());
                }
            }
        }

        [TestMethod]
        public unsafe void AvoidMateInOne_1_GetAttackedfields()
        {
            var position = new Position()
            {
            { Colours.White, Pieces.Rook, "b1"},
            { Colours.White, Pieces.Rook, "h1"},
            { Colours.White, Pieces.Knight, "c3"},
            { Colours.White, Pieces.Knight, "f3"},
            { Colours.White, Pieces.King, KnownKingPositions.WhiteKingStartingPosition},
            { Colours.White, Pieces.Queen, KnownQueenPositions.WhiteQueenStartingPosition},
            { Colours.White, Pieces.Bishop, "f1"},
            { Colours.White, Pieces.Bishop, "c1"},

            { Colours.White, Pieces.Pawn, "a4"},
            { Colours.White, Pieces.Pawn, "c2"},
            { Colours.White, Pieces.Pawn, "d4"},
            { Colours.White, Pieces.Pawn, "f2"},
            { Colours.White, Pieces.Pawn, "g2"},
            { Colours.White, Pieces.Pawn, "h2"},

            { Colours.Black, Pieces.Rook, "b8"},
            { Colours.Black, Pieces.Rook, "h8"},
            { Colours.Black, Pieces.KingMoved, "a5"},
            { Colours.Black, Pieces.Queen, KnownQueenPositions.BlackQueenStartingPosition},
            { Colours.Black, Pieces.Bishop, "f8"},
            { Colours.Black, Pieces.Bishop, "a6"},

            { Colours.Black, Pieces.Pawn, "d7"},
            { Colours.Black, Pieces.Pawn, "f6"},
            { Colours.Black, Pieces.Pawn, "c7"},
            { Colours.Black, Pieces.Pawn, "f7"},
            { Colours.Black, Pieces.Pawn, "b7"},
            { Colours.Black, Pieces.Pawn, "g7"},
            { Colours.Black, Pieces.Pawn, "a7"},
            { Colours.Black, Pieces.Pawn, "h7"},
            };
            position.OnMove = Colours.Black;

            Console.WriteLine(position);

            var attacked = position.GetAttackedFieldsWhite();
            Console.WriteLine($"{attacked.ToString()}");
        }

        [TestMethod]
        public void AvoidMateInOne_1_B8_C8()
        {
            var position = new Position()
            {
            { Colours.White, Pieces.Rook, "b1"},
            { Colours.White, Pieces.Rook, "h1"},
            { Colours.White, Pieces.Knight, "c3"},
            { Colours.White, Pieces.Knight, "f3"},
            { Colours.White, Pieces.King, KnownKingPositions.WhiteKingStartingPosition},
            { Colours.White, Pieces.Queen, KnownQueenPositions.WhiteQueenStartingPosition},
            { Colours.White, Pieces.Bishop, "f1"},
            { Colours.White, Pieces.Bishop, "c1"},

            { Colours.White, Pieces.Pawn, "a4"},
            { Colours.White, Pieces.Pawn, "c2"},
            { Colours.White, Pieces.Pawn, "d4"},
            { Colours.White, Pieces.Pawn, "f2"},
            { Colours.White, Pieces.Pawn, "g2"},
            { Colours.White, Pieces.Pawn, "h2"},

            { Colours.Black, Pieces.Rook, "b8"},
            { Colours.Black, Pieces.Rook, "h8"},
            { Colours.Black, Pieces.KingMoved, "a5"},
            { Colours.Black, Pieces.Queen, KnownQueenPositions.BlackQueenStartingPosition},
            { Colours.Black, Pieces.Bishop, "f8"},
            { Colours.Black, Pieces.Bishop, "a6"},

            { Colours.Black, Pieces.Pawn, "d7"},
            { Colours.Black, Pieces.Pawn, "f6"},
            { Colours.Black, Pieces.Pawn, "c7"},
            { Colours.Black, Pieces.Pawn, "f7"},
            { Colours.Black, Pieces.Pawn, "b7"},
            { Colours.Black, Pieces.Pawn, "g7"},
            { Colours.Black, Pieces.Pawn, "a7"},
            { Colours.Black, Pieces.Pawn, "h7"},
            };
            position.OnMove = Colours.Black;
            position.Move("b8-c8");
            Console.WriteLine(position);

            var algorithm = new MinMaxWithAlphaBeta();
            var (evaluation, move) = algorithm.GetTopMove(position);

            Console.WriteLine(evaluation);
            Console.WriteLine(move);

            Assert.AreEqual("b1-b5", move.Move);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.AreEqual(EvaluationResult.WhiteWinMate, evaluation.Result);
        }

        [TestMethod]
        public unsafe void AvoidMateInOne_1_C7_C6_B1_B5()
        {
            var position = new Position()
            {
            { Colours.White, Pieces.Rook, "b1"},
            { Colours.White, Pieces.Rook, "h1"},
            { Colours.White, Pieces.Knight, "c3"},
            { Colours.White, Pieces.Knight, "f3"},
            { Colours.White, Pieces.King, KnownKingPositions.WhiteKingStartingPosition},
            { Colours.White, Pieces.Queen, KnownQueenPositions.WhiteQueenStartingPosition},
            { Colours.White, Pieces.Bishop, "f1"},
            { Colours.White, Pieces.Bishop, "c1"},

            { Colours.White, Pieces.Pawn, "a4"},
            { Colours.White, Pieces.Pawn, "c2"},
            { Colours.White, Pieces.Pawn, "d4"},
            { Colours.White, Pieces.Pawn, "f2"},
            { Colours.White, Pieces.Pawn, "g2"},
            { Colours.White, Pieces.Pawn, "h2"},

            { Colours.Black, Pieces.Rook, "b8"},
            { Colours.Black, Pieces.Rook, "h8"},
            { Colours.Black, Pieces.KingMoved, "a5"},
            { Colours.Black, Pieces.Queen, KnownQueenPositions.BlackQueenStartingPosition},
            { Colours.Black, Pieces.Bishop, "f8"},
            { Colours.Black, Pieces.Bishop, "a6"},

            { Colours.Black, Pieces.Pawn, "d7"},
            { Colours.Black, Pieces.Pawn, "f6"},
            { Colours.Black, Pieces.Pawn, "c7"},
            { Colours.Black, Pieces.Pawn, "f7"},
            { Colours.Black, Pieces.Pawn, "b7"},
            { Colours.Black, Pieces.Pawn, "g7"},
            { Colours.Black, Pieces.Pawn, "a7"},
            { Colours.Black, Pieces.Pawn, "h7"},
            };
            position.OnMove = Colours.Black;
            position.Move("c7-c6");
            Console.WriteLine(position);

            position.Move("b1-b5");
            Console.WriteLine(position);

            Console.WriteLine(nameof(position.GetMoves));
            fixed (MoveSimulation* moves = stackalloc MoveSimulation[MovesEnumerators.MaxMovesInPosition])
            {
                var movesLength = position.GetMoves(moves);
                for (int i = 0; i < movesLength; i++)
                {
                    var move_ = *(moves + i);
                    Console.WriteLine(move_.MoveStruct);
                    Console.WriteLine(new Position(move_.Position, position.OnMove.GetOpposite()).ToString());
                }
            }

            Console.WriteLine(nameof(position.GetMovesNoCheckValidation));
            fixed (MoveWithPieceStruct* moves = stackalloc MoveWithPieceStruct[MovesEnumerators.MaxMovesInPosition])
            {
                var movesLength = position.GetMovesNoCheckValidation(moves);
                for (int i = 0; i < movesLength; i++)
                {
                    var move_ = *(moves + i);
                    Console.WriteLine(move_);
                }
            }

            var algorithm = new MinMaxWithAlphaBeta();
            var (evaluation, move) = algorithm.GetTopMove(position);

            Console.WriteLine(evaluation);
            Console.WriteLine(move);

            position.Move("c6-b5");
            Console.WriteLine(position);

            Assert.IsFalse(position.IsBlackKingChecked);

            Assert.AreEqual("c6-b5", move.Move);
            Assert.IsFalse(evaluation.IsDecisive);
            Assert.IsFalse(evaluation.IsTerminal);
            Assert.AreNotEqual(EvaluationResult.WhiteWinMate, evaluation.Result);
        }

        [TestMethod]
        public void AvoidMateInOne_1_B7_B5()
        {
            var position = new Position()
            {
            { Colours.White, Pieces.Rook, "b1"},
            { Colours.White, Pieces.Rook, "h1"},
            { Colours.White, Pieces.Knight, "c3"},
            { Colours.White, Pieces.Knight, "f3"},
            { Colours.White, Pieces.King, KnownKingPositions.WhiteKingStartingPosition},
            { Colours.White, Pieces.Queen, KnownQueenPositions.WhiteQueenStartingPosition},
            { Colours.White, Pieces.Bishop, "f1"},
            { Colours.White, Pieces.Bishop, "c1"},

            { Colours.White, Pieces.Pawn, "a4"},
            { Colours.White, Pieces.Pawn, "c2"},
            { Colours.White, Pieces.Pawn, "d4"},
            { Colours.White, Pieces.Pawn, "f2"},
            { Colours.White, Pieces.Pawn, "g2"},
            { Colours.White, Pieces.Pawn, "h2"},

            { Colours.Black, Pieces.Rook, "b8"},
            { Colours.Black, Pieces.Rook, "h8"},
            { Colours.Black, Pieces.KingMoved, "a5"},
            { Colours.Black, Pieces.Queen, KnownQueenPositions.BlackQueenStartingPosition},
            { Colours.Black, Pieces.Bishop, "f8"},
            { Colours.Black, Pieces.Bishop, "a6"},

            { Colours.Black, Pieces.Pawn, "d7"},
            { Colours.Black, Pieces.Pawn, "f6"},
            { Colours.Black, Pieces.Pawn, "c7"},
            { Colours.Black, Pieces.Pawn, "f7"},
            { Colours.Black, Pieces.Pawn, "b7"},
            { Colours.Black, Pieces.Pawn, "g7"},
            { Colours.Black, Pieces.Pawn, "a7"},
            { Colours.Black, Pieces.Pawn, "h7"},
            };
            position.OnMove = Colours.Black;
            position.Move("b7-b5");
            Console.WriteLine(position);

            var algorithm = new MinMaxWithAlphaBeta();
            var (evaluation, move) = algorithm.GetTopMove(position);

            Console.WriteLine(evaluation);
            Console.WriteLine(move);

            Assert.IsFalse(evaluation.IsDecisive);
            Assert.IsFalse(evaluation.IsTerminal);
            Assert.AreNotEqual(EvaluationResult.WhiteWinMate, evaluation.Result);
        }

        [TestMethod]
        public void EvaluateStarting()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());

            var algorithm = new MinMaxWithAlphaBeta();
            var (evaluation, move) = algorithm.GetTopMove(position);

            Console.WriteLine(evaluation);
            Console.WriteLine(move);

            Assert.IsFalse(evaluation.IsDecisive);
            Assert.IsFalse(evaluation.IsTerminal);
            Assert.AreEqual(EvaluationResult.DrawOrUknown, evaluation.Result);
        }
    }
}
