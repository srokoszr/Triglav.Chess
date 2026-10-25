using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Core.Algorithms;
using Triglav.Chess.Model.Enumerations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Tests.BSTMinMaxTests
{
    [TestClass]
    public class EndGameTests
    {
        [TestMethod]
        public void MateInOne_1()
        {
            var position = new Position()
            {
                {Colours.White, Pieces.KingMoved, "d6" },
                {Colours.White, Pieces.Rook, "h7" },
                {Colours.Black, Pieces.KingMoved, "d8" },
                {Colours.White, Pieces.Pawn, "a3" },
                {Colours.Black, Pieces.Pawn, "a4" },
            };
            Console.WriteLine(position.ToString());

            var algorithm = new BreadthFirstTreeSearch();
            var (evaluation, move, topLine, _) = algorithm.GetTopMove(position);

            Console.WriteLine(evaluation);
            Console.WriteLine(move);

            Assert.IsNotNull(topLine);
            PrintTopLine(topLine);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.AreEqual(EvaluationResult.WhiteWinMate, evaluation.Result);
            Assert.AreEqual(1, evaluation.InNMoves);

            Assert.AreEqual(MoveStruct.Parse("h7-h8"), move.Move);
        }

        [TestMethod]
        public void MateInTwo_1_1()
        {
            var position = new Position()
            {
                {Colours.White, Pieces.KingMoved, "c6" },
                {Colours.White, Pieces.Rook, "h7" },
                {Colours.Black, Pieces.KingMoved, "a8" },
                {Colours.White, Pieces.Pawn, "a3" },
                {Colours.Black, Pieces.Pawn, "a4" },
                {Colours.White, Pieces.Pawn, "b4" },
                {Colours.Black, Pieces.Pawn, "c4" },
            };
            Console.WriteLine(position.ToString());

            var algorithm = new BreadthFirstTreeSearch();
            var (evaluation, move, topLine, _) = algorithm.GetTopMove(position);

            Console.WriteLine(evaluation);
            Console.WriteLine(move);

            Assert.IsNotNull(topLine);
            PrintTopLine(topLine);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.AreEqual(EvaluationResult.WhiteWinMate, evaluation.Result);
            Assert.AreEqual(2, evaluation.InNMoves);

            var moves = new MoveStruct[] { MoveStruct.Parse("c6-b6"), MoveStruct.Parse("c6-b5") };
            Assert.IsTrue(moves.Contains(move.Move));
        }

        [TestMethod]
        public void MateInTwo_1_1_H7_A7()
        {
            var position = new Position()
            {
                {Colours.White, Pieces.KingMoved, "c6" },
                {Colours.White, Pieces.Rook, "h7" },
                {Colours.Black, Pieces.KingMoved, "a8" },
                {Colours.White, Pieces.Pawn, "a3" },
                {Colours.Black, Pieces.Pawn, "a4" },
            };

            position.Move("h7-a7");
            Console.WriteLine(position.ToString());

            var algorithm = new BreadthFirstTreeSearch();
            var (evaluation, move, topLine, _) = algorithm.GetTopMove(position);

            Console.WriteLine(evaluation);
            Console.WriteLine(move);

            Assert.IsNotNull(topLine);
            PrintTopLine(topLine);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.AreEqual(EvaluationResult.WhiteWinNakedMate, evaluation.Result);
            Assert.AreEqual(2, evaluation.InNMoves);

            var moves = new MoveStruct[] { MoveStruct.Parse("a8-a7") };
            Assert.IsTrue(moves.Contains(move.Move));
        }

        [TestMethod]
        public void MateInTwo_1_1_H7_A7_A8_A7()
        {
            var position = new Position()
            {
                {Colours.White, Pieces.KingMoved, "c6" },
                {Colours.White, Pieces.Rook, "h7" },
                {Colours.Black, Pieces.KingMoved, "a8" },
                {Colours.White, Pieces.Pawn, "a3" },
                {Colours.Black, Pieces.Pawn, "a4" },
            };

            position.Move("h7-a7");
            position.Move("a8-a7");
            Console.WriteLine(position.ToString());

            var algorithm = new BreadthFirstTreeSearch();
            var (evaluation, move, topLine, _) = algorithm.GetTopMove(position);

            Console.WriteLine(evaluation);
            Console.WriteLine(move);

            Assert.IsNotNull(topLine);
            PrintTopLine(topLine);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.AreEqual(EvaluationResult.WhiteWinNakedMate, evaluation.Result);
            Assert.AreEqual(2, evaluation.InNMoves);

            var moves = new MoveStruct[] { MoveStruct.Parse("c6-b5") };
            Assert.IsTrue(moves.Contains(move.Move));
        }

        [TestMethod]
        public void MateInTwo_2()
        {
            var position = new Position()
            {
                {Colours.White, Pieces.KingMoved, "c6" },
                {Colours.White, Pieces.Rook, "h7" },
                {Colours.Black, Pieces.KingMoved, "a8" },
                {Colours.White, Pieces.Pawn, "a2" },
                {Colours.White, Pieces.Pawn, "h6" },
                {Colours.Black, Pieces.Pawn, "a3" },
                {Colours.Black, Pieces.Pawn, "h5" },
            };
            Console.WriteLine(position.ToString());

            var algorithm = new BreadthFirstTreeSearch();
            var (evaluation, move, topLine, _) = algorithm.GetTopMove(position);

            Console.WriteLine(evaluation);
            Console.WriteLine(move);

            Assert.IsNotNull(topLine);
            PrintTopLine(topLine);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.AreEqual(EvaluationResult.WhiteWinMate, evaluation.Result);
            Assert.AreEqual(2, evaluation.InNMoves);

            Assert.AreEqual(MoveStruct.Parse("c6-b6"), move.Move);
        }

        [TestMethod]
        public void StalemateInOne_1()
        {
            var position = new Position()
            {
                {Colours.White, Pieces.KingMoved, "c6" },
                {Colours.White, Pieces.Rook, "h7" },
                {Colours.Black, Pieces.KingMoved, "a8" },
                {Colours.White, Pieces.Pawn, "a2" },
                {Colours.White, Pieces.Pawn, "h6" },
                {Colours.Black, Pieces.Pawn, "a3" },
            };
            Console.WriteLine(position.ToString());

            var algorithm = new BreadthFirstTreeSearch();
            var (evaluation, move, topLine, _) = algorithm.GetTopMove(position, 2);

            Console.WriteLine(evaluation);
            Console.WriteLine(move);

            Assert.IsNotNull(topLine);
            PrintTopLine(topLine);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.AreEqual(EvaluationResult.WhiteWinStalemate, evaluation.Result);
            Assert.AreEqual(1, evaluation.InNMoves);

            Assert.AreEqual(MoveStruct.Parse("h7-b7"), move.Move);
        }

        [TestMethod]
        public void StalemateInOne_MateIn2()
        {
            var position = new Position()
            {
                {Colours.White, Pieces.KingMoved, "c6" },
                {Colours.White, Pieces.Rook, "h7" },
                {Colours.Black, Pieces.KingMoved, "a8" },
                {Colours.White, Pieces.Pawn, "a2" },
                {Colours.White, Pieces.Pawn, "h6" },
                {Colours.Black, Pieces.Pawn, "a3" },
            };
            Console.WriteLine(position.ToString());

            var algorithm = new BreadthFirstTreeSearch();
            var (evaluation, move, topLine, _) = algorithm.GetTopMove(position, 4);

            Console.WriteLine(evaluation);
            Console.WriteLine(move);

            Assert.IsNotNull(topLine);
            PrintTopLine(topLine);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.AreEqual(EvaluationResult.WhiteWinMate, evaluation.Result);
            Assert.AreEqual(2, evaluation.InNMoves);

            Assert.AreEqual(MoveStruct.Parse("c6-b6"), move.Move);
        }

        [TestMethod]
        public void MateInOneSmothered_TopMove()
        {
            var position = new Position()
            {
                {Colours.White, Pieces.KingMoved, "a1" },
                {Colours.White, Pieces.Rook, "d8" },
                {Colours.White, Pieces.Rook, "d1" },
                {Colours.White, Pieces.Knight, "e6" },
                {Colours.Black, Pieces.KingMoved, "a8" },
                {Colours.Black, Pieces.Rook, "h8" },
                {Colours.Black, Pieces.Knight, "b8" },
                {Colours.Black, Pieces.Pawn, "a7" },
                {Colours.Black, Pieces.Pawn, "b7" },
            };
            Console.WriteLine(position.ToString());

            var arr = position.ToArray();

            var algorithm = new BreadthFirstTreeSearch();

            var (evaluation, move, topLine, _) = algorithm.GetTopMove(position);

            Console.WriteLine(evaluation);
            Console.WriteLine(move);

            Assert.IsNotNull(topLine);
            PrintTopLine(topLine);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.AreEqual(EvaluationResult.WhiteWinMate, evaluation.Result);
            Assert.AreEqual(1, evaluation.InNMoves);

            Assert.AreEqual(MoveStruct.Parse("e6-c7"), move.Move);
        }

        [TestMethod]
        public void MateInThree_1()
        {
            var position = new Position()
            {
                {Colours.White, Pieces.KingMoved, "f7" },
                {Colours.White, Pieces.Rook, "f1" },
                {Colours.White, Pieces.Rook, "d1" },
                {Colours.White, Pieces.Pawn, "g3" },
                {Colours.White, Pieces.Knight, "f2" },
                {Colours.Black, Pieces.KingMoved, "e5" },
                {Colours.Black, Pieces.Queen, "a1" },
                {Colours.Black, Pieces.Rook, "a6" },
                {Colours.Black, Pieces.Rook, "g6" },
                {Colours.Black, Pieces.Knight, "h7" },
                {Colours.Black, Pieces.Pawn, "b2" },
                {Colours.Black, Pieces.Pawn, "e4" },
                {Colours.Black, Pieces.Pawn, "d6" },
            };
            Console.WriteLine(position.ToString());

            var algorithm = new BreadthFirstTreeSearch();
            var (evaluation, move, topLine, _) = algorithm.GetTopMove(position);

            Console.WriteLine(evaluation);
            Console.WriteLine(move);

            Assert.IsNotNull(topLine);
            PrintTopLine(topLine);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.AreEqual(EvaluationResult.WhiteWinMate, evaluation.Result);
            Assert.AreEqual(3, evaluation.InNMoves);

            Assert.AreEqual(MoveStruct.Parse("f2-g4"), move.Move);
        }

        [TestMethod]
        public void MateInFour_1()
        {
            var position = new Position()
            {
                {Colours.White, Pieces.KingMoved, "g1" },
                {Colours.White, Pieces.Rook, "a2" },
                {Colours.White, Pieces.Rook, "c2" },
                {Colours.White, Pieces.Knight, "c4" },
                {Colours.White, Pieces.Pawn, "a5" },
                {Colours.White, Pieces.Pawn, "b5" },
                {Colours.White, Pieces.Pawn, "e3" },
                {Colours.White, Pieces.Pawn, "g2" },
                {Colours.Black, Pieces.KingMoved, "a8" },
                {Colours.Black, Pieces.Rook, "c8" },
                {Colours.Black, Pieces.Rook, "g3" },
                {Colours.Black, Pieces.Bishop, "d5" },
                {Colours.Black, Pieces.Pawn, "a7" },
                {Colours.Black, Pieces.Pawn, "b7" },
                {Colours.Black, Pieces.Pawn, "f7" },
                {Colours.Black, Pieces.Pawn, "h7" },
            };
            Console.WriteLine(position.ToString());

            var algorithm = new BreadthFirstTreeSearch();
            var (evaluation, move, topLine, _) = algorithm.GetTopMove(position);

            Console.WriteLine(evaluation);
            Console.WriteLine(move);

            Assert.IsNotNull(topLine);
            PrintTopLine(topLine);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.AreEqual(EvaluationResult.WhiteWinMate, evaluation.Result);
            Assert.AreEqual(4, evaluation.InNMoves);

            Assert.AreEqual(MoveStruct.Parse("c4-b6"), move.Move);
        }

        //[Ignore]//Long running
        [TestMethod]
        public void FromLysia()
        {
            var position = new Position()
            {
                {Colours.White, Pieces.KingMoved, "g3" },
                {Colours.White, Pieces.Bishop, "e3" },
                {Colours.White, Pieces.Pawn, "d5" },
                {Colours.White, Pieces.Pawn, "e4" },
                {Colours.White, Pieces.Pawn, "f5" },
                {Colours.White, Pieces.Pawn, "g5" },
                {Colours.White, Pieces.Pawn, "h4" },
                {Colours.Black, Pieces.KingMoved, "d8" },
                {Colours.Black, Pieces.QueenMoved, "d6" },
                {Colours.Black, Pieces.Pawn, "e5" },
                {Colours.Black, Pieces.Pawn, "e7" },
                {Colours.Black, Pieces.Pawn, "f6" },
            };
            Console.WriteLine(position.ToString());

            var algorithm = new BreadthFirstTreeSearch();
            var (evaluation, move, topLine, _) = algorithm.GetTopMove(position, 16);

            Console.WriteLine(evaluation);
            Console.WriteLine(move);

            Assert.IsNotNull(topLine);
            PrintTopLine(topLine);
            /*Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.AreEqual(EvaluationResult.WhiteWin, evaluation.Result);
            Assert.AreEqual(1, evaluation.InNMoves);

            Assert.AreEqual(MoveStruct.Parse("h7-h8"), move.Move);*/
        }

        private static void PrintTopLine(MoveWithPieceStruct[] topLine)
        {
            Console.WriteLine("Top line:");
            for (int i = 0; i + 1 < topLine.Length; i += 2)
            {
                Console.WriteLine($"{(i / 2) + 1} {topLine[i].ToString()}, {topLine[i + 1].ToString()}");
            }
            if (topLine.Length % 2 == 1)
            {
                Console.WriteLine($"{((topLine.Length + 1) / 2)} {topLine[topLine.Length - 1].ToString()}");
            }
        }
    }
}
