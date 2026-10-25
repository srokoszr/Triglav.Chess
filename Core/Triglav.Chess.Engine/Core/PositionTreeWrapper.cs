using System;
using System.Collections.Generic;
using System.Text;
using Triglav.Chess.Engine.Model;
using Triglav.Chess.Interop;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Core
{
    public class PositionTreeWrapper : IDisposable
    {
        public PositionTree NativeTree { get; private set; }

        private bool disposedValue = false;

        public unsafe Evaluation Evaluation => this.NativeTree.Root->BestMoveEvaluation;

        public unsafe MoveWithPieceStruct TopMove
        {
            get
            {
                if (this.NativeTree.Root->Children == null)
                {
                    return default(MoveWithPieceStruct);
                }
                var child = *this.NativeTree.Root->FirstChild;
                var move = Position.ReconstructMove(this.NativeTree.Root->Position, child.Position).Move;

                return move;
            }
        }

        public unsafe MoveWithPieceStruct[] TopLine
        {
            get
            {
                int guardian = 64;
                var result = new List<MoveWithPieceStruct>();

                PositionTreeNode* root = this.NativeTree.Root;
                {
                    var it = root;
                    while (it != null && it->Children != null && guardian > 0)
                    {
                        var child = *it->Children;
                        var move = Position.ReconstructMove(it->Position, child->Position).Move;
                        result.Add(move);

                        it = it->FirstChild;
                        guardian--;
                    }

                    return result.ToArray();
                }
            }
        }

        public unsafe MoveWithPieceStruct[] MovesInOrder
        {
            get
            {
                var result = new List<MoveWithPieceStruct>();

                PositionTreeNode* root = this.NativeTree.Root;
                {
                    for(int i = 0; i < root->AnalysedMovesCount; i++)
                    {
                        var child = root->Children[i];
                        var move = Position.ReconstructMove(root->Position, child->Position);
                        result.Add(move.Move);
                    }
                }

                return result.ToArray();
            }
        }

        private PositionTreeWrapper(PositionTree nativeTree)
        {
            this.NativeTree = nativeTree;
        }

        public static PositionTreeWrapper Build(PositionData position, Colours onMove, int maxDepth)
        {
            var nativeTree = PositionTreeNativeInterface.Build(position, onMove, maxDepth);
            return new PositionTreeWrapper(nativeTree);
        }

        public static unsafe PositionTreeWrapper BuildWithMinMax(PositionData position, Colours onMove, int maxDepth, PositionStackWrapper stack = null)
        {
            stack ??= PositionStackWrapper.New();
            var nativeTree = PositionTreeNativeInterface.MinMaxBuild(position, new EvaluationModifiers().EvaluationModifiersData, maxDepth, onMove, stack.NativeStack);
            return new PositionTreeWrapper(nativeTree);
        }

        public static unsafe PositionTreeWrapper BuildWithMinMaxWithCopying(PositionData position, Colours onMove, int maxDepth, PositionStackWrapper stack = null)
        {
            stack ??= PositionStackWrapper.New();
            var nativeTree = PositionTreeNativeInterface.MinMaxBuildWithCopying(position, new EvaluationModifiers().EvaluationModifiersData, maxDepth, onMove, stack.NativeStack);
            return new PositionTreeWrapper(nativeTree);
        }

        public unsafe void Extend(PositionTreeNode* startingNode, Colours onMove, int maxDepth)
        {
            var positionKey = startingNode->Position;
            var positionData = Position.GetPositionData(positionKey, onMove);
            var nativeTree = PositionTreeNativeInterface.Extend(this.NativeTree, startingNode, positionData, onMove, maxDepth);
            this.NativeTree = nativeTree;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                PositionTreeNativeInterface.Free(this.NativeTree);
                // TODO: set large fields to null
                this.disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~PositionTreeWrapper()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
