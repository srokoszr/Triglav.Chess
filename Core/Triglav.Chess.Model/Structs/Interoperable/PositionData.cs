using System;
using System.Runtime.InteropServices;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    [StructLayout(LayoutKind.Explicit, Size = 56)]
    public readonly struct PositionData : IComparable<PositionData>
    {
        [FieldOffset(0)]
        public readonly ulong Hash;

        [FieldOffset(8)]
        public readonly ulong TakenFields;

        [FieldOffset(16)]
        public readonly ulong PieceColours;

        [FieldOffset(24)]
        public readonly ulong Pieces_0;

        [FieldOffset(32)]
        public readonly ulong Pieces_1;

        [FieldOffset(40)]
        public readonly ulong Pieces_2;

        [FieldOffset(48)]
        public readonly int WhiteKingLocation;

        [FieldOffset(52)]
        public readonly int BlackKingLocation;

        public ulong WhitePieces => this.PieceColours ^ this.TakenFields;

        public PositionData(ulong hash, ulong takenFields, ulong pieceColours, ulong pieces_0, ulong pieces_1, ulong pieces_2, int whiteKingLocation, int blackKingLocation)
        {
            this.Hash = hash;
            this.TakenFields = takenFields;
            this.PieceColours = pieceColours;
            this.Pieces_0 = pieces_0;
            this.Pieces_1 = pieces_1;
            this.Pieces_2 = pieces_2;
            this.WhiteKingLocation = whiteKingLocation;
            this.BlackKingLocation = blackKingLocation;
        }

        public int CompareTo(PositionData other)
        {
            int c = this.Hash.CompareTo(other.Hash);
            if (c != 0) return c;

            c = this.TakenFields.CompareTo(other.TakenFields);
            if (c != 0) return c;

            c = this.PieceColours.CompareTo(other.PieceColours);
            if (c != 0) return c;

            c = this.Pieces_0.CompareTo(other.Pieces_0);
            if (c != 0) return c;

            c = this.Pieces_1.CompareTo(other.Pieces_1);
            if (c != 0) return c;

            c = this.Pieces_2.CompareTo(other.Pieces_2);
            if (c != 0) return c;

            return 0;
        }

        public override int GetHashCode() => (int)(this.TakenFields & 0x0000FFFF ^ this.TakenFields >> 8 * 4);
    }
}
