using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.Engine.Tests.PopularFormats
{
    [TestClass]
    public class PGN_GameNotationTests
    {
        [TestMethod]
        public void StartingPosition_E4()
        {
            var position = Position.GetStartingPosition();

            var move = position.MoveDescriptive("e2-e4");

            var algebraic = move.AlgebraicNotation;

            Assert.AreEqual("e4", algebraic);
        }

        [TestMethod]
        public void StartingPosition_E4_C5()
        {
            var position = Position.GetStartingPosition();

            position.Move("e2-e4");
            var move = position.MoveDescriptive("c7-c5");

            var algebraic = move.AlgebraicNotation;

            Assert.AreEqual("c5", algebraic);
        }

        [TestMethod]
        public void StartingPosition_E4_C5_NF3()
        {
            var position = Position.GetStartingPosition();

            position.Move("e2-e4");
            position.Move("c7-c5");
            var move = position.MoveDescriptive("g1-f3");

            var algebraic = move.AlgebraicNotation;

            Assert.AreEqual("Nf3", algebraic);
        }

        [TestMethod]
        public void StartingPosition_E4_D5_EXD5()
        {
            var position = Position.GetStartingPosition();

            position.Move("e2-e4");
            position.Move("d7-d5");
            var move = position.MoveDescriptive("e4-d5");

            var algebraic = move.AlgebraicNotation;

            Assert.AreEqual("exd5", algebraic);
        }

        [TestMethod]
        public void SimplePromotion()
        {
            var position = Position.GetEmptyWithKingsNotMoved();
            position.Set(Colours.White, Pieces.Pawn, "a7");

            var move = position.MoveDescriptive("a7-a8");

            var algebraic = move.AlgebraicNotation;

            Assert.AreEqual("a8=Q", algebraic);
        }

        [TestMethod]
        public void PromotionWithCapture()
        {
            var position = Position.GetEmptyWithKingsNotMoved();
            position.Set(Colours.Black, Pieces.Rook, "b8");
            position.Set(Colours.White, Pieces.Pawn, "a7");

            var move = position.MoveDescriptive("a7-b8");

            var algebraic = move.AlgebraicNotation;

            Assert.AreEqual("axb8=Q", algebraic);
        }

        [TestMethod]
        public void MoveRookA()
        {
            var position = Position.GetEmptyWithKingsNotMoved();
            position.Set(Colours.White, Pieces.Rook, "a2");
            position.Set(Colours.White, Pieces.Rook, "h2");

            var move = position.MoveDescriptive("a2-b2");

            var algebraic = move.AlgebraicNotation;

            Assert.AreEqual("Rab2", algebraic);
        }

        [TestMethod]
        public void MoveRook1()
        {
            var position = Position.GetEmptyWithKingsNotMoved();
            position.Set(Colours.White, Pieces.Rook, "a1");
            position.Set(Colours.White, Pieces.Rook, "a4");

            var move = position.MoveDescriptive("a1-a3");

            var algebraic = move.AlgebraicNotation;

            Assert.AreEqual("R1a3", algebraic);
        }

        [TestMethod]
        public void MoveRookA_1()
        {
            var position = Position.GetEmptyWithKingsNotMoved();
            position.Set(Colours.White, Pieces.Rook, "a1");
            position.Set(Colours.White, Pieces.Rook, "d4");

            var move = position.MoveDescriptive("a1-a4");

            var algebraic = move.AlgebraicNotation;

            Assert.AreEqual("Raa4", algebraic);
        }

        [TestMethod]
        public void MoveQueenA1()
        {
            var position = Position.GetEmptyWithKings();
            position.Set(Colours.White, Pieces.QueenMoved, "a1");
            position.Set(Colours.White, Pieces.QueenMoved, "c1");
            position.Set(Colours.White, Pieces.QueenMoved, "a3");

            var move = position.MoveDescriptive("a1-b2");

            var algebraic = move.AlgebraicNotation;

            Assert.AreEqual("Qa1b2", algebraic);
        }

        [TestMethod]
        public void MoveQueenA1_1()
        {
            var position = Position.GetEmptyWithKings();
            position.Set(Colours.White, Pieces.QueenMoved, "a1");
            position.Set(Colours.White, Pieces.QueenMoved, "c1");
            position.Set(Colours.White, Pieces.QueenMoved, "a3");
            position.Set(Colours.White, Pieces.QueenMoved, "c3");

            var move = position.MoveDescriptive("a1-b2");

            var algebraic = move.AlgebraicNotation;

            Assert.AreEqual("Qa1b2", algebraic);
        }
    }
}
