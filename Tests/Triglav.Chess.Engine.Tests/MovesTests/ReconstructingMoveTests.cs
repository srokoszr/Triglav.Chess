using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Extensions;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Tests.MovesTests
{
    [TestClass]
    public class ReconstructingMoveTests
    {
        [TestMethod]
        public void StartingPosition_PositionDataFromKey()
        {
            var position = Position.GetStartingPosition();

            var key = position.PositionKey;

            var reconstructed = Position.GetPositionData(key, position.OnMove);

            Assert.AreEqual(position.State, reconstructed);

            var moves = position.GetMoves();

            foreach (var move in moves)
            {
                key = new PositionKey(move.Position.PieceColours, move.Position.Pieces_0, move.Position.Pieces_1, move.Position.Pieces_2);

                reconstructed = Position.GetPositionData(key, position.OnMove.GetOpposite());

                Assert.AreEqual(move.Position, reconstructed);
            }
        }

        [TestMethod]
        public void StartingPosition_E4D4E5D5_PositionDataFromKey()
        {
            var position = Position.GetStartingPosition();
            position.Move("e2-e4");
            position.Move("e7-e5");
            position.Move("d2-d4");
            position.Move("d7-d5");

            var key = position.PositionKey;

            var reconstructed = Position.GetPositionData(key, position.OnMove);

            Assert.AreEqual(position.State, reconstructed);

            var moves = position.GetMoves();

            foreach (var move in moves)
            {
                key = new PositionKey(move.Position.PieceColours, move.Position.Pieces_0, move.Position.Pieces_1, move.Position.Pieces_2);

                reconstructed = Position.GetPositionData(key, position.OnMove.GetOpposite());

                Assert.AreEqual(move.Position, reconstructed);
            }
        }

        [TestMethod]
        public void StartingPosition_E4D4E5D5A4_PositionDataFromKey()
        {
            var position = Position.GetStartingPosition();
            position.Move("e2-e4");
            position.Move("e7-e5");
            position.Move("d2-d4");
            position.Move("d7-d5");
            position.Move("a2-a4");

            var key = position.PositionKey;

            var reconstructed = Position.GetPositionData(key, position.OnMove);

            Assert.AreEqual(position.State, reconstructed);

            var moves = position.GetMoves();

            foreach (var move in moves)
            {
                key = new PositionKey(move.Position.PieceColours, move.Position.Pieces_0, move.Position.Pieces_1, move.Position.Pieces_2);

                reconstructed = Position.GetPositionData(key, position.OnMove.GetOpposite());

                Assert.AreEqual(move.Position, reconstructed);
            }
        }

        [TestMethod]
        public void StartingPosition_GetMove()
        {
            var position = Position.GetStartingPosition();

            var moves = position.GetMoves();

            foreach (var move in moves)
            {
                var key = new PositionKey(move.Position.PieceColours, move.Position.Pieces_0, move.Position.Pieces_1, move.Position.Pieces_2);

                var reconstructed = Position.ReconstructMove(position.PositionKey, key);

                Assert.AreEqual(move.MoveStruct, reconstructed.Move);
            }
        }

        [TestMethod]
        public void StartingPosition_E4D4E5D5_GetMove()
        {
            var position = Position.GetStartingPosition();
            position.Move("e2-e4");
            position.Move("e7-e5");
            position.Move("d2-d4");
            position.Move("d7-d5");

            var moves = position.GetMoves();

            foreach (var move in moves)
            {
                var key = new PositionKey(move.Position.PieceColours, move.Position.Pieces_0, move.Position.Pieces_1, move.Position.Pieces_2);

                var reconstructed = Position.ReconstructMove(position.PositionKey, key);

                Assert.AreEqual(move.MoveStruct, reconstructed.Move);
            }
        }

        [TestMethod]
        public void StartingPosition_E4D4E5D5A4_GetMove()
        {
            var position = Position.GetStartingPosition();
            position.Move("e2-e4");
            position.Move("e7-e5");
            position.Move("d2-d4");
            position.Move("d7-d5");
            position.Move("a2-a4");

            var moves = position.GetMoves();

            foreach (var move in moves)
            {
                var key = new PositionKey(move.Position.PieceColours, move.Position.Pieces_0, move.Position.Pieces_1, move.Position.Pieces_2);

                var reconstructed = Position.ReconstructMove(position.PositionKey, key);

                Assert.AreEqual(move.MoveStruct, reconstructed.Move);
            }
        }
    }
}
