using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    //[StructLayout(LayoutKind.Explicit, Size = 8, Pack = 2)]
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct EvaluationCompare
    {
        //[FieldOffset(0)]
        public readonly Evaluation This;

        //[FieldOffset(4)]
        public readonly Evaluation Other;

        public EvaluationCompare(Evaluation @this, Evaluation other)
        {
            this.This = @this;
            this.Other = other;
        }
    }
}
