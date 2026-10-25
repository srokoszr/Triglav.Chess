using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Triglav.Chess.Engine.Tests.MovesTests
{
    [TestClass]
    public class PawnMoves
    {
        [TestMethod]
        public void White_A2()
        {
            var position = new Position()
            {
                { Colours.White, Pieces.Pawn, "a2" },
            };
            var moves = position.GetMovesNoCheckValidation().ToArray();

            Assert.IsNotNull(moves);
            Assert.AreEqual(2, moves.Length);
            Assert.IsTrue(moves.Any(x => x == "a2-a3"));
            Assert.IsTrue(moves.Any(x => x == "a2-a4"));
        }

        [TestMethod]
        public void White_A2_Blocked()
        {
            var position = new Position()
            {
                { Colours.White, Pieces.Pawn, "a2" },
                { Colours.Black, Pieces.Pawn, "a3" },
            };
            var moves = position.GetMovesNoCheckValidation().ToArray();

            Assert.IsNotNull(moves);
            Assert.AreEqual(0, moves.Length);
        }

        [TestMethod]
        public void White_A2_Capture()
        {
            var position = new Position()
            {
                { Colours.White, Pieces.Pawn, "a2" },
                { Colours.Black, Pieces.Pawn, "a3" },
                { Colours.Black, Pieces.Pawn, "b3" },
            };
            var moves = position.GetMovesNoCheckValidation().ToArray();

            Assert.IsNotNull(moves);
            Assert.AreEqual(1, moves.Length);
            Assert.IsTrue(moves.Any(x => x == "a2-b3"));
        }

        [TestMethod]
        public void White_H3()
        {
            var position = new Position()
            {
                { Colours.White, Pieces.Pawn, "h3" },
            };
            var moves = position.GetMovesNoCheckValidation().ToArray();

            Assert.IsNotNull(moves);
            Assert.AreEqual(1, moves.Length);
            Assert.IsTrue(moves.Any(x => x == "h3-h4"));
        }

        [TestMethod]
        public void Black_D7()
        {
            var position = new Position()
            {
                { Colours.Black, Pieces.Pawn, "d7" },
            };
            position.OnMove = Colours.Black;
            var moves = position.GetMovesNoCheckValidation().ToArray();

            Assert.IsNotNull(moves);
            Assert.AreEqual(2, moves.Length);
            Assert.IsTrue(moves.Any(x => x == "d7-d6"));
            Assert.IsTrue(moves.Any(x => x == "d7-d5"));
        }

        [TestMethod]
        public void Black_D6()
        {
            var position = new Position()
            {
                { Colours.Black, Pieces.Pawn, "d6" },
            };
            position.OnMove = Colours.Black;
            var moves = position.GetMovesNoCheckValidation().ToArray();

            Assert.IsNotNull(moves);
            Assert.AreEqual(1, moves.Length);
            Assert.IsTrue(moves.Any(x => x == "d6-d5"));
        }

        [TestMethod]
        public void PromotionTestWhite_1()
        {
            var position = new Position()
            {
                { Colours.White, Pieces.Pawn, "a7" },
            };
            position.OnMove = Colours.White;

            position.Move("a7-a8");
            var piece = position.GetPiece("a8");
            Assert.AreEqual(Pieces.Queen, piece);
        }

        [TestMethod]
        public void PromotionTestWhite_2()
        {
            var position = new Position()
            {
                { Colours.White, Pieces.Pawn, "a7" },
            };
            position.OnMove = Colours.White;

            var moves = position.GetMovesNoCheckValidation().ToArray();

            Assert.IsNotNull(moves);
            Assert.IsTrue(moves.Any(x => x == "a7-a8"));
            Assert.AreEqual(1, moves.Length);

            position.Move(moves[0]);
            var piece = position.GetPiece("a8");
            Assert.AreEqual(Pieces.Queen, piece);
        }

        [TestMethod]
        public void PromotionTestWhite_3()
        {
            var position = Position.GetEmptyWithKings();
            position.Add(Colours.White, Pieces.Pawn, "a7");
            position.OnMove = Colours.White;

            var moves = position.GetMoves();

            Assert.IsNotNull(moves);
            Assert.IsTrue(moves.Any(x => x.MoveStruct.Move == "a7-a8"));

            var move = moves.Single(x => x.MoveStruct.Move == "a7-a8");

            position.Move(move.MoveStruct);
            var piece = position.GetPiece("a8");
            Assert.AreEqual(Pieces.Queen, piece);
        }

        [TestMethod]
        public void PromotionTestWhite_4()
        {
            var position = Position.GetEmptyWithKings();
            position.Add(Colours.White, Pieces.Pawn, "a7");
            position.Add(Colours.Black, Pieces.Rook, "a8");
            position.Add(Colours.Black, Pieces.Rook, "b8");
            position.OnMove = Colours.White;

            var moves = position.GetMoves();

            Assert.IsNotNull(moves);
            Assert.IsTrue(moves.Any(x => x.MoveStruct.Move == "a7-b8"));

            var move = moves.Single(x => x.MoveStruct.Move == "a7-b8");

            position.Move(move.MoveStruct);
            var piece = position.GetPiece("b8");
            Assert.AreEqual(Pieces.Queen, piece);
        }

        [TestMethod]
        public void PromotionTestBlack_1()
        {
            var position = new Position()
            {
                { Colours.Black, Pieces.Pawn, "a2" },
            };
            position.OnMove = Colours.Black;

            position.Move("a2-a1");
            var piece = position.GetPiece("a1");
            Assert.AreEqual(Pieces.Queen, piece);
        }

        [TestMethod]
        public void PromotionTestBlack_2()
        {
            var position = new Position()
            {
                { Colours.Black, Pieces.Pawn, "a2" },
            };
            position.OnMove = Colours.Black;

            var moves = position.GetMovesNoCheckValidation().ToArray();

            Assert.IsNotNull(moves);
            Assert.IsTrue(moves.Any(x => x == "a2-a1"));
            Assert.AreEqual(1, moves.Length);

            position.Move(moves[0]);
            var piece = position.GetPiece("a1");
            Assert.AreEqual(Pieces.Queen, piece);
        }

        [TestMethod]
        public void PromotionTestBlack_3()
        {
            var position = Position.GetEmptyWithKings();
            position.Add(Colours.Black, Pieces.Pawn, "a2");
            position.OnMove = Colours.Black;

            var moves = position.GetMoves();

            Assert.IsNotNull(moves);
            Assert.IsTrue(moves.Any(x => x.MoveStruct.Move == "a2-a1"));

            var move = moves.Single(x => x.MoveStruct.Move == "a2-a1");

            position.Move(move.MoveStruct);
            var piece = position.GetPiece("a1");
            Assert.AreEqual(Pieces.Queen, piece);
        }

        [TestMethod]
        public void PromotionTestBlack_4()
        {
            var position = Position.GetEmptyWithKings();
            position.Add(Colours.Black, Pieces.Pawn, "a2");
            position.Add(Colours.White, Pieces.Rook, "a1");
            position.Add(Colours.White, Pieces.Rook, "b1");
            position.OnMove = Colours.Black;

            var moves = position.GetMoves();

            Assert.IsNotNull(moves);
            Assert.IsTrue(moves.Any(x => x.MoveStruct.Move == "a2-b1"));

            var move = moves.Single(x => x.MoveStruct.Move == "a2-b1");

            position.Move(move.MoveStruct);
            var piece = position.GetPiece("b1");
            Assert.AreEqual(Pieces.Queen, piece);
        }
    }
}
