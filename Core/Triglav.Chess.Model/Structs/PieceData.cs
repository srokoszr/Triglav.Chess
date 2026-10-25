using Triglav.Chess.Model.Enumerations;
using System;
using System.Runtime.InteropServices;

namespace Triglav.Chess.Model.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public readonly struct PieceData
    {
        [FieldOffset(0)]
        public readonly byte Piece;

        [FieldOffset(1)]
        public readonly Coordinates Location;

        public PieceData(byte piece, Coordinates location)
        {
            this.Piece = piece;
            this.Location = location;
        }
    }
}
