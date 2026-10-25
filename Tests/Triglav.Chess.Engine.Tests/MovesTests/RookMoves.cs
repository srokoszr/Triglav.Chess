using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Triglav.Chess.Engine.Tests.MovesTests
{
    [TestClass]
    public class RookMoves
    {
        [TestMethod]
        public void SingleRookAlone()
        {
            for (byte i = 0; i < 64; i++)
            {
                var position = new Position()
                {
                    {Colours.White, Pieces.Rook, new Coordinates(i) },
                };

                var movesCount = position.GetMovesNoCheckValidation();
                var attackedFieldsCount = position.GetAttackedFields(ColourConstants.White);

                Assert.AreEqual(14, movesCount.Length);
                Assert.AreEqual(14, attackedFieldsCount.Count);
            }
        }

        [TestMethod]
        public void TwoRooks_1()
        {
            var position = new Position()
            {
                {ColourConstants.White, Pieces.Rook, "a1" },
                {ColourConstants.White, Pieces.Rook, "h8" },
            };
            Console.WriteLine(position.ToString());

            var movesCount = position.GetMovesNoCheckValidation().Length;
            var attackedFieldsCount = position.GetAttackedFields(ColourConstants.White).Count;

            Assert.AreEqual(28, movesCount);
            Assert.AreEqual(26, attackedFieldsCount);
        }


        [TestMethod]
        public void TwoRooks_2()
        {
            var position = new Position()
            {
                {ColourConstants.White, Pieces.Rook, "d4" },
                {ColourConstants.White, Pieces.Rook, "d5" },
            };
            Console.WriteLine(position.ToString());

            var movesCount = position.GetMovesNoCheckValidation().Length;
            var attackedFieldsCount = position.GetAttackedFields(ColourConstants.White).Count;

            Assert.AreEqual(20, movesCount);
            Assert.AreEqual(22, attackedFieldsCount);
        }


        [TestMethod]
        public void TwoRooksWithEnemyRook_1()
        {
            var position = new Position()
            {
                {Colours.White, Pieces.Rook, "d4" },
                {Colours.White, Pieces.Rook, "d5" },
                {Colours.Black, Pieces.Rook, "d6" },
            };
            Console.WriteLine(position.ToString());

            var movesCount = position.GetMovesNoCheckValidation().Length;
            var attackedFieldsCount = position.GetAttackedFields(ColourConstants.White).Count;

            Assert.AreEqual(18, movesCount);
            Assert.AreEqual(20, attackedFieldsCount);
        }
    }
}
