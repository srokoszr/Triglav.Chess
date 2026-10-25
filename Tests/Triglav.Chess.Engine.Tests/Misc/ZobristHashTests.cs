using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Extensions;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Tests.Misc
{
    [TestClass]
    public class ZobristHashTests
    {
        [TestMethod]
        public void AssertZobristTableUniqueness()
        {
            var table = ZobristTable.Table;
            var hashes = new SortedSet<ulong>();
            for (int i = 0; i < 16; i++)
            {
                for(int j = 0; j < 64; j++)
                {
                    hashes.Add(table[i, j]);
                }
            }

            Assert.AreEqual(ZobristTable.TABLE_SIZE_N, hashes.Count);
        }

        //[TestMethod]
        public void AssertZobristTableUniquenessXOR()
        {
            var table = ZobristTable.Table;
            var hashes = new SortedSet<ulong>();
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    hashes.Add(table[i, j] & 0x0000FFFF);
                }
            }

            Assert.AreEqual(ZobristTable.TABLE_SIZE_N, hashes.Count);
        }

        [TestMethod]
        public unsafe void StartingPositionGetMovesAndCheckHashes()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());
            Console.WriteLine();

            var startingHash = position.State.Hash;
            var hashes = new SortedSet<ulong>();

            fixed (MoveSimulation* moves = stackalloc MoveSimulation[MovesEnumerators.MaxMovesInPosition])
            {
                var movesLength = position.GetMoves(moves);
                
                for (int i = 0; i < movesLength; i++)
                {
                    var simulation = moves[i];
                    Assert.AreNotEqual(startingHash, simulation.Position.Hash);

                    var unique = hashes.Add(simulation.Position.Hash);
                    //Assert.IsTrue(unique);
                }
                Assert.AreEqual(movesLength, hashes.Count);
            }
        }

        [TestMethod]
        public unsafe void StartingPositionGetMovesAndRecomputeHashes()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());
            Console.WriteLine();

            var startingHash = position.State.Hash;
            var recomputedHash = ZobristTable.RecomputeHash(position);
            Assert.AreEqual(startingHash, recomputedHash);

            fixed (MoveSimulation* moves = stackalloc MoveSimulation[MovesEnumerators.MaxMovesInPosition])
            {
                var movesLength = position.GetMoves(moves);

                for (int i = 0; i < movesLength; i++)
                {
                    var simulation = moves[i];
                    Assert.AreNotEqual(startingHash, simulation.Position.Hash);

                    recomputedHash = ZobristTable.RecomputeHash(new Position(simulation.Position, position.OnMove.GetOpposite()));

                    Assert.AreEqual(simulation.Position.Hash, recomputedHash);
                }
            }
        }
    }
}
