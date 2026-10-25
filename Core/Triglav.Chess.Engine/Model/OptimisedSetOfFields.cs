using System;
using System.Runtime.InteropServices;
using System.Text;
using Triglav.Chess.Model.Structs;

namespace Triglav.Chess.Engine.Model
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public ref struct OptimisedSetOfFields
    {
        private const ulong mask = 0x0000_0001;
        private const ulong mask0 = 0x0000_0000;

        [FieldOffset(0)]
        private ulong Buffer;

        [FieldOffset(0)]
        private int Buffer_0;

        [FieldOffset(4)]
        private int Buffer_1;

        public ulong Data => this.Buffer;

        public int Count
        {
            get
            {
                int c = this.Buffer_0;
                // magic (http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel)
                unchecked
                {
                    c = c - (c >> 1 & 0x55555555);
                    c = (c & 0x33333333) + (c >> 2 & 0x33333333);
                    c = (c + (c >> 4) & 0xF0F0F0F) * 0x1010101 >> 24;
                }

                var result = c;

                c = this.Buffer_1;
                unchecked
                {
                    c = c - (c >> 1 & 0x55555555);
                    c = (c & 0x33333333) + (c >> 2 & 0x33333333);
                    c = (c + (c >> 4) & 0xF0F0F0F) * 0x1010101 >> 24;
                }

                return result + c;
            }
        }

        public bool this[int index] {get => (this.Buffer & mask << index) != 0;}

        public OptimisedSetOfFields()
        {
            this.Buffer = mask0;
        }

        public OptimisedSetOfFields(ulong data)
        {
            this.Buffer = data;
        }

        public void Set(int index) => this.Buffer |= mask << index;

        public string Binary
        {
            get
            {
                var data = this.Buffer;
                string result = string.Empty;
                while (data > 0)
                {
                    byte @byte = (byte)(data & 0xFF);
                    result = Convert.ToString(@byte, 2).PadLeft(8, '0') + '_' + result;
                    data >>= 8;
                }

                result = result.TrimEnd('_');

                return "0b" + result;
            }
        }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            sb.Append("{ ");

            for (int i = 0; i < 64; i++)
            {
                if (this[i])
                {
                    if (first)
                    {
                        sb.Append(new Coordinates((byte)i).HumanReadableCoordinates);
                        first = false;
                    }
                    else
                    {
                        sb.Append(", ");
                        sb.Append(new Coordinates((byte)i).HumanReadableCoordinates);
                    }
                }
            }

            sb.Append(" }");

            return sb.ToString();
        }
    }
}