using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public readonly unsafe struct PositionTreeNodeCache
    {
        [FieldOffset(0)]
        public readonly PositionTreeNodeHashTable* Hashtable;
    }
}
