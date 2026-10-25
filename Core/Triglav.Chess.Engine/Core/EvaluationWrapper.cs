using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Triglav.Chess.Engine.Model;
using Triglav.Chess.Interop;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;

namespace Triglav.Chess.Engine.Core
{
    public class EvaluationWrapper : IEquatable<EvaluationWrapper>, IComparable<EvaluationWrapper>
    {
        public Evaluation Evaluation { get; }

        public EvaluationResult Result => this.Evaluation.EvaluationResult;

        public float EvaluationValue => this.Evaluation.EvaluationValue / Evaluation.EvaluationFactor;

        public int InNMoves => this.Evaluation.InNMoves;

        public float CentiPawnsEvaluation => this.EvaluationValue * EvaluationModifiers.BaseCentiPawnMultiplier;

        //based on Lichess evaluation bar - https://lichess.org/page/accuracy
        public double WinChanceFunc => this.IsBlackWin ? 
            -1f : this.IsWhiteWin ? 
                1f : this.IsDraw ? 
                    0f : 2f / (1f + Math.Exp(-0.004f * this.CentiPawnsEvaluation)) - 1f;

        public double WhiteWinChance => 50f + (50f * WinChanceFunc);

        public EvaluationWrapper(Evaluation evaluation)
        {
            this.Evaluation = evaluation;
        }

        public bool IsDecisive => this.Result != EvaluationResult.DrawOrUknown;

        public bool IsTerminal => Evaluation.EvaluationNaN == this.Evaluation.EvaluationValue;

        public bool IsDraw => this.Result == EvaluationResult.DrawOrUknown && this.IsTerminal;

        public bool IsUknown => this.Result == EvaluationResult.DrawOrUknown && !this.IsTerminal;

        public bool IsWhiteWin => (int)this.Result >= (int)EvaluationResult.WhiteWinStalemate;

        public bool IsBlackWin => (int)this.Result <= (int)EvaluationResult.BlackWinStalemate;

        public bool Equals(EvaluationWrapper other) => this.Evaluation.Equals(other.Evaluation);

        public static bool operator ==(EvaluationWrapper left, EvaluationWrapper right) => left.Evaluation == right.Evaluation;

        public static bool operator !=(EvaluationWrapper left, EvaluationWrapper right) => left.Evaluation != right.Evaluation;

        public override bool Equals([NotNullWhen(true)] object obj) => this == (EvaluationWrapper)obj;

        public override int GetHashCode() => this.Evaluation.GetHashCode();

        public int CompareTo(EvaluationWrapper other) => NativeInterface.EvaluationCompareTo(this.Evaluation, other.Evaluation);        

        public static bool operator >(EvaluationWrapper left, EvaluationWrapper right) => left.CompareTo(right) > 0;

        public static bool operator <(EvaluationWrapper left, EvaluationWrapper right) => left.CompareTo(right) < 0;

        public static bool operator >=(EvaluationWrapper left, EvaluationWrapper right) => left.CompareTo(right) >= 0;

        public static bool operator <=(EvaluationWrapper left, EvaluationWrapper right) => left.CompareTo(right) <= 0;

        public static implicit operator EvaluationWrapper(Evaluation evaluation) => new EvaluationWrapper(evaluation);

        public override string ToString() => this.IsTerminal ? $"{this.Evaluation.EvaluationResult} in {this.Evaluation.InNMoves} {(this.Evaluation.InNMoves == 1 ? "move" : "moves")}" : $"{this.CentiPawnsEvaluation.ToString()}cp - win chance: {this.WhiteWinChance}%";
    }
}
