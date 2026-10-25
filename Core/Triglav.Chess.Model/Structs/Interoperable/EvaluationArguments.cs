using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    [StructLayout(LayoutKind.Explicit, Size = 104)]
    public readonly struct EvaluationArguments
    {
        [FieldOffset(0)]
        public readonly PositionData Position;

        [FieldOffset(56)]
        public readonly EvaluationModifiersData EvaluationModifiers;

        [FieldOffset(96)]
        public readonly int Move;

        [FieldOffset(100)]
        public readonly int OnMove;

        public EvaluationArguments(PositionData positionData, EvaluationModifiersData evaluationModifiers, int move)
        {
            this.Position = positionData;
            this.EvaluationModifiers = evaluationModifiers;
            this.Move = move;
        }

        public EvaluationArguments(PositionData positionData, EvaluationModifiersData evaluationModifiers, int move, Colours onMove)
        {
            this.Position = positionData;
            this.EvaluationModifiers = evaluationModifiers;
            this.Move = move;
            this.OnMove = (int)onMove;
        }
    }
}
