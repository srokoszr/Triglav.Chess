using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Extensions;

namespace Triglav.Chess.Engine.Tests.MovesTests
{
    [TestClass]
    public class GameEndingRulesTests
    {
        [TestMethod]
        [DataRow(Colours.White)]
        [DataRow(Colours.Black)]
        public void NakedMate_IsLossWhenLastEnemyPieceCannotBeCaptured(Colours bareKingColour)
        {
            Position position = new() { OnMove = bareKingColour };
            position.Add(bareKingColour, Pieces.KingMoved, "a1");
            position.Add(bareKingColour.GetOpposite(), Pieces.KingMoved, "h8");
            position.Add(bareKingColour.GetOpposite(), Pieces.Bishop, "f6");
            Assert.IsFalse(position.GetMoves().Any(static x => x.MoveStruct.Move.ToString().EndsWith("-f6")));
            EvaluationWrapper evaluation = PositionEvaluator.GetDefault().Evaluate(position, 1);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.AreEqual(bareKingColour == Colours.White ? EvaluationResult.BlackWinNakedMate
                : EvaluationResult.WhiteWinNakedMate, evaluation.Result);
        }

        [TestMethod]
        [DataRow(Colours.White)]
        [DataRow(Colours.Black)]
        public void DoubleNakedMate_LastPieceCanBeCaptured_LeavesTwoKingsAndDraw(Colours bareKingColour)
        {
            Position position = new() { OnMove = bareKingColour };
            position.Add(bareKingColour, Pieces.KingMoved, "a1");
            position.Add(bareKingColour.GetOpposite(), Pieces.KingMoved, "h8");
            position.Add(bareKingColour.GetOpposite(), Pieces.Bishop, "b2");
            Assert.IsTrue(position.GetMoves().Any(static x => x.MoveStruct.Move == "a1-b2"));
            EvaluationWrapper evaluation = PositionEvaluator.GetDefault().Evaluate(position, 1);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.IsFalse(evaluation.IsDecisive);
            Assert.AreEqual(EvaluationResult.DrawOrUknown, evaluation.Result);
            position.Move("a1-b2");
            Assert.AreEqual(2, position.Count);
            Assert.IsTrue(position.IsWhiteKingAlone);
            Assert.IsTrue(position.IsBlackKingAlone);
            evaluation = PositionEvaluator.GetDefault().Evaluate(position, 1);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.IsFalse(evaluation.IsDecisive);
            Assert.AreEqual(EvaluationResult.DrawOrUknown, evaluation.Result);
        }

        [TestMethod]
        [DataRow(Colours.White, false)]
        [DataRow(Colours.Black, false)]
        [DataRow(Colours.White, true)]
        [DataRow(Colours.Black, true)]
        public void NoLegalMoves_IsLossWithOrWithoutCheck(Colours losingColour, System.Boolean inCheck)
        {
            Position position = new() { OnMove = losingColour };
            position.Add(losingColour, Pieces.KingMoved, "a1");
            // A blocked pawn keeps this from also being a naked-mate position.
            position.Add(losingColour, Pieces.Pawn, "h4");
            position.Add(losingColour.GetOpposite(), Pieces.Pawn, losingColour == Colours.White ? "h5" : "h3");
            position.Add(losingColour.GetOpposite(), Pieces.KingMoved, "c3");
            position.Add(losingColour.GetOpposite(), Pieces.Rook, "b8");
            position.Add(losingColour.GetOpposite(), Pieces.Rook, "g2");
            if (inCheck)
            {
                position.Add(losingColour.GetOpposite(), Pieces.Rook, "a8");
            }

            Assert.AreEqual(inCheck, losingColour == Colours.White ? position.IsWhiteKingChecked : position.IsBlackKingChecked);
            Assert.AreEqual(0, position.GetMoves().Length);
            EvaluationWrapper evaluation = PositionEvaluator.GetDefault().Evaluate(position, 0);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.IsTrue(evaluation.IsDecisive);
            EvaluationResult expected = (losingColour, inCheck) switch
            {
                (Colours.White, true) => EvaluationResult.BlackWinMate,
                (Colours.White, false) => EvaluationResult.BlackWinStalemate,
                (Colours.Black, true) => EvaluationResult.WhiteWinMate,
                _ => EvaluationResult.WhiteWinStalemate
            };
            Assert.AreEqual(expected, evaluation.Result);
        }
    }
}
