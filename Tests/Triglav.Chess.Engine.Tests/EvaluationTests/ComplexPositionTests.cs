using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.Engine.Tests.EvaluationTests
{
    [TestClass]
    public class ComplexPositionTests
    {
        [TestMethod]
        public unsafe void StartingPosition()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());

            var data = PositionEvaluator.GetDefault().Evaluate(position, 0);
            Console.WriteLine(data);

            Assert.IsFalse(data.IsTerminal);
            Assert.IsFalse(data.IsDecisive);
            Assert.IsTrue(data.IsUknown);
            Assert.AreEqual(EvaluationResult.DrawOrUknown, data.Result);
            Assert.AreEqual(0M, data.InNMoves);
            Assert.AreEqual(0f, data.EvaluationValue);
        }

        [TestMethod]
        public void WhiteWithRook()
        {
            var position = Position.GetEmptyWithKingsNotMoved();
            position.Set(Colours.White, Pieces.Rook, "a1");
            Console.WriteLine(position.ToString());

            var data = PositionEvaluator.GetDefault().EvaluateNonTerminalPosition(position, 0);
            Console.WriteLine(data);

            Assert.IsFalse(data.IsTerminal);
            Assert.IsFalse(data.IsDecisive);
            Assert.IsTrue(data.IsUknown);
            Assert.AreEqual(EvaluationResult.DrawOrUknown, data.Result);
            Assert.AreEqual(0M, data.InNMoves);
            Assert.IsGreaterThan(0f, data.EvaluationValue);
        }

        [TestMethod]
        public void BlackWithRook()
        {
            var position = Position.GetEmptyWithKingsNotMoved();
            position.Set(Colours.Black, Pieces.Rook, "a8");
            Console.WriteLine(position.ToString());

            var data = PositionEvaluator.GetDefault().EvaluateNonTerminalPosition(position, 0);
            Console.WriteLine(data);

            Assert.IsFalse(data.IsTerminal);
            Assert.IsFalse(data.IsDecisive);
            Assert.IsTrue(data.IsUknown);
            Assert.AreEqual(EvaluationResult.DrawOrUknown, data.Result);
            Assert.AreEqual(0M, data.InNMoves);
            Assert.IsLessThan(0f, data.EvaluationValue);
        }
    }
}
