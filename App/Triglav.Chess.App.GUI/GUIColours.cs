using System.Drawing;

namespace Triglav.Chess.App.GUI
{
    public static class GUIColours
    {
        public static readonly Color White = Color.White;
        public static readonly Color Black = Color.Black;

        public static readonly Color WhiteFieldColour = Color.FromArgb(250, 240, 240);
        public static readonly Color BlackFieldColour = Color.FromArgb(222, 170, 135);

        public static readonly Color LineColour = Color.FromArgb(177, 81, 17);
        public static readonly Color BackgroundColour = Color.FromArgb(159, 89, 43);

        public static readonly Color WhitePiecesColour = Color.FromKnownColor(KnownColor.White);
        public static readonly Color BlackPiecesColour = Color.FromKnownColor(KnownColor.Black);

        public static readonly Color LegalMoveColour = Color.FromArgb(140, 40, 180, 40);
        public static readonly Color KingCheckedColour = Color.FromArgb(160, 220, 80, 80);
        public static readonly Color LastMoveColour = Color.FromArgb(120, 180, 220, 120);
    }
}
