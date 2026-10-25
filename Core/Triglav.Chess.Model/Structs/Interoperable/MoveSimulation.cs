using System.Runtime.InteropServices;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    [StructLayout(LayoutKind.Explicit, Size = 72)]
    public readonly struct MoveSimulation
    {
        [FieldOffset(0)]
        public readonly PositionData Position;

        [FieldOffset(56)]
        public readonly MoveWithPieceStruct MoveStruct;

        [FieldOffset(68)]
        public readonly MoveFlags MoveFlags;

        public MoveSimulation(PositionData position, MoveWithPieceStruct move, MoveFlags moveFlags)
        {
            this.Position = position;
            this.MoveStruct = move;
            this.MoveFlags = moveFlags;
        }
    }
}
