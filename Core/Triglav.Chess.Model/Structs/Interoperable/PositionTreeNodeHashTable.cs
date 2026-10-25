using System;
using System.Runtime.InteropServices;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    [StructLayout(LayoutKind.Explicit, Size = 24)]
    public readonly unsafe struct PositionTreeNodeHashTable
    {
        //2^24 = 16 777 216
        public const int Size = 0x08000000;//0x01000000;

        [FieldOffset(0)]
        public readonly IntPtr Buckets;

        [FieldOffset(8)]
        public readonly int OperationsCount;

        [FieldOffset(12)]
        public readonly int UniqueItems;

        [FieldOffset(16)]
        public readonly int UsedBuckets;

        public float CacheReuseRatio => ((float)this.OperationsCount / (float)this.UniqueItems) - 1f;

        public float FillRatio => (float)this.UsedBuckets / (float)Size;

        public float ItemsPerBucket => (float)this.UniqueItems / (float)this.UsedBuckets;
    }
}