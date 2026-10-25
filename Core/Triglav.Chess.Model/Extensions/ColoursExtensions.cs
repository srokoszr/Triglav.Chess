using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.Model.Extensions
{
    public static class ColoursExtensions
    {
        public const int White = (int)Colours.White;
        public const int Black = (int)Colours.Black;

        public static Colours GetOpposite(this Colours colour) => colour == Colours.White ? Colours.Black : Colours.White;

        public static Colours GetOppositeBitwise(this Colours colour) => (Colours)((byte)colour ^ Black);

        public static int GetOppositeBitwiseReturnInt(this Colours colour) => (byte)colour ^ Black;
    }
}
