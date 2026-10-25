using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Tests.EvaluationStructTests
{
    [TestClass]
    public class EvaluationComparisonTests
    {
        [TestMethod]
        public void WhiteWinMateVsUnknown()
        {
            var left = new EvaluationWrapper(new Evaluation(0, EvaluationResult.WhiteWinMate, 1));
            var right = new EvaluationWrapper(new Evaluation(0, EvaluationResult.DrawOrUknown, 0));

            Assert.IsTrue(left > right);
            Assert.IsTrue(left >= right);
            Assert.IsTrue(right < left);
            Assert.IsTrue(right <= left);
        }

        [TestMethod]
        public void WhiteWinVsDraw()
        {
            var left = new EvaluationWrapper(new Evaluation(0, EvaluationResult.WhiteWinMate, 1));
            var right = new EvaluationWrapper(new Evaluation(0, EvaluationResult.DrawOrUknown, 1));

            Assert.IsTrue(left > right);
            Assert.IsTrue(left >= right);
            Assert.IsTrue(right < left);
            Assert.IsTrue(right <= left);
        }
        [TestMethod]
        public void WhiteWinNakedMateVsUnknown()
        {
            var left = new EvaluationWrapper(new Evaluation(0, EvaluationResult.WhiteWinNakedMate, 1));
            var right = new EvaluationWrapper(new Evaluation(0, EvaluationResult.DrawOrUknown, 0));

            Assert.IsTrue(left > right);
            Assert.IsTrue(left >= right);
            Assert.IsTrue(right < left);
            Assert.IsTrue(right <= left);
        }

        [TestMethod]
        public void WhiteWinNakedMateVsDraw()
        {
            var left = new EvaluationWrapper(new Evaluation(0, EvaluationResult.WhiteWinNakedMate, 1));
            var right = new EvaluationWrapper(new Evaluation(0, EvaluationResult.DrawOrUknown, 1));

            Assert.IsTrue(left > right);
            Assert.IsTrue(left >= right);
            Assert.IsTrue(right < left);
            Assert.IsTrue(right <= left);
        }
        [TestMethod]
        public void WhiteWinStalemateVsUnknown()
        {
            var left = new EvaluationWrapper(new Evaluation(0, EvaluationResult.WhiteWinStalemate, 1));
            var right = new EvaluationWrapper(new Evaluation(0, EvaluationResult.DrawOrUknown, 0));

            Assert.IsTrue(left > right);
            Assert.IsTrue(left >= right);
            Assert.IsTrue(right < left);
            Assert.IsTrue(right <= left);
        }

        [TestMethod]
        public void WhiteWinStalemateVsDraw()
        {
            var left = new EvaluationWrapper(new Evaluation(0, EvaluationResult.WhiteWinStalemate, 1));
            var right = new EvaluationWrapper(new Evaluation(0, EvaluationResult.DrawOrUknown, 1));

            Assert.IsTrue(left > right);
            Assert.IsTrue(left >= right);
            Assert.IsTrue(right < left);
            Assert.IsTrue(right <= left);
        }

        [TestMethod]
        public void WhiteWinMateVsStalemate()
        {
            var left = new EvaluationWrapper(new Evaluation(0, EvaluationResult.WhiteWinMate, 1));
            var right = new EvaluationWrapper(new Evaluation(0, EvaluationResult.WhiteWinStalemate, 1));

            Assert.IsTrue(left > right);
            Assert.IsTrue(left >= right);
            Assert.IsTrue(right < left);
            Assert.IsTrue(right <= left);
        }

        [TestMethod]
        public void DrawVsUknown()
        {
            var left = new EvaluationWrapper(new Evaluation(0, EvaluationResult.DrawOrUknown, 1));
            var right = new EvaluationWrapper(new Evaluation(0, EvaluationResult.DrawOrUknown, 0));

            Assert.IsTrue(left >= right);
            Assert.IsTrue(right <= left);
        }

        [TestMethod]
        public void WhiteWinVsBlackWin()
        {
            var left = new EvaluationWrapper(new Evaluation(0, EvaluationResult.WhiteWinMate, 0));
            var right = new EvaluationWrapper(new Evaluation(0, EvaluationResult.BlackWinMate, 0));

            Assert.IsTrue(left > right);
            Assert.IsTrue(left >= right);
            Assert.IsTrue(right < left);
            Assert.IsTrue(right <= left);
        }

        [TestMethod]
        public void WhiteWinMateVsWhiteWinMate()
        {
            var left = new EvaluationWrapper(new Evaluation(0, EvaluationResult.WhiteWinMate, 0));
            var right = new EvaluationWrapper(new Evaluation(0, EvaluationResult.WhiteWinMate, 0));

            Assert.IsTrue(left == right);
            Assert.IsTrue(left >= right);
            Assert.IsTrue(right <= left);
        }

        [TestMethod]
        public void WhiteWinIn1VsWhiteWinIn2()
        {
            var left = new EvaluationWrapper(new Evaluation(0, EvaluationResult.WhiteWinMate, 1));
            var right = new EvaluationWrapper(new Evaluation(0, EvaluationResult.WhiteWinMate, 2));

            Assert.IsTrue(left > right);
            Assert.IsTrue(left >= right);
            Assert.IsTrue(right < left);
            Assert.IsTrue(right <= left);
        }

        [TestMethod]
        public void WhiteWinStalemateIn1VsWhiteWinMateIn2()
        {
            var left = new EvaluationWrapper(new Evaluation(0, EvaluationResult.WhiteWinStalemate, 1));
            var right = new EvaluationWrapper(new Evaluation(0, EvaluationResult.WhiteWinMate, 2));

            Assert.IsTrue(left < right);
            Assert.IsTrue(left <= right);
            Assert.IsTrue(right > left);
            Assert.IsTrue(right >= left);
        }
    }
}
