using System;
using System.Runtime.InteropServices;

namespace Triglav.Chess.Model.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = Size)]
    public unsafe struct PieceArray
    {
        public const int Size = 64;

        [FieldOffset(0)]
        public unsafe fixed byte Pieces[Size];
    }
}
