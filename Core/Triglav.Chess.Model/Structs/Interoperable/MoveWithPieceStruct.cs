using Triglav.Chess.Model.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public readonly struct MoveWithPieceStruct
    {
        [FieldOffset(0)]
        public readonly MoveStruct Move;

        [FieldOffset(8)]
        public readonly Pieces Piece;

        public MoveWithPieceStruct(MoveStruct move, byte piece)
        {
            this.Move = move;
            this.Piece = (Pieces)piece;
        }

        public MoveWithPieceStruct(MoveStruct move, Pieces piece)
        {
            this.Move = move;
            this.Piece = piece;
        }

        public override string ToString()
        {
            return $"{(Pieces)this.Piece} - {this.Move}";
        }
    }
}
