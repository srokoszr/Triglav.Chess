using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.Model.Structs
{
    public readonly struct PieceDataExtended
    {
        public readonly Colours Colour;
        public readonly Pieces Piece;
        public readonly Coordinates Location;

        public PieceDataExtended(Colours colour, Pieces piece, Coordinates location)
        {
            this.Colour = colour;
            this.Piece = piece;
            this.Location = location;
        }
    }
}
