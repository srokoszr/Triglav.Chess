using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Triglav.Chess.Model.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 8 * 5)]
    public readonly struct PositionKey : IComparable<PositionKey>
    {
        [FieldOffset(0)]
        public readonly ulong _takenFields;

        [FieldOffset(8)]
        public readonly ulong _pieceColours;

        [FieldOffset(16)]
        public readonly ulong _pieces_0;

        [FieldOffset(24)]
        public readonly ulong _pieces_1;

        [FieldOffset(32)]
        public readonly ulong _pieces_2;

        public PositionKey(ulong pieces_0, ulong pieces_1, ulong pieces_2, ulong takenFields, ulong pieceColours)
        {
            this._pieces_0 = pieces_0;
            this._pieces_1 = pieces_1;
            this._pieces_2 = pieces_2;
            this._takenFields = takenFields;
            this._pieceColours = pieceColours;
        }

        public int CompareTo(PositionKey other)
        {
            int c = this._takenFields.CompareTo(other._takenFields);
            if (c != 0) return c;

            c = this._pieceColours.CompareTo(other._pieceColours);
            if (c != 0) return c;

            c = this._pieces_0.CompareTo(other._pieces_0);
            if (c != 0) return c;

            c = this._pieces_1.CompareTo(other._pieces_1);
            if (c != 0) return c;

            c = this._pieces_2.CompareTo(other._pieces_2);
            if (c != 0) return c;

            return 0;
        }

        public override int GetHashCode() => (int)(this._takenFields ^ (this._takenFields >> 32));
    }
}
