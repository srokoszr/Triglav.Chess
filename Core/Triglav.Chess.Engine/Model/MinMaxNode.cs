using Triglav.Chess.Model.Structs;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Model
{
    public sealed class MinMaxNode
    {
        public readonly MoveStruct Move;

        public readonly Evaluation NodeEvaluation;

        public readonly MinMaxNode[] Children;

        public MinMaxNode(MoveStruct move, Evaluation nodeEvaluation, MinMaxNode[] children)
        {
            this.Move = move;
            this.NodeEvaluation = nodeEvaluation;
            this.Children = children;
        }

        public void Print(TextWriter stream, MoveStruct[] previousMoves)
        {
            var sb = new StringBuilder();

            stream.WriteLine("Variation: ");
            string variation = $"{string.Join(',', previousMoves)}...{this.Move}";
            string padded = String.Empty.PadRight(variation.Length - 2);
            stream.WriteLine(variation);

            if (this.Children != null)
            {
                stream.WriteLine("Best moves:");
                foreach (var child in this.Children)
                {
                    if (child != null)
                    {
                        stream.WriteLine($"{padded}- {child.Move}: {child.NodeEvaluation}");
                    }
                }
            }
            stream.WriteLine($"Variation Valuation: {this.NodeEvaluation}");
            stream.WriteLine();
            if (this.Children != null)
            {
                foreach (var child in this.Children)
                {
                    if (child != null)
                    {
                        child.Print(stream, previousMoves.Append(this.Move).ToArray());
                    }
                }
            }
            stream.WriteLine();
        }
    }
}
