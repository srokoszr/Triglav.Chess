using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public readonly unsafe struct PositionTreeNodeMove
    {
        [FieldOffset(0)]
        public readonly PositionTreeNode* ResultingPositionNode;

        [FieldOffset(8)]
        public readonly MoveFlags MoveFlags;

        [FieldOffset(12)]
        public readonly MoveWithPieceStructPacked MoveStruct;
    }
}
