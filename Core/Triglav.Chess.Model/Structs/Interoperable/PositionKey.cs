using System;
using System.IO;
using System.Runtime.InteropServices;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public readonly struct PositionKey : IComparable<PositionKey>
    {
        [FieldOffset(0)]
        public readonly ulong PieceColours;

        [FieldOffset(8)]
        public readonly ulong Pieces_0;

        [FieldOffset(16)]
        public readonly ulong Pieces_1;

        [FieldOffset(24)]
        public readonly ulong Pieces_2;

        public PositionKey(ulong pieceColours, ulong pieces_0, ulong pieces_1, ulong pieces_2)
        {
            this.PieceColours = pieceColours;
            this.Pieces_0 = pieces_0;
            this.Pieces_1 = pieces_1;
            this.Pieces_2 = pieces_2;
        }

        public int CompareTo(PositionKey other)
        {
            int c = this.PieceColours.CompareTo(other.PieceColours);
            if (c != 0) return c;

            c = this.Pieces_0.CompareTo(other.Pieces_0);
            if (c != 0) return c;

            c = this.Pieces_1.CompareTo(other.Pieces_1);
            if (c != 0) return c;

            c = this.Pieces_2.CompareTo(other.Pieces_2);
            if (c != 0) return c;

            return 0;
        }

        public override int GetHashCode() => (int)(this.PieceColours & 0x0000FFFF ^ this.PieceColours >> 8 * 4);
    }
}
