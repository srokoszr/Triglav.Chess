using Triglav.Chess.Engine.Model;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Triglav.Chess.Model.Structs.Interoperable;
using Triglav.Chess.Interop;

namespace Triglav.Chess.Engine.Core.Algorithms
{
    public sealed class MinMaxWithAlphaBeta
    {
        public PositionEvaluator PositionEvaluator { get; }

        public AlgorithmParameters AlgorithmParameters { get; }

        public static readonly Evaluation MinEvaluationForWhite = new Evaluation(0, EvaluationResult.BlackWinMate, 0);
        public static readonly Evaluation MaxEvaluationForBlack = new Evaluation(0, EvaluationResult.WhiteWinMate, 0);

        public MinMaxWithAlphaBeta()
        {
            this.PositionEvaluator = PositionEvaluator.GetDefault();
            this.AlgorithmParameters = new AlgorithmParameters();
            this.AlgorithmParameters.Print(Console.Out);
        }

        [Obsolete]
        public unsafe (EvaluationWrapper Evaluation, Triglav.Chess.Model.Structs.Interoperable.MoveWithPieceStruct Move) GetTopMove(Position position, int halfMoves = 0)
        {
            return this.GetTopMove(position, PositionStackWrapper.New(), halfMoves);
        }

        public unsafe (EvaluationWrapper Evaluation, Triglav.Chess.Model.Structs.Interoperable.MoveWithPieceStruct Move) GetTopMove(Position position, PositionStackWrapper stack, int halfMoves = 0)
        {
            var startingDepth = halfMoves > 0 ? halfMoves : this.AlgorithmParameters.StartingDepth + ((32 - position.Count) / 6);
            var positionData = position.State;
            Console.WriteLine($"Starting algorithm, {nameof(startingDepth)}:{startingDepth}");

            var stopWatch = Stopwatch.StartNew();
            var nativeResult = NativeInterface.MinMaxGetTopMove(positionData, this.PositionEvaluator.Modifiers.EvaluationModifiersData, startingDepth, position.OnMove, stack.NativeStack);
            
            stopWatch.Stop();
            Console.WriteLine($"Algorith run for: {stopWatch.Elapsed.ToString()}");
            
            return (nativeResult.Evaluation, nativeResult.Move);
        }

        public void Print(TextWriter stream)
        {
            this.PositionEvaluator.Modifiers.Print(stream);
            this.AlgorithmParameters.Print(stream);
        }
    }
}