using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.Engine.Tests.MovesTests
{
    [TestClass]
    public class ComplexPositionTests
    {
        private static Position SmotheredPosition() => new()
        {
            { Colours.White, Pieces.KingMoved, "a1" },
            { Colours.White, Pieces.Rook, "d8" },
            { Colours.White, Pieces.Rook, "d1" },
            { Colours.White, Pieces.Knight, "e6" },
            { Colours.Black, Pieces.KingMoved, "a8" },
            { Colours.Black, Pieces.Rook, "h8" },
            { Colours.Black, Pieces.Knight, "b8" },
            { Colours.Black, Pieces.Pawn, "a7" },
            { Colours.Black, Pieces.Pawn, "b7" },
        };

        [TestMethod]
        public void RookCheck_AllowsKingToCaptureUndefendedRook()
        {
            Position position = SmotheredPosition();
            Assert.IsTrue(position.GetMoves().Any(static x => x.MoveStruct.Move == "d8-b8"));
            position.Move("d8-b8");
            Assert.AreEqual(Colours.Black, position.OnMove);
            Assert.IsTrue(position.IsBlackKingChecked);
            CollectionAssert.AreEquivalent(new[] { "a8-b8", "h8-b8" },
                position.GetMoves().Select(static x => x.MoveStruct.Move.ToString()).ToArray());
            Assert.IsFalse(PositionEvaluator.GetDefault().Evaluate(position, 0).IsTerminal);
        }

        [TestMethod]
        public void MateInOneSmothered_2()
        {
            Position position = SmotheredPosition();
            Assert.IsTrue(position.GetMoves().Any(static x => x.MoveStruct.Move == "e6-c7"));
            position.Move("e6-c7");
            Assert.AreEqual(Colours.Black, position.OnMove);
            Assert.IsTrue(position.IsBlackKingChecked);
            Assert.AreEqual(0, position.GetMoves().Length);
            EvaluationWrapper evaluation = PositionEvaluator.GetDefault().Evaluate(position, 0);
            Assert.IsTrue(evaluation.IsTerminal);
            Assert.IsTrue(evaluation.IsDecisive);
            Assert.AreEqual(EvaluationResult.WhiteWinMate, evaluation.Result);
        }
    }
}
