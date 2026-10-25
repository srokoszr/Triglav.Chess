using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    [StructLayout(LayoutKind.Explicit, Size = 3)]
    public readonly struct MoveWithPieceStructPacked
    {
        [FieldOffset(0)]
        public readonly byte From;

        [FieldOffset(1)]
        public readonly byte To;

        [FieldOffset(2)]
        public readonly byte Piece;
    }
}
