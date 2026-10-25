using Triglav.Chess.Engine.Generators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Triglav.Chess.Engine.Tests.Misc
{
    //[TestClass]
    public sealed class UpTo6EndgamesGeneratorTests
    {
        [TestMethod]
        public void LogToConsole()
        {
            foreach (var position in UpTo6EndgamesGenerator.Generate())
            {
                //Console.WriteLine(position.ToString());
            }
        }
    }
}
