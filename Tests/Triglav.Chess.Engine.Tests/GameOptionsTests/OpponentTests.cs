using Microsoft.VisualStudio.TestTools.UnitTesting;
using Triglav.Chess.App.GUI.Model;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.Engine.Tests.GameOptionsTests
{
    [TestClass]
    public class OpponentTests
    {
        [TestMethod]
        [DataRow(Colours.White)]
        [DataRow(Colours.Black)]
        public void EngineGameKeepsSelectedColourHuman(Colours colour)
        {
            GameOptions options = new()
            {
                ColourToPlay = colour
            };
            Assert.AreEqual(colour == Colours.White, options.IsHuman(Colours.White));
            Assert.AreEqual(colour == Colours.Black, options.IsHuman(Colours.Black));
        }

        [TestMethod]
        [DataRow(Colours.White)]
        [DataRow(Colours.Black)]
        public void FriendGameAllowsBothColoursAndSwitchingBackRestoresEngine(Colours colour)
        {
            GameOptions options = new()
            {
                PlayWithFriend = true,
                ColourToPlay = colour
            };
            Assert.IsTrue(options.IsHuman(Colours.White));
            Assert.IsTrue(options.IsHuman(Colours.Black));
            options.PlayWithFriend = false;
            Assert.AreEqual(colour == Colours.White, options.IsHuman(Colours.White));
            Assert.AreEqual(colour == Colours.Black, options.IsHuman(Colours.Black));
        }
    }
}
