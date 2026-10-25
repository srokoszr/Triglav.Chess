using Triglav.Chess.Engine.Core;
using System.Runtime.InteropServices;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Model
{
    public readonly struct MoveSimulationExtended
    {
        public readonly MoveWithPieceStruct MoveStruct;

        public readonly Position Position;
        
        public readonly bool IsCapture;

        public readonly bool IsCheck;

        public MoveSimulationExtended(Position position, MoveWithPieceStruct move, bool isCapture, bool isCheck)
        {
            this.Position = position;
            this.MoveStruct = move;
            this.IsCapture = isCapture;
            this.IsCheck = isCheck;
        }
    }
}
