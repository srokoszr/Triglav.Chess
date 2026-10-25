using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Triglav.Chess.Model.Structs
{
    //[StructLayout(LayoutKind.Explicit, Size = 3)]
    public readonly struct Coordinates : IComparable
    {
        private const byte maskLowerPart = 0x07;

        //[FieldOffset(0)]
        public readonly byte SingleCoordinate;

        //[FieldOffset(1)]
        public readonly byte Row;

        //[FieldOffset(2)]
        public readonly byte Column;

        public (byte Row, byte Column) CartessianCoordinates => (this.Row, this.Column);

        public string HumanReadableCoordinates => new string(new char[] { (char)('a' + this.Column), (char)('1' + (7 - this.Row)) });

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte GetSingleCoordinate(byte row, byte column) => (byte)((row << 3) | column);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte GetSingleCoordinate(int row, int column) => (byte)((row << 3) | column);

        public Coordinates(byte row, byte column)
        {
            this.Row = row;
            this.Column = column;
            this.SingleCoordinate = (byte)((row << 3) | column);
        }

        public Coordinates(int row, int column)
        {
            this.Row = (byte)row;
            this.Column = (byte)column;
            this.SingleCoordinate = (byte)((row << 3) | column);
        }

        public Coordinates(byte data)
        {
            this.SingleCoordinate = data;
            this.Row = (byte)(data >> 3);
            this.Column = (byte)(data & maskLowerPart);
        }

        public Coordinates(string humanReadable)
        {
            humanReadable = humanReadable.ToLower();
            this.Column = (byte)(7 - ('h' - humanReadable[0]));
            this.Row = (byte)(7 - (humanReadable[1] - '1'));
            this.SingleCoordinate = (byte)(this.Row * 8 + this.Column);
        }

        public int CompareTo(object obj)
        {
            return this.SingleCoordinate.CompareTo(((Coordinates)obj).SingleCoordinate);
        }

        public override int GetHashCode()
        {
            return this.SingleCoordinate.GetHashCode();
        }

        public bool IsValid => this.Row <=7 && this.Column <= 7;

        public bool IsAdjacent(Coordinates other) => (this.Row - other.Row) * (this.Row - other.Row) <= 1 &&
                                                  (this.Column - other.Column) * (this.Column - other.Column) <= 1;

        public static implicit operator Coordinates(string humanReadable)
        {
            return new Coordinates(humanReadable);
        }

        public static explicit operator Coordinates(byte data)
        {
            return new Coordinates(data);
        }

        public static bool operator ==(Coordinates left, Coordinates right) => left.CompareTo(right) == 0;

        public static bool operator !=(Coordinates left, Coordinates right) => left.CompareTo(right) != 0;

        public override bool Equals([NotNullWhen(true)] object obj) => this == (Coordinates)obj;

        public override string ToString() => this.HumanReadableCoordinates;
    }
}
