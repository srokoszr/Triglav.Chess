using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public readonly struct MoveStruct : IComparable<MoveStruct>
    {
        [FieldOffset(0)]
        public readonly int From;
        
        [FieldOffset(4)]
        public readonly int To;

        public bool IsValid => this.From < 64 && this.To < 64 && this.From != this.To;

        public MoveStruct(byte from, byte to)
        {
            this.From = from;
            this.To = to;
        }

        public override string ToString()
        {
            return $"{new Coordinates((byte)this.From).HumanReadableCoordinates}-{new Coordinates((byte)this.To).HumanReadableCoordinates}";
        }

        public static bool operator ==(MoveStruct left, MoveStruct right) => left.From == right.From && left.To == right.To;

        public static bool operator !=(MoveStruct left, MoveStruct right) => left.From != right.From || left.To != right.To;

        public override bool Equals([NotNullWhen(true)] object obj) => this == (MoveStruct)obj;

        public override int GetHashCode() => this.From << 8 | this.To;

        public static implicit operator MoveStruct(string humanReadable) => Parse(humanReadable);

        public static MoveStruct Parse(string s)
        {
            var split = s.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return new MoveStruct(new Coordinates(split[0]).SingleCoordinate, new Coordinates(split[1]).SingleCoordinate);
        }

        public static bool TryParse(string s, out MoveStruct result)
        {
            try
            {
                result = Parse(s);
                if(!result.IsValid)
                {
                    throw new ArgumentException();
                }
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        public int CompareTo(MoveStruct other)
        {
            int c = other.From.CompareTo(this.From);

            if(c != 0)
            {
                return c;
            }

            return other.To.CompareTo(this.To);
        }
    }
}