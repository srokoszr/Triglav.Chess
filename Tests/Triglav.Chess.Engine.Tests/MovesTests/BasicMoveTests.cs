using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Triglav.Chess.Engine.Tests.MovesTests
{
    [TestClass]
    public class BasicMoveTests
    {
        [TestMethod]
        public void FirstMoveTest_e2_e4()
        {
            var position = Position.GetStartingPosition();
            position.Move("e2-e4");

            Console.WriteLine(position.ToString());
            Assert.IsFalse(position.IsFieldTaken("e2"));
            Assert.IsTrue(position.IsFieldTaken("e4"));
            Assert.AreEqual(Colours.White, position.GetPieceColour("e4"));
            Assert.AreEqual(Pieces.Pawn, position.GetPiece("e4"));
            Assert.AreEqual(Colours.Black, position.OnMove);
        }

        [TestMethod]
        public void FirstMoveTest_e7_e5()
        {
            var position = Position.GetStartingPosition();
            position.OnMove = Colours.Black;
            position.Move("e7-e5");

            Console.WriteLine(position.ToString());
            Assert.IsFalse(position.IsFieldTaken("e7"));
            Assert.IsTrue(position.IsFieldTaken("e5"));
            Assert.AreEqual(Colours.Black, position.GetPieceColour("e5"));
            Assert.AreEqual(Pieces.Pawn, position.GetPiece("e5"));
            Assert.AreEqual(Colours.White, position.OnMove);
        }
    }
}
