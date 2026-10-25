using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    [StructLayout(LayoutKind.Explicit, Size = 24)]
    public readonly struct GetMovesResult
    {
        [FieldOffset(0)]
        public readonly SetOf128Bits CheckingMoves;

        [FieldOffset(16)]
        public readonly int ResultCount;
    }
}
