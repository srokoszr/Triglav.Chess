using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public readonly struct MinMaxResult
    {
        [FieldOffset(0)]
        public readonly Evaluation Evaluation;

        [FieldOffset(4)]
        public readonly MoveWithPieceStruct Move;
    }
}
