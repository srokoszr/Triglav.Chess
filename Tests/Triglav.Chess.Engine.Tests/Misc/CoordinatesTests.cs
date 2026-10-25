using Triglav.Chess.Model.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Triglav.Chess.Engine.Tests.Misc
{
    [TestClass]
    public class CoordinatesTests
    {
        [TestMethod]
        public void CheckAll()
        {
            for (byte i = 0; i < 64; i++)
            {
                var coordinate = new Coordinates(i);
                Assert.AreEqual(i, coordinate.SingleCoordinate);
            }
        }

        [DataRow("a8", 0, 0)]
        [DataRow("h8", 0, 7)]
        [DataRow("a1", 7, 0)]
        [DataRow("h1", 7, 7)]
        [TestMethod]
        public void CheckCornersHumanVsCartessian(string humanReadable, int row, int column)
        {
            var coordinate = new Coordinates((byte)row, (byte)column);
            Assert.AreEqual(humanReadable, coordinate.HumanReadableCoordinates);
        }

        [DataRow("a8", 0, 0)]
        [DataRow("h8", 0, 7)]
        [DataRow("a1", 7, 0)]
        [DataRow("h1", 7, 7)]
        [TestMethod]
        public void CheckCornersCartessianVsHuman(string humanReadable, int row, int column)
        {
            var coordinate = new Coordinates(humanReadable);
            Assert.AreEqual(row, coordinate.Row);
            Assert.AreEqual(column, coordinate.Column);
        }

        [DataRow("a8", 0)]
        [DataRow("h8", 7)]
        [DataRow("a1", 56)]
        [DataRow("h1", 63)]
        [TestMethod]
        public void CheckCornersHumanVsSingleCoordinate(string humanReadable, int singleCooridinate)
        {
            var coordinate = new Coordinates((byte)singleCooridinate);
            Assert.AreEqual(humanReadable, coordinate.HumanReadableCoordinates);
        }

        [DataRow("a8", 0)]
        [DataRow("h8", 7)]
        [DataRow("a1", 56)]
        [DataRow("h1", 63)]
        [TestMethod]
        public void CheckCornersSingleCoordinateVsHuman(string humanReadable, int singleCooridinate)
        {
            var coordinate = new Coordinates(humanReadable);
            Assert.AreEqual(singleCooridinate, coordinate.SingleCoordinate);
        }

        [DataRow(0, 0, 0)]
        [DataRow(7, 0, 7)]
        [DataRow(56, 7, 0)]
        [DataRow(63, 7, 7)]
        [TestMethod]
        public void CheckCornersSingleCoordinateVsCartessian(int singleCooridinate, int row, int column)
        {
            var coordinate = new Coordinates((byte)row, (byte)column);
            Assert.AreEqual(singleCooridinate, coordinate.SingleCoordinate);
        }

        [DataRow(0, 0, 0)]
        [DataRow(7, 0, 7)]
        [DataRow(56, 7, 0)]
        [DataRow(63, 7, 7)]
        [TestMethod]
        public void CheckCornersCartessianVsSingleCoordinate(int singleCooridinate, int row, int column)
        {
            var coordinate = new Coordinates((byte)singleCooridinate);
            Assert.AreEqual(row, coordinate.Row);
            Assert.AreEqual(column, coordinate.Column);
        }
    }
}
