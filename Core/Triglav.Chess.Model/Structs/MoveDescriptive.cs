using System.Text;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Model.Structs
{
    public sealed class MoveDescriptive
    {
        public MoveStruct Move { get; private set; }

        public Colours Colour { get; private set; }

        public Pieces Piece { get; private set; }

        public bool IsCapture { get; private set; }

        public bool IsCheck {  get; private set; }

        public bool IsMate { get; private set; }

        public bool IsPromotion { get; private set; }

        public string DisambiguitySuffix { get; private set; }

        public string AlgebraicNotation => this.ToString();

        public MoveDescriptive(MoveStruct move, Colours colour, Pieces piece, bool isCapture, bool isCheck, bool isMate, bool isPromotion, string disambiguitySuffix)
        {
            this.Move = move;
            this.Colour = colour;
            this.Piece = piece;
            this.IsCapture = isCapture;
            this.IsCheck = isCheck;
            this.IsMate = isMate;
            this.IsPromotion = isPromotion;
            this.DisambiguitySuffix = disambiguitySuffix;
        }

        public override System.String ToString()
        {
            var result = new StringBuilder();

            string pieceLetter = this.Piece switch
            {
                Pieces.Pawn => string.Empty,
                Pieces.Rook => "R",
                Pieces.Knight => "N",
                Pieces.Bishop => "B",
                Pieces.Queen => this.IsPromotion ? string.Empty : "Q",
                Pieces.QueenMoved => "Q",
                Pieces.King => "K",
                Pieces.KingMoved => "K",
                _ => string.Empty,
            };
            result.Append(pieceLetter);

            result.Append(this.DisambiguitySuffix);

            if(this.IsCapture)
            {
                result.Append("x");
            }

            result.Append(new Coordinates((byte)this.Move.To).HumanReadableCoordinates);

            if(this.IsPromotion)
            {
                result.Append("=Q");
            }

            if(this.IsCheck)
            {
                result.Append(this.IsMate ? "#" : "+");
            }

            return result.ToString();
        }
    }
}
