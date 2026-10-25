using System.IO;
using System.Text;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Model
{
    public sealed class EvaluationModifiers
    {
        public const float BasePawnValue = 4f;

        public const float BaseCentiPawnMultiplier = 100f / BasePawnValue;

        public float MaterialValue { get; set; } = BasePawnValue;

        public float PawnValue => this.MaterialValue * 1f;

        private float rookValue = 5f;

        public float RookValue { get => this.rookValue * this.PawnValue; set => this.rookValue = value; }

        private float knightValue = 3f;

        public float KnightValue { get => this.knightValue * this.PawnValue; set => this.knightValue = value; }

        private float bishopValue = 2f;

        public float BishopValue { get => this.bishopValue * this.PawnValue; set => this.bishopValue = value; }

        private float queenValue = 3f;

        public float QueenValue { get => this.queenValue * this.PawnValue; set => this.queenValue = value; }

        private float queenMovedValue = 2.5f;

        public float QueenMovedValue { get => this.queenMovedValue * this.PawnValue; set => this.queenMovedValue = value; }

        public float AttackedFieldsModifier { get; set; } = 4f;

        public float AttackedFieldsWihtoutOwnPiecesModifier { get; set; } = 6f;

        public float CoveredPiecesPercentageModifier { get; set; } = 2f;

        public EvaluationModifiersData EvaluationModifiersData => new EvaluationModifiersData(this.MaterialValue,
            this.PawnValue,
            this.RookValue,
            this.KnightValue,
            this.BishopValue,
            this.QueenValue,
            this.QueenMovedValue,
            this.AttackedFieldsModifier,
            this.AttackedFieldsWihtoutOwnPiecesModifier,
            this.CoveredPiecesPercentageModifier);

        public EvaluationModifiers()
        {

        }

        public void Print(TextWriter stream)
        {
            var sb = new StringBuilder();
            sb.AppendLine("EvaluationModifiers:");
            sb.AppendLine($"\tPawnValue: {this.PawnValue}");
            sb.AppendLine($"\tRookValue: {this.RookValue}");
            sb.AppendLine($"\tBishopValue: {this.BishopValue}");
            sb.AppendLine($"\tQueenValue: {this.QueenValue}");
            sb.AppendLine($"\tQueenMovedValue: {this.QueenMovedValue}");
            sb.AppendLine($"\tAttackedFieldsModifier: {this.AttackedFieldsModifier}");
            sb.AppendLine($"\tAttackedFieldsWihtoutOwnPiecesModifier: {this.AttackedFieldsWihtoutOwnPiecesModifier}");
            sb.AppendLine($"\tCoveredPiecesPercentageModifier: {this.CoveredPiecesPercentageModifier}");
            stream.WriteLine(sb);
        }
    }
}