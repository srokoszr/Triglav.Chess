using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.KnownPieceLocations;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.Engine.Tests.PopularFormats
{
    [TestClass]
    public class FEN_PositionNotationTests
    {
        [TestMethod]
        public void StartingPosition()
        {
            var position = Position.GetStartingPosition();

            var fen = position.FEN_Notation;

            Assert.AreEqual("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w QKkq - 0 1", fen);
        }

        [TestMethod]
        public void StartingPosition_E4()
        {
            var position = Position.GetStartingPosition();

            position.Move("e2-e4");

            var fen = position.FEN_Notation;

            Assert.AreEqual("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b QKkq - 0 1", fen);
        }

        [TestMethod]
        public void StartingPosition_E4_C5()
        {
            var position = Position.GetStartingPosition();

            position.Move("e2-e4");
            position.Move("c7-c5");

            var fen = position.FEN_Notation;

            Assert.AreEqual("rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w QKkq - 0 2", fen);
        }

        [TestMethod]
        public void StartingPosition_E4_C5_NF3()
        {
            var position = Position.GetStartingPosition();

            position.Move("e2-e4");
            position.Move("c7-c5");
            position.Move("g1-f3");

            var fen = position.FEN_Notation;

            Assert.AreEqual("rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b QKkq - 1 2", fen);
        }

        [TestMethod]
        public void EmptyWithKings()
        {
            var position = Position.GetEmptyWithKings();

            var fen = position.FEN_Notation;

            Assert.AreEqual("4k3/8/8/8/8/8/8/4K3 w - - 0 1", fen);
        }

        [TestMethod]
        public void EmptyWithKingsAndQueens()
        {
            var position = Position.GetEmptyWithKings();
            position.Add(Colours.Black, Pieces.QueenMoved, KnownQueenPositions.BlackQueenStartingPosition);
            position.Add(Colours.White, Pieces.QueenMoved, KnownQueenPositions.WhiteQueenStartingPosition);

            var fen = position.FEN_Notation;

            Assert.AreEqual("3qk3/8/8/8/8/8/8/3QK3 w - - 0 1", fen);
        }
    }
}
