using System;
using System.Diagnostics;
using System.IO;
using Triglav.Chess.Engine.Model;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Core.Algorithms
{
    public sealed class BreadthFirstTreeSearch
    {
        public int StartingDepth { get; private set; } = (int)AlgorithmDepthLevel.Master * 2;
        public PositionEvaluator PositionEvaluator { get; set; } = new PositionEvaluator(new EvaluationModifiers());

        public BreadthFirstTreeSearch() : this(AlgorithmDepthLevel.Master)
        {

        }

        public BreadthFirstTreeSearch(AlgorithmDepthLevel algorithmDepthLevel)
        {
            this.StartingDepth = (int)algorithmDepthLevel * 2;
        }

        [Obsolete]
        public unsafe (EvaluationWrapper Evaluation, MoveWithPieceStruct Move, MoveWithPieceStruct[] TopLine, MoveWithPieceStruct[] MovesInOrder) GetTopMove(Position position, int halfMoves = 0)
        {
            return this.GetTopMove(position, PositionStackWrapper.New(), null, halfMoves);
        }

        public unsafe (EvaluationWrapper Evaluation, MoveWithPieceStruct Move, MoveWithPieceStruct[] TopLine, MoveWithPieceStruct[] MovesInOrder) GetTopMove(Position position, PositionStackWrapper stack, TextWriter textWriter = null, int halfMoves = 0)
        {
            var startingDepth = halfMoves > 0 ? (halfMoves + 1) / 2 : (this.StartingDepth + ((32 - position.Count) / 4)  + 1) / 2;
            if(halfMoves <= 0 && startingDepth > this.StartingDepth)
            {
                startingDepth = this.StartingDepth;
            }
            Console.WriteLine($"Starting algorithm, {nameof(startingDepth)}:{startingDepth}");

            var stopWatch = Stopwatch.StartNew();

            using (var tree = PositionTreeWrapper.BuildWithMinMaxWithCopying(position.State, position.OnMove, startingDepth, stack))
            {
                stopWatch.Stop();
                Console.WriteLine($"Algorith run for: {stopWatch.Elapsed.ToString()}");
                Console.WriteLine($"MinMax Top move: {tree.TopMove}, MinMax evaluation: {tree.Evaluation}");
                textWriter?.WriteLine($"Algorith run for: {stopWatch.Elapsed.ToString()} at depth: {tree.TopLine?.Length}");
                textWriter?.WriteLine($"MinMax Top move: {tree.TopMove}, MinMax evaluation: {tree.Evaluation}");

                return (tree.Evaluation, tree.TopMove, tree.TopLine, tree.MovesInOrder);
            }
        }

        public void Print(StreamWriter stream)
        {
            this.PositionEvaluator.Modifiers.Print(stream);
            //AlgorithmParameters.Print(stream);
        }
    }
}
