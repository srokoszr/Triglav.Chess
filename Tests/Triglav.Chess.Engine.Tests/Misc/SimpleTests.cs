using System;
using System.Linq;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Model;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Triglav.Chess.Model.Structs.Interoperable;
using Triglav.Chess.Model.Extensions;

namespace Triglav.Chess.Engine.Tests.Misc
{
    [TestClass]
    public class SimpleTests
    {
        [TestMethod]
        public void GenerateStartingPosition()
        {
            Console.WriteLine(Position.GetStartingPosition().ToString());
#if DEBUG
            Console.WriteLine(Position.GetStartingPosition().TakenFieldsAsBinary);
            Console.WriteLine(Position.GetStartingPosition().PieceColoursAsBinary);
            //Console.WriteLine(Position.GetStartingPosition().PiecesAsBinary);
#endif
        }

        [TestMethod]
        public void GenerateStartingPosition_Pieces()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());
            var data = position.ToArray();

            foreach (var piece in data)
            {
                Console.WriteLine($"{piece.Colour} {piece.Piece} {piece.Location} {piece.Location.HumanReadableCoordinates}");
            }
        }

        [TestMethod]
        public unsafe void GenerateStartingPositionGetMovesAndShowThem()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());
            Console.WriteLine();

            fixed (MoveSimulation* moves = stackalloc MoveSimulation[MovesEnumerators.MaxMovesInPosition])
            {
                var movesLength = position.GetMoves(moves);
                Console.WriteLine($"{movesLength} moves");
                for (int i = 0; i < movesLength; i++)
                {
                    var move = *(moves + i);
                    Console.WriteLine(move.MoveStruct);
                    Console.WriteLine(new Position(move.Position, position.OnMove.GetOpposite()).ToString());
                }
                Assert.AreEqual(16 + (4 * 2) + 5, movesLength);
            }
        }

        [TestMethod]
        public unsafe void GenerateStartingPositionGetMovesNoCheckValidation()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());
            Console.WriteLine();

            fixed (MoveWithPieceStruct* moves = stackalloc MoveWithPieceStruct[MovesEnumerators.MaxMovesInPosition])
            {
                var movesLength = position.GetMovesNoCheckValidation(moves);
                for (int i = 0; i < movesLength; i++)
                {
                    var move = *(moves + i);
                    Console.WriteLine(move);
                }
                Assert.AreEqual(16 + (4 * 2) + 5, movesLength);
            }
        }

        [TestMethod]
        public void PromotionTestWhite()
        {
            var position = Position.GetEmptyWithKings();
            position.Set(Colours.White, Pieces.Pawn, "a7");

            Console.WriteLine(position.ToString());
            var move = new MoveStruct(new Coordinates("a7").SingleCoordinate, new Coordinates("a8").SingleCoordinate);
            Console.WriteLine(move);

            position.Move(move);
            Console.WriteLine(position.ToString());
        }

        [TestMethod]
        public void PromotionTestBlack()
        {
            var position = Position.GetEmptyWithKings();
            position.Set(Colours.Black, Pieces.Pawn, "b2");

            Console.WriteLine(position.ToString());
            var move = new MoveStruct(new Coordinates("b2").SingleCoordinate, new Coordinates("b1").SingleCoordinate);
            Console.WriteLine(move);

            position.Move(move);
            Console.WriteLine(position.ToString());
        }

        [TestMethod]
        public void KingFirstMove()
        {
            var position = Position.GetEmptyWithKingsNotMoved();

            Console.WriteLine(position.ToString());
            var move = new MoveStruct(new Coordinates("e1").SingleCoordinate, new Coordinates("e2").SingleCoordinate);
            position.Move(move);

            Console.WriteLine(position.ToString());

            var arr = position.ToArray();
            Assert.IsNotNull(arr);
            Assert.AreEqual(1, arr.Count(x => x.Piece == Pieces.KingMoved));
        }

        [TestMethod]
        public void QueenFirstMove()
        {
            var position = Position.GetEmptyWithKings();
            position.Set(Colours.Black, Pieces.Queen, "d1");
            position.OnMove = Colours.Black;

            Console.WriteLine(position.ToString());
            var move = new MoveStruct(new Coordinates("d1").SingleCoordinate, new Coordinates("e2").SingleCoordinate);
            position.Move(move);

            Console.WriteLine(position.ToString());
            var arr = position.ToArray();
            Assert.IsNotNull(arr);
            Assert.AreEqual(1, arr.Count(x => x.Piece == Pieces.QueenMoved));
        }
    }
}
