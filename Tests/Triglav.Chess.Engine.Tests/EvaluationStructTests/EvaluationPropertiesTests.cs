using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.KnownPieceLocations;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;

namespace Triglav.Chess.Engine.Tests.EvaluationStructTests
{
    [TestClass]
    public class EvaluationPropertiesTests
    {
        [TestMethod]
        public void StartingPositionEvaluation()
        {
            var position = Position.GetStartingPosition();
            var positionEvaluator = PositionEvaluator.GetDefault();

            EvaluationWrapper evaluation = positionEvaluator.EvaluateNonTerminalPosition(position, 0);
            Assert.IsFalse(evaluation.IsDecisive);
            Assert.IsFalse(evaluation.IsTerminal);
            Assert.IsFalse(evaluation.IsDraw);
        }

        [TestMethod]
        public void RandomEvaluation()
        {
            EvaluationWrapper evaluation = new Evaluation(42, EvaluationResult.DrawOrUknown, 0);
            Assert.IsFalse(evaluation.IsDecisive);
            Assert.IsFalse(evaluation.IsTerminal);
            Assert.IsFalse(evaluation.IsDraw);
        }

        [TestMethod]
        public void DrawIn1()
        {
            EvaluationWrapper evaluation = Evaluation.Draw(0);
            Assert.IsFalse(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.IsTrue(evaluation.IsDraw);
        }

        [TestMethod]
        public void WhiteWinStalemateIn1()
        {
            EvaluationWrapper evaluation = new Evaluation(0, EvaluationResult.WhiteWinStalemate, 1);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.IsFalse(evaluation.IsDraw);
            Assert.IsTrue(evaluation.IsWhiteWin);
        }

        [TestMethod]
        public void WhiteWinNakedMateIn1()
        {
            EvaluationWrapper evaluation = new Evaluation(0, EvaluationResult.WhiteWinNakedMate, 1);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.IsFalse(evaluation.IsDraw);
            Assert.IsTrue(evaluation.IsWhiteWin);
        }

        [TestMethod]
        public void WhiteWinMateIn1()
        {
            EvaluationWrapper evaluation = new Evaluation(0, EvaluationResult.WhiteWinMate, 1);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.IsFalse(evaluation.IsDraw);
            Assert.IsTrue(evaluation.IsWhiteWin);
        }

        [TestMethod]
        public void BlackWinStalemateIn1()
        {
            EvaluationWrapper evaluation = new Evaluation(0, EvaluationResult.BlackWinStalemate, 1);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.IsFalse(evaluation.IsDraw);
            Assert.IsTrue(evaluation.IsBlackWin);
        }

        [TestMethod]
        public void BlackWinNakedMateIn1()
        {
            EvaluationWrapper evaluation = new Evaluation(0, EvaluationResult.BlackWinNakedMate, 1);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.IsFalse(evaluation.IsDraw);
            Assert.IsTrue(evaluation.IsBlackWin);
        }

        [TestMethod]
        public void BlackWinMateIn1()
        {
            EvaluationWrapper evaluation = new Evaluation(0, EvaluationResult.BlackWinMate, 1);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.IsFalse(evaluation.IsDraw);
            Assert.IsTrue(evaluation.IsBlackWin);
        }

        [TestMethod]
        public void DrawIn0()
        {
            EvaluationWrapper evaluation = new Evaluation(0, EvaluationResult.DrawOrUknown, 0);
            Assert.IsFalse(evaluation.IsDecisive);
            Assert.IsFalse(evaluation.IsTerminal);
            Assert.IsFalse(evaluation.IsDraw);
        }

        [TestMethod]
        public void WhiteWinStalemateIn0()
        {
            EvaluationWrapper evaluation = new Evaluation(0, EvaluationResult.WhiteWinStalemate, 0);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.IsFalse(evaluation.IsDraw);
            Assert.IsTrue(evaluation.IsWhiteWin);
        }

        [TestMethod]
        public void WhiteWinNakedMateIn0()
        {
            EvaluationWrapper evaluation = new Evaluation(0, EvaluationResult.WhiteWinNakedMate, 0);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.IsFalse(evaluation.IsDraw);
            Assert.IsTrue(evaluation.IsWhiteWin);
        }

        [TestMethod]
        public void WhiteWinMateIn0()
        {
            EvaluationWrapper evaluation = new Evaluation(0, EvaluationResult.WhiteWinMate, 0);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.IsFalse(evaluation.IsDraw);
            Assert.IsTrue(evaluation.IsWhiteWin);
        }

        [TestMethod]
        public void BlackWinStalemateIn0()
        {
            EvaluationWrapper evaluation = new Evaluation(0, EvaluationResult.BlackWinStalemate, 0);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.IsFalse(evaluation.IsDraw);
            Assert.IsTrue(evaluation.IsBlackWin);
        }

        [TestMethod]
        public void BlackWinNakedMateIn0()
        {
            EvaluationWrapper evaluation = new Evaluation(0, EvaluationResult.BlackWinNakedMate, 0);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.IsFalse(evaluation.IsDraw);
            Assert.IsTrue(evaluation.IsBlackWin);
        }

        [TestMethod]
        public void BlackWinMateIn0()
        {
            EvaluationWrapper evaluation = new Evaluation(0, EvaluationResult.BlackWinMate, 0);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.IsFalse(evaluation.IsDraw);
            Assert.IsTrue(evaluation.IsBlackWin);
        }

        [TestMethod]
        public void MaxEvaluationTest()
        {
            var position = new Position()
            {
                { Colours.White, Pieces.Rook, "a1"},
                { Colours.White, Pieces.Rook, "h1"},
                { Colours.White, Pieces.Knight, "b1"},
                { Colours.White, Pieces.Knight, "g1"},
                { Colours.White, Pieces.King, KnownKingPositions.WhiteKingStartingPosition},
                { Colours.White, Pieces.Queen, KnownQueenPositions.WhiteQueenStartingPosition},
                { Colours.White, Pieces.Bishop, "f1"},
                { Colours.White, Pieces.Bishop, "c1"},

                { Colours.White, Pieces.Pawn, "d2"},
                { Colours.White, Pieces.Pawn, "e2"},
                { Colours.White, Pieces.Pawn, "c2"},
                { Colours.White, Pieces.Pawn, "f2"},
                { Colours.White, Pieces.Pawn, "b2"},
                { Colours.White, Pieces.Pawn, "g2"},
                { Colours.White, Pieces.Pawn, "a2"},
                { Colours.White, Pieces.Pawn, "h2"},

                { Colours.Black, Pieces.King, KnownKingPositions.BlackKingStartingPosition},
            };
            var positionEvaluator = PositionEvaluator.GetDefault();

            EvaluationWrapper evaluation = positionEvaluator.EvaluateNonTerminalPosition(position, 0);
            Console.WriteLine(evaluation.ToString());
            Console.WriteLine(evaluation.EvaluationValue);
            Assert.IsFalse(evaluation.IsDecisive);
            Assert.IsFalse(evaluation.IsTerminal);
            Assert.IsFalse(evaluation.IsDraw);
        }
    }
}
