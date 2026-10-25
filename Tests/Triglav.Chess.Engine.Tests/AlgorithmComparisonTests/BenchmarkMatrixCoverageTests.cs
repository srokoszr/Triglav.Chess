using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Triglav.Chess.Engine.Comparison;

namespace Triglav.Chess.Engine.Tests.AlgorithmComparisonTests
{
    [TestClass]
    public class BenchmarkMatrixCoverageTests
    {
        [TestMethod]
        public void MatrixKeepsBothStartingSidesButDoesNotMirrorOtherSyntheticPositions()
        {
            AlgorithmBenchmarkCase[] scenarios = [.. AlgorithmBenchmarkCase.All];
            Assert.HasCount(38, scenarios);
            Assert.AreEqual(scenarios.Length, scenarios.Select(static scenario => scenario.ToString()).Distinct().Count());
            var roots = scenarios.GroupBy(static scenario => (scenario.Kind, scenario.Black, scenario.GamePly)).ToArray();
            Assert.HasCount(8, roots);
            CollectionAssert.AreEquivalent(new[] { false, true }, scenarios
                .Where(static scenario => scenario.Kind == "Start")
                .Select(static scenario => scenario.Black)
                .Distinct()
                .ToArray());
            Assert.IsFalse(scenarios.Any(static scenario => scenario.GamePly == 0
                && scenario.Kind != "Start" && scenario.Black));
            foreach (var root in roots)
            {
                Int32[] expected = root.Key.Kind == "Start" ? [1, 2, 3, 4] : [1, 2, 3, 4, 5];
                CollectionAssert.AreEquivalent(expected, root.Select(static scenario => scenario.Depth).ToArray());
                Assert.IsTrue(root.All(static scenario => scenario.Plies == scenario.Depth * 2));
            }
        }
    }
}
