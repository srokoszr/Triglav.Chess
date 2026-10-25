using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    [StructLayout(LayoutKind.Explicit, Size = 112)]
    public readonly unsafe struct MinMaxArguments
    {
        [FieldOffset(0)]
        public readonly PositionData Position;

        [FieldOffset(56)]
        public readonly EvaluationModifiersData EvaluationModifiers;

        [FieldOffset(96)]
        public readonly int Depth;

        [FieldOffset(100)]
        public readonly int OnMove;

        [FieldOffset(104)]
        public readonly PositionStack* Stack;

        public MinMaxArguments(PositionData positionData, EvaluationModifiersData evaluationModifiers, int depth, int onMove, PositionStack* stack)
        {
            this.Position = positionData;
            this.EvaluationModifiers = evaluationModifiers;
            this.Depth = depth;
            this.OnMove = onMove;
            this.Stack = stack;
        }
    }
}
