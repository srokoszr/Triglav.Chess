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
    public class BasicTreeWithMinMaxWithCopyingTests
    {
        [TestMethod]
        public unsafe void StartingPosition_MaxDepth1()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());
            using var postitionStack = PositionStackWrapper.New();

            var wrapper = PositionTreeWrapper.BuildWithMinMaxWithCopying(position.State, Colours.White, 1, postitionStack);

            Assert.IsNotNull(wrapper);
            Assert.AreNotEqual(new PositionTree(), wrapper.NativeTree);

            var topLine = wrapper.TopLine;
            Assert.IsNotNull(topLine);
            Assert.HasCount(1 * 2, topLine);

            Assert.AreEqual(0, postitionStack.Count);

            wrapper?.Dispose();
        }

        [TestMethod]
        public unsafe void StartingPosition_MaxDepth2()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());
            using var postitionStack = PositionStackWrapper.New();

            var wrapper = PositionTreeWrapper.BuildWithMinMaxWithCopying(position.State, Colours.White, 2, postitionStack);

            Assert.IsNotNull(wrapper);
            Assert.AreNotEqual(new PositionTree(), wrapper.NativeTree);

            var topLine = wrapper.TopLine;
            Assert.IsNotNull(topLine);
            Assert.HasCount(2 * 2, topLine);

            Assert.AreEqual(0, postitionStack.Count);

            wrapper?.Dispose();
        }

        [TestMethod]
        public unsafe void StartingPosition_MaxDepth3()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());
            using var postitionStack = PositionStackWrapper.New();

            var wrapper = PositionTreeWrapper.BuildWithMinMaxWithCopying(position.State, Colours.White, 3, postitionStack);

            Assert.IsNotNull(wrapper);
            Assert.AreNotEqual(new PositionTree(), wrapper.NativeTree);

            var topLine = wrapper.TopLine;
            Assert.IsNotNull(topLine);
            Assert.HasCount(3 * 2, topLine);

            Assert.AreEqual(0, postitionStack.Count);

            wrapper?.Dispose();
        }

        [TestMethod]
        public unsafe void StartingPosition_MaxDepth4()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());
            using var postitionStack = PositionStackWrapper.New();

            var wrapper = PositionTreeWrapper.BuildWithMinMaxWithCopying(position.State, Colours.White, 4, postitionStack);

            Assert.IsNotNull(wrapper);
            Assert.AreNotEqual(new PositionTree(), wrapper.NativeTree);

            var topLine = wrapper.TopLine;
            Assert.IsNotNull(topLine);
            Assert.HasCount(4 * 2, topLine);

            var root = wrapper.NativeTree.Root;
            Assert.IsLessThanOrEqualTo(root->AvailableMovesCount, root->AnalysedMovesCount);
            Assert.IsLessThanOrEqualTo(root->LegalMovesCount, root->AvailableMovesCount);
            Assert.IsLessThan(root->LegalMovesCount, root->AvailableMovesCount);

            Assert.AreEqual(0, postitionStack.Count);

            wrapper?.Dispose();
        }

        [Ignore]//long running
        [TestMethod]
        public unsafe void StartingPosition_MaxDepth5()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());

            var wrapper = PositionTreeWrapper.BuildWithMinMaxWithCopying(position.State, Colours.White, 5);

            Assert.IsNotNull(wrapper);
            Assert.AreNotEqual(new PositionTree(), wrapper.NativeTree);

            var topLine = wrapper.TopLine;
            Assert.IsNotNull(topLine);
            Assert.HasCount(5 * 2, topLine);

            wrapper?.Dispose();
        }

        [Ignore]//long running
        [TestMethod]
        public unsafe void StartingPosition_MaxDepth6()
        {
            var position = Position.GetStartingPosition();
            Console.WriteLine(position.ToString());

            var wrapper = PositionTreeWrapper.BuildWithMinMaxWithCopying(position.State, Colours.White, 6);

            Assert.IsNotNull(wrapper);
            Assert.AreNotEqual(new PositionTree(), wrapper.NativeTree);

            wrapper?.Dispose();
        }
    }
}
