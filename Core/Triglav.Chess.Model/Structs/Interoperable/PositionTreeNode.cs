using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Runtime.InteropServices;
using System.Text;

namespace Triglav.Chess.Model.Structs.Interoperable
{
    [StructLayout(LayoutKind.Explicit, Size = 48)]
    public readonly unsafe struct PositionTreeNode
    {
        [FieldOffset(0)]
        public readonly PositionKey Position;

        [FieldOffset(32)]
        public readonly PositionTreeNode** Children;

        public PositionTreeNode* FirstChild
        {
            get => this.Children != null ? Children[0] : null;
        }

        [FieldOffset(40)]
        public readonly Evaluation Evaluation;

        public readonly Evaluation BestMoveEvaluation => (this.Children != null) ? this.Children[0]->Evaluation : this.Evaluation;

        [FieldOffset(44)]
        public readonly byte AnalysedMovesCount;

        [FieldOffset(45)]
        public readonly byte LegalMovesCount;

        [FieldOffset(46)]
        public readonly byte AvailableMovesCount;

        public unsafe PositionTreeNode[] ChildrenArr
        {
            get
            {
                var result = new PositionTreeNode[this.AnalysedMovesCount];

                for(int i = 0; i < this.AnalysedMovesCount; i++)
                {
                    result[i] = *this.Children[i];
                }

                return result;
            }
        }
    }
}
