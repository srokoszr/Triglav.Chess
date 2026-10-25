using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.Model.Structs
{
    public readonly struct FieldData
    {
        public readonly bool IsTaken;
        public readonly Colours PieceColour;
        public readonly Pieces Piece;

        public FieldData(bool isTaken, Colours pieceColour, Pieces piece)
        {
            this.IsTaken = isTaken;
            this.PieceColour = pieceColour;
            this.Piece = piece;
        }
    }
}
