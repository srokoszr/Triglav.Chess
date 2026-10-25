using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Triglav.Chess.Engine.Tests.EvaluationTests
{
    [TestClass]
    public class TerminalPositionTests
    {
        [TestMethod]
        public unsafe void GetEmptyWithKings()
        {
            var position = Position.GetEmptyWithKings();
            Console.WriteLine(position.ToString());

            var data = PositionEvaluator.GetDefault().Evaluate(position, 1);
            Console.WriteLine(data);

            Assert.IsFalse(data.IsDecisive);
            Assert.IsTrue(data.IsTerminal);
            Assert.AreEqual(EvaluationResult.DrawOrUknown, data.Result);
        }

        [TestMethod]
        public unsafe void NakedMate_1()
        {
            var position = Position.GetEmptyWithKings();
            position.Set(Colours.White, Pieces.Queen, "d1");
            position.OnMove = Colours.Black;
            Console.WriteLine(position.ToString());

            var data = PositionEvaluator.GetDefault().Evaluate(position, 0);
            Console.WriteLine(data);

            Assert.IsTrue(data.IsDecisive);
            Assert.IsTrue(data.IsTerminal);
            Assert.AreEqual(EvaluationResult.WhiteWinNakedMate, data.Result);
        }

        [TestMethod]
        public unsafe void NakedMate_2()
        {
            var position = Position.GetEmptyWithKings();
            position.Set(Colours.Black, Pieces.Queen, "d8");
            Console.WriteLine(position.ToString());

            var data = PositionEvaluator.GetDefault().Evaluate(position, 0);
            Console.WriteLine(data);

            Assert.IsTrue(data.IsDecisive);
            Assert.IsTrue(data.IsTerminal);
            Assert.AreEqual(EvaluationResult.BlackWinNakedMate, data.Result);
        }

        [TestMethod]
        public unsafe void DoubleNakedMate_1()
        {
            var position = Position.GetEmptyWithKings();
            position.Set(Colours.Black, Pieces.Queen, "d1");
            Console.WriteLine(position.ToString());

            var data = PositionEvaluator.GetDefault().Evaluate(position, 1);
            Console.WriteLine(data);

            Assert.IsFalse(data.IsDecisive);
            Assert.IsTrue(data.IsTerminal);
            Assert.AreEqual(EvaluationResult.DrawOrUknown, data.Result);
        }

        [TestMethod]
        public unsafe void DoubleNakedMate_2()
        {
            var position = Position.GetEmptyWithKings();
            position.Set(Colours.White, Pieces.Queen, "e8");
            position.OnMove = Colours.Black;
            Console.WriteLine(position.ToString());

            var data = PositionEvaluator.GetDefault().Evaluate(position, 1);
            Console.WriteLine(data);

            Assert.IsFalse(data.IsDecisive);
            Assert.IsTrue(data.IsTerminal);
            Assert.AreEqual(EvaluationResult.DrawOrUknown, data.Result);
        }

        [TestMethod]
        public unsafe void Stalemate_1()
        {
            var position = Position.GetEmptyWithKingsNotMoved();
            position.Set(Colours.White, Pieces.Queen, "d1");
            position.Set(Colours.Black, Pieces.Rook, "f8");
            position.Set(Colours.Black, Pieces.Rook, "g8");
            position.Set(Colours.Black, Pieces.Rook, "d7");
            position.Set(Colours.Black, Pieces.Rook, "c8");
            position.Set(Colours.Black, Pieces.Rook, "b8");
            position.Set(Colours.Black, Pieces.Rook, "h3");
            position.Set(Colours.Black, Pieces.Rook, "h2");
            Console.WriteLine(position.ToString());

            var data = PositionEvaluator.GetDefault().Evaluate(position, 0);
            Console.WriteLine(data);

            Assert.IsTrue(data.IsDecisive);
            Assert.IsTrue(data.IsTerminal);
            Assert.AreEqual(EvaluationResult.BlackWinStalemate, data.Result);
        }

        [TestMethod]
        public unsafe void Stalemate_2()
        {
            var position = Position.GetEmptyWithKingsNotMoved();
            position.Set(Colours.Black, Pieces.Queen, "d8");
            position.Set(Colours.White, Pieces.Rook, "b1");
            position.Set(Colours.White, Pieces.Rook, "c1");
            position.Set(Colours.White, Pieces.Rook, "g1");
            position.Set(Colours.White, Pieces.Rook, "d2");
            position.Set(Colours.White, Pieces.Rook, "f1");
            position.Set(Colours.White, Pieces.Rook, "h7");
            position.Set(Colours.White, Pieces.Rook, "h6");
            position.OnMove = Colours.Black;
            Console.WriteLine(position.ToString());

            var data = PositionEvaluator.GetDefault().Evaluate(position, 0);
            Console.WriteLine(data);

            Assert.IsTrue(data.IsDecisive);
            Assert.IsTrue(data.IsTerminal);
            Assert.AreEqual(EvaluationResult.WhiteWinStalemate, data.Result);
        }

        [TestMethod]
        public unsafe void Checkmate_1()
        {
            var position = Position.GetEmptyWithKingsNotMoved();
            position.Set(Colours.White, Pieces.Queen, "d1");
            position.Set(Colours.Black, Pieces.Rook, "f8");
            position.Set(Colours.Black, Pieces.Rook, "g8");
            position.Set(Colours.Black, Pieces.Rook, "d8");
            position.Set(Colours.Black, Pieces.Rook, "c8");
            position.Set(Colours.Black, Pieces.Rook, "b8");
            position.Set(Colours.Black, Pieces.Rook, "h3");
            position.Set(Colours.Black, Pieces.Rook, "h2");
            position.Set(Colours.Black, Pieces.Rook, "e7");
            Console.WriteLine(position.ToString());

            var data = PositionEvaluator.GetDefault().Evaluate(position, 0);
            Console.WriteLine(data);

            Assert.IsTrue(data.IsDecisive);
            Assert.IsTrue(data.IsTerminal);
            Assert.AreEqual(EvaluationResult.BlackWinMate, data.Result);
        }

        [TestMethod]
        public unsafe void Checkmate_2()
        {
            var position = Position.GetEmptyWithKings();
            position.Set(Colours.Black, Pieces.Queen, "e8");
            position.Set(Colours.White, Pieces.Rook, "b1");
            position.Set(Colours.White, Pieces.Rook, "c1");
            position.Set(Colours.White, Pieces.Rook, "g1");
            position.Set(Colours.White, Pieces.Rook, "e2");
            position.Set(Colours.White, Pieces.Rook, "f1");
            position.Set(Colours.White, Pieces.Rook, "h7");
            position.Set(Colours.White, Pieces.Rook, "h6");
            position.Set(Colours.White, Pieces.Rook, "d1");
            position.OnMove = Colours.Black;
            Console.WriteLine(position.ToString());

            var data = PositionEvaluator.GetDefault().Evaluate(position, 0);
            Console.WriteLine(data);

            Assert.IsTrue(data.IsDecisive);
            Assert.IsTrue(data.IsTerminal);
            Assert.AreEqual(EvaluationResult.WhiteWinMate, data.Result);
        }

        [TestMethod]
        public unsafe void MateInOneSmothered_1()
        {
            var position = new Position()
            {
                {Colours.White, Pieces.KingMoved, "a1" },
                {Colours.White, Pieces.Rook, "d8" },
                {Colours.White, Pieces.Rook, "d1" },
                {Colours.White, Pieces.Knight, "c7" },
                {Colours.Black, Pieces.KingMoved, "a8" },
                {Colours.Black, Pieces.Rook, "h8" },
                {Colours.Black, Pieces.Knight, "b8" },
                {Colours.Black, Pieces.Pawn, "a7" },
                {Colours.Black, Pieces.Pawn, "b7" },
            };
            position.OnMove = Colours.Black;
            Console.WriteLine(position.ToString());
            Console.WriteLine();

            var data = PositionEvaluator.GetDefault().Evaluate(position, 0);
            Console.WriteLine(data);

            Assert.IsTrue(data.IsDecisive);
            Assert.IsTrue(data.IsTerminal);
            Assert.AreEqual(EvaluationResult.WhiteWinMate, data.Result);
        }
    }
}