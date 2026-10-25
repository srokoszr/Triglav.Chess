using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Triglav.Chess.Engine.Core.Algorithms;
using Triglav.Chess.Engine.Model;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Core
{
    public class Algorithm
    {
        private readonly AlgorithmDepthLevel algorithmDepthLevel;

        private readonly bool humanMode;

        public Algorithm(AlgorithmDepthLevel algorithmDepthLevel, bool humanMode)
        {
            this.algorithmDepthLevel = algorithmDepthLevel;
            this.humanMode = humanMode;
        }

        public (EvaluationWrapper Evaluation, Triglav.Chess.Model.Structs.Interoperable.MoveWithPieceStruct Move) GetMove(Position position, PositionStackWrapper positionStack)
        {
            EvaluationWrapper terminal = PositionEvaluator.GetDefault().EvaluateTerminalPosition(position, positionStack, 0);
            if (terminal.IsTerminal)
            {
                return (terminal, default);
            }

            var bst = new BreadthFirstTreeSearch(this.algorithmDepthLevel);
            var moves = position.GetMoves();

            if (this.algorithmDepthLevel == AlgorithmDepthLevel.RandomMoves)
            {
                var randomMove = moves[Random.Shared.Next(0, moves.Length)];
                return (new Evaluation(), randomMove.MoveStruct);
            }

            if (moves.Length == 1)
            {
                var algorithmResult1 = bst.GetTopMove(position, positionStack, halfMoves: 1);
                return (algorithmResult1.Evaluation, algorithmResult1.Move);
            }

            var algorithmResult = bst.GetTopMove(position, positionStack);

            if (this.humanMode)
            {
                var randomMoveChance = (int)this.algorithmDepthLevel + 1;
                if (Random.Shared.Next() % randomMoveChance == 0)
                {
                    if (algorithmResult.MovesInOrder.Length < (int)this.algorithmDepthLevel + 2)
                    {
                        var randomMove = algorithmResult.MovesInOrder[Random.Shared.Next(0, algorithmResult.MovesInOrder.Length)];
                        return (new Evaluation(), randomMove);
                    }
                    else
                    {
                        var randomMove = algorithmResult.MovesInOrder[Random.Shared.Next(0, algorithmResult.MovesInOrder.Length - ((int)this.algorithmDepthLevel + 2))];
                        return (new Evaluation(), randomMove);
                    }
                }
                else
                {
                    return (algorithmResult.Evaluation, algorithmResult.Move);
                }
            }
            else
            {
                return (algorithmResult.Evaluation, algorithmResult.Move);
            }
        }
    }
}
