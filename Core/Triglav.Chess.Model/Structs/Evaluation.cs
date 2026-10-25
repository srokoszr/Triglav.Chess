using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.Model.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 4, Pack = 1)]
    //[StructLayout(LayoutKind.Sequential)]
    public readonly struct Evaluation : IEquatable<Evaluation>
    {
        public const int EvaluationMax = Int16.MaxValue;
        public const int EvaluationMin = Int16.MinValue + 1;
        public const int EvaluationNaN = Int16.MinValue;
        public const float EvaluationFactor = 1024f;

        [FieldOffset(0)]
        public readonly sbyte Result;

        [FieldOffset(1)]
        public readonly byte InNMoves;

        public EvaluationResult EvaluationResult => (EvaluationResult)this.Result;

        [FieldOffset(2)]
        public readonly Int16 EvaluationValue;
        
        public Evaluation(Int16 evaluationValue, EvaluationResult evaluationResult, byte inNMoves)
        {
            this.Result = (sbyte)evaluationResult;
            this.InNMoves = inNMoves;

            this.EvaluationValue = evaluationResult switch
            {
                EvaluationResult.DrawOrUknown => evaluationValue,
                EvaluationResult.WhiteWinStalemate => EvaluationNaN,
                EvaluationResult.WhiteWinNakedMate => EvaluationNaN,
                EvaluationResult.WhiteWinMate => EvaluationNaN,
                EvaluationResult.BlackWinStalemate => EvaluationNaN,
                EvaluationResult.BlackWinNakedMate => EvaluationNaN,
                EvaluationResult.BlackWinMate => EvaluationNaN,
                _ => evaluationValue
            };
        }

        //for draw
        private Evaluation(byte inNMoves)
        {
            this.Result = (sbyte)EvaluationResult.DrawOrUknown;
            this.InNMoves = inNMoves;
            this.EvaluationValue = EvaluationNaN;
        }

        //for unknown
        public Evaluation(Int16 evaluationValue) : this(evaluationValue, EvaluationResult.DrawOrUknown, 0)
        {

        }

        public static Evaluation Draw(byte inNMoves) => new Evaluation(inNMoves);

        public static bool operator ==(Evaluation left, Evaluation right) => left.Result == right.Result && left.InNMoves == right.InNMoves && (left.EvaluationValue == right.EvaluationValue || (float.IsNaN(left.EvaluationValue) && float.IsNaN(right.EvaluationValue)));

        public static bool operator !=(Evaluation left, Evaluation right) => left.Result != right.Result || left.InNMoves != right.InNMoves || left.EvaluationValue != right.EvaluationValue;

        public override bool Equals([NotNullWhen(true)] object obj) => this == (Evaluation)obj;

        public override int GetHashCode() => this.EvaluationValue.GetHashCode();

        public bool Equals(Evaluation other) => this == other;

        public override string ToString() => $"{this.EvaluationResult} in {this.InNMoves} {(this.InNMoves == 1 ? "move" : "moves")} - {this.EvaluationValue.ToString()}";
    }
}
