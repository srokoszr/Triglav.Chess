using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    [StructLayout(LayoutKind.Explicit, Size = 40)]
    public readonly struct EvaluationModifiersData
    {
        [FieldOffset(0)]
        public readonly float MaterialValue;

        [FieldOffset(4)]
        public readonly float PawnValue;

        [FieldOffset(8)]
        public readonly float RookValue;

        [FieldOffset(12)]
        public readonly float KnightValue;

        [FieldOffset(16)]
        public readonly float BishopValue;

        [FieldOffset(20)]
        public readonly float QueenValue;

        [FieldOffset(24)]
        public readonly float QueenMovedValue;

        [FieldOffset(28)]
        public readonly float AttackedFieldsModifier;

        [FieldOffset(32)]
        public readonly float AttackedFieldsWihtoutOwnPiecesModifier;

        [FieldOffset(36)]
        public readonly float CoveredPiecesPercentageModifier;

        public EvaluationModifiersData(float materialValue, float pawnValue, float rookValue, float knightValue, float bishopValue, float queenValue, float queenMovedValue, float attackedFieldsModifier, float attackedFieldsWihtoutOwnPiecesModifier, float coveredPiecesPercentageModifier)
        {
            this.MaterialValue = materialValue;
            this.PawnValue = pawnValue;
            this.RookValue = rookValue;
            this.KnightValue = knightValue;
            this.BishopValue = bishopValue;
            this.QueenValue = queenValue;
            this.QueenMovedValue = queenMovedValue;
            this.AttackedFieldsModifier = attackedFieldsModifier;
            this.AttackedFieldsWihtoutOwnPiecesModifier = attackedFieldsWihtoutOwnPiecesModifier;
            this.CoveredPiecesPercentageModifier = coveredPiecesPercentageModifier;
        }
    }
}
