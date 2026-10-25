using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public readonly unsafe struct TreeNodeMovePool
    {
        public const int POOL_BUFFER_SIZE = 0x00010000;

        [FieldOffset(0)]
        public readonly PositionTreeNodeMove*** Buffers;

        [FieldOffset(8)]
        public readonly PositionTreeNodeMove** LastBufferNextElementPointer;

        [FieldOffset(16)]
        public readonly int BuffersCount;

        [FieldOffset(20)]
        public readonly int CurrentBuffer;

        [FieldOffset(24)]
        public readonly int LastBufferNextElementOffset;

        [FieldOffset(28)]
        public readonly int ReturnedNodes;

        public long TotalItems => (this.CurrentBuffer * POOL_BUFFER_SIZE) + this.LastBufferNextElementOffset;

        public long AllocatedItems => this.BuffersCount * POOL_BUFFER_SIZE;

        public long AllocatedBytes => this.AllocatedItems * sizeof(PositionTreeNodeMove*);

        public double AllocatedKiloBytes => (double)this.AllocatedBytes / 1024;

        public double AllocatedMegaBytes => this.AllocatedKiloBytes / 1024;

        public double AllocatedGigaBytes => this.AllocatedMegaBytes / 1024;

        public double NodesUtilisation => (double)this.ReturnedNodes / (double)this.TotalItems;
    }
}
