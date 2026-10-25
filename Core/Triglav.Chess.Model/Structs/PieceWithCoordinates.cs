using System;
using System.Runtime.InteropServices;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.Model.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct PieceWithCoordinates : IComparable<PieceWithCoordinates>
    {
        public const ushort ColourMask = 0b00000000_00000001;
        public const ushort PieceMask  = 0b00000000_00001110;
        public const ushort FieldMask  = 0b00000011_11110000;

        public const int FieldShift = 4;
        public const int PieceShift = 1;

        [FieldOffset(0)]
        private readonly ushort _data;

        public ushort Data => this._data;

        public Colours Colour => (Colours)(this._data % 2);

        public Pieces Piece => (Pieces)((this._data & PieceMask) >> PieceShift);

        public byte Field => (byte)((this._data & FieldMask) >> FieldShift);

        public PieceWithCoordinates(byte colour, byte piece, byte field)
        {
            this._data |= (ushort)colour;
            this._data |= (ushort)((ushort)piece << PieceShift);
            this._data |= (ushort)(field << FieldShift);
        }

        public PieceWithCoordinates(byte colour, PieceData pieceData)
        {
            this._data |= (ushort)colour;
            this._data |= (ushort)((ushort)pieceData.Piece << PieceShift);
            this._data |= (ushort)(pieceData.Location.SingleCoordinate << FieldShift);
        }

        public PieceWithCoordinates(ushort rawData)
        {
            this._data = rawData;
        }

        public void Deconstruct(out Colours colour, out Pieces piece, out byte field)
        {
            colour = this.Colour;
            piece = this.Piece;
            field = this.Field;
        }

        public string Binary
        {
            get
            {
                var data = this._data;
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

        public override string ToString() => $"{this.Colour}{this.Piece}-{new Coordinates(this.Field).HumanReadableCoordinates}";

        public int CompareTo(PieceWithCoordinates other) => this._data.CompareTo(other._data);
    }
}
