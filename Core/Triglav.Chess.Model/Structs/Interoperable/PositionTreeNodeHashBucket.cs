using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    [StructLayout(LayoutKind.Explicit, Size = 56)]
    public unsafe struct PositionTreeNodeHashBucket
    {
        [FieldOffset(0)]
        public fixed byte Item[56];

        [FieldOffset(48)]
        public readonly PositionTreeNodeHashBucket* Next;
    }
}
