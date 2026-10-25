using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Tests.PositionTreeTests
{
    [TestClass]
    public class SimpleTests
    {
        [TestMethod]
        public unsafe void StartingPosition_SmokeTest()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());

            var wrapper = PositionTreeWrapper.Build(position.State, Colours.White, 1);

            Assert.IsNotNull(wrapper);
            Assert.AreNotEqual(new PositionTree(), wrapper.NativeTree);

            Assert.AreEqual(29, wrapper.NativeTree.Root->LegalMovesCount);
            Assert.AreEqual(29, wrapper.NativeTree.Root->AvailableMovesCount);
            Assert.AreEqual(29, wrapper.NativeTree.Root->AnalysedMovesCount);

            for (int i = 0; i < wrapper.NativeTree.Root->AnalysedMovesCount; i++)
            {
                Assert.AreNotEqual(new PositionKey(), wrapper.NativeTree.Root->Children[i]->Position);
            }

            wrapper?.Dispose();
        }

        [TestMethod]
        public unsafe void StartingPosition_MaxDepth2()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());

            var wrapper = PositionTreeWrapper.Build(position.State, Colours.White, 2);

            Assert.IsNotNull(wrapper);
            Assert.AreNotEqual(new PositionTree(), wrapper.NativeTree);

            wrapper?.Dispose();
        }

        [TestMethod]
        public unsafe void StartingPosition_MaxDepth3()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());

            var wrapper = PositionTreeWrapper.Build(position.State, Colours.White, 3);

            Assert.IsNotNull(wrapper);
            Assert.AreNotEqual(new PositionTree(), wrapper.NativeTree);

            wrapper?.Dispose();
        }

        [TestMethod]
        public unsafe void StartingPosition_MaxDepth4()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());

            var wrapper = PositionTreeWrapper.Build(position.State, Colours.White, 4);

            Assert.IsNotNull(wrapper);
            Assert.AreNotEqual(new PositionTree(), wrapper.NativeTree);

            wrapper?.Dispose();
        }

        [TestMethod]
        public unsafe void StartingPosition_MaxDepth5()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());

            var wrapper = PositionTreeWrapper.Build(position.State, Colours.White, 5);

            Assert.IsNotNull(wrapper);
            Assert.AreNotEqual(new PositionTree(), wrapper.NativeTree);

            wrapper?.Dispose();
        }

        [TestMethod]
        public unsafe void StartingPosition_MaxDepth6()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());

            var wrapper = PositionTreeWrapper.Build(position.State, Colours.White, 6);

            Assert.IsNotNull(wrapper);
            Assert.AreNotEqual(new PositionTree(), wrapper.NativeTree);

            wrapper?.Dispose();
        }

        //[Ignore]//long running
        [TestMethod]
        public unsafe void StartingPosition_MaxDepth7()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());

            var wrapper = PositionTreeWrapper.Build(position.State, Colours.White, 7);

            Assert.IsNotNull(wrapper);
            Assert.AreNotEqual(new PositionTree(), wrapper.NativeTree);

            wrapper?.Dispose();
        }

        [Ignore]//long running
        [TestMethod]
        public unsafe void StartingPosition_MaxDepth8()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());

            var wrapper = PositionTreeWrapper.Build(position.State, Colours.White, 1);
            for (int i = 0; i < wrapper.NativeTree.Root->AnalysedMovesCount; i++)
            {
                wrapper.Extend(wrapper.NativeTree.Root->Children[i], Colours.Black, 7);
            }

            Assert.IsNotNull(wrapper);
            Assert.AreNotEqual(new PositionTree(), wrapper.NativeTree);

            wrapper?.Dispose();
        }

        [Ignore]//long running
        [TestMethod]
        public unsafe void StartingPosition_MaxDepth8_NoLoop()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());

            var wrapper = PositionTreeWrapper.Build(position.State, Colours.White, 8);

            Assert.IsNotNull(wrapper);
            Assert.AreNotEqual(new PositionTree(), wrapper.NativeTree);

            wrapper?.Dispose();
        }
    }
}
