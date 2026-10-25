using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Structs;
using System.IO;
using System.Linq;
using System.Text;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Model
{
    public ref struct MinMaxRootNode
    {
        public readonly Position Position;

        public readonly Evaluation NodeEvaluation;

        public readonly MinMaxNode[] Children;

        public readonly MoveStruct TopMove;

        public MinMaxRootNode(Position position, Evaluation nodeEvaluation, MinMaxNode[] children, MoveStruct topMove)
        {
            this.Position = position;
            this.NodeEvaluation = nodeEvaluation;
            this.Children = children;
            this.TopMove = topMove;
        }

        public void Print(TextWriter stream)
        {
            var sb = new StringBuilder();

            stream.WriteLine("Position:");
            stream.WriteLine(this.Position.ToString());

            stream.WriteLine($"Overall evaluation: {this.NodeEvaluation}");
            if (this.TopMove.IsValid)
            {
                stream.WriteLine($"Top move: {this.TopMove}");
            }

            if (this.Children != null)
            {
                stream.WriteLine("Best moves:");
                foreach (var child in this.Children)
                {
                    if (child != null)
                    {
                        stream.WriteLine($"\t{child.Move}: {child.NodeEvaluation}");
                    }
                }
                stream.WriteLine("Top subtrees");
                stream.WriteLine();
                foreach (var child in this.Children)
                {
                    if (child != null)
                    {
                        child.Print(stream, new MoveStruct[] { });
                    }
                }
            }

            stream.WriteLine();

        }
    }
}
