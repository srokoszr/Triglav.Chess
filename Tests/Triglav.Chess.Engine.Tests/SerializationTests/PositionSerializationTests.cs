using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Triglav.Chess.Engine.Core;

namespace Triglav.Chess.Engine.Tests.SerializationTests
{
    [TestClass]
    public class PositionSerializationTests
    {
        [TestMethod]
        public void EmptyWithKingsRoundtrip()
        {
            var original = Position.GetEmptyWithKings();
            using(var stream = new MemoryStream())
            {
                original.Save(stream);
                stream.Position = 0;
                var deserialized = Position.Deserialize(stream);

                Assert.AreEqual(original.State, deserialized.State);
                Assert.AreEqual(original.OnMove, deserialized.OnMove);
            }
        }

        [TestMethod]
        public void EmptyWithKingsRoundtrip_1()
        {
            var original = Position.GetEmptyWithKings();
            using (var stream = new MemoryStream())
            {
                original.Save(stream);
                original.OnMove = Chess.Model.Enumerations.Colours.Black;
                stream.Position = 0;
                var deserialized = Position.Deserialize(stream);

                Assert.AreEqual(original.State, deserialized.State);
                Assert.AreNotEqual(original.OnMove, deserialized.OnMove);
            }
        }

        [TestMethod]
        public void EmptyWithKingsRoundtrip_2()
        {
            var original = Position.GetEmptyWithKings();
            original.OnMove = Chess.Model.Enumerations.Colours.Black;
            using (var stream = new MemoryStream())
            {
                original.Save(stream);
                original.OnMove = Chess.Model.Enumerations.Colours.White;
                stream.Position = 0;
                var deserialized = Position.Deserialize(stream);

                Assert.AreEqual(original.State, deserialized.State);
                Assert.AreNotEqual(original.OnMove, deserialized.OnMove);
            }
        }

        [TestMethod]
        public void EmptyWithKingsRoundtrip_3()
        {
            var original = Position.GetEmptyWithKings();
            original.OnMove = Chess.Model.Enumerations.Colours.Black;
            using (var stream = new MemoryStream())
            {
                original.Save(stream);
                stream.Position = 0;
                var deserialized = Position.Deserialize(stream);

                Assert.AreEqual(original.State, deserialized.State);
                Assert.AreEqual(original.OnMove, deserialized.OnMove);
            }
        }

        [TestMethod]
        public void StartingPositionRoundtrip()
        {
            var original = Position.GetStartingPosition();
            using (var stream = new MemoryStream())
            {
                original.Save(stream);
                stream.Position = 0;
                var deserialized = Position.Deserialize(stream);

                Assert.AreEqual(original.State, deserialized.State);
                Assert.AreEqual(original.OnMove, deserialized.OnMove);
            }
        }
    }
}
