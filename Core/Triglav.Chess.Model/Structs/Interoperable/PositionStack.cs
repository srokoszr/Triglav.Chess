using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public readonly unsafe struct PositionStack
    {
        [FieldOffset(0)]
        public readonly PositionData* Array;

        [FieldOffset(8)]
        public readonly int Count;

        [FieldOffset(12)]
        public readonly int Capacity;
    }
}
