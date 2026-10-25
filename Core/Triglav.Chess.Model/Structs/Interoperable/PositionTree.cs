using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public readonly unsafe struct PositionTree
    {
        [FieldOffset(0)]
        public readonly PositionTreeNode* Root;

        [FieldOffset(8)]
        public readonly PositionTreeNodeCache* NodeCache;

        [FieldOffset(16)]
        public readonly HashBucketPool* BucketPool;

        [FieldOffset(24)]
        public readonly TreeNodeMovePool* NodesPool;
    }
}
