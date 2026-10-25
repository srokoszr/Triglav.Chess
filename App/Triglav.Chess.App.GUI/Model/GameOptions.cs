using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.App.GUI.Model
{
    public class GameOptions
    {
        public Colours ColourToPlay
        {
            get; set;
        }

        public AlgorithmDepthLevel Level { get; set; } = AlgorithmDepthLevel.Master;

        public bool HumanMode { get; set; } = false;

        public bool PlayWithFriend
        {
            get; set;
        }

        public bool IsHuman(Colours colour) => this.PlayWithFriend || this.ColourToPlay == colour;
    }
}
