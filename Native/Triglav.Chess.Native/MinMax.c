#include "MinMax.h"
#include "InteropTypes.h"
#include "InteropInterface.h"
#include "PieceMoves.h"
#include "GlobalConstants.h"
#include "Evaluation.h"
#include "PositionEvaluator.h"
#include "MovesEnumerator.h"
#include "PositionStack.h"
#include "Util.h"
#include <malloc.h>

#define AdditionalDepthForCaptures 2
#define MaxMovesTrackedHardLimitOverStartingDepth 2

typedef struct ExecutionData
{
	EvaluationModifiersData EvaluationModifiers;
	Evaluation BestEvaluationSoFarForWhite;
	Evaluation BestEvaluationSoFarForBlack;
	MoveSimulation* Buffers;
	MoveSimulation* ReusableBuffer;
	PositionStack* Stack;
	int32_t StartingDepth;
	int32_t MaxMovesTracked;
	int32_t UseFixedDepth;
	int32_t DeepestPlyReached;
} ExecutionData;

const Evaluation MinEvaluationForWhite = { BlackWinMate, 0, 0 };
const Evaluation MaxEvaluationForBlack = { WhiteWinMate, 0, 0 };

static Evaluation Search(const PositionData position, ExecutionData* restrict executionData,
	const int depthToGo, int moveCount, Evaluation alpha, Evaluation beta, const Colours onMove);

static MinMaxResult SearchRoot(const PositionData position, ExecutionData* restrict executionData,
	Evaluation alpha, Evaluation beta, const Colours onMove)
{
	const int isWhite = onMove == White;
	PositionStack_Push(executionData->Stack, position);
	if (!isWhite)
	{
		executionData->BestEvaluationSoFarForBlack = MaxEvaluationForBlack;
	}
	Evaluation minMaxEvaluation = isWhite ? MinEvaluationForWhite : executionData->BestEvaluationSoFarForBlack;
	Evaluation* bestSoFar = isWhite ? &executionData->BestEvaluationSoFarForWhite : &executionData->BestEvaluationSoFarForBlack;
	MoveWithPieceStruct minMaxMove = { 0, 0 };
	MoveSimulation* simulations = executionData->Buffers;
	int simulationsLength = TriglavChess_GetMoves(position, simulations, executionData->ReusableBuffer, onMove);
	if (simulationsLength == 0)
	{
		EvaluationArguments arguments = { position, executionData->EvaluationModifiers, 0 };
		return (MinMaxResult) { isWhite ? TriglavChess_External_EvaluateWhite(arguments) : TriglavChess_External_EvaluateBlack(arguments), 0 };
	}

	for (int i = 0; i < simulationsLength; i++)
	{
		MoveWithPieceStruct move = simulations[i].MoveStruct;
		Evaluation evaluation = Search(simulations[i].Position, executionData,
			executionData->StartingDepth - 1, 1, alpha, beta, !onMove);
		PositionStack_Pop(executionData->Stack);

		if (isWhite ? Evaluation_IsGreater(evaluation, minMaxEvaluation) : Evaluation_IsLess(evaluation, minMaxEvaluation))
		{
			minMaxEvaluation = evaluation;
			minMaxMove = move;
			if (isWhite ? Evaluation_IsGreater(evaluation, *bestSoFar) : Evaluation_IsLess(evaluation, *bestSoFar))
			{
				*bestSoFar = evaluation;
			}

			if ((isWhite
				? (minMaxEvaluation.Result == WhiteWinMate || minMaxEvaluation.Result == WhiteWinNakedMate || minMaxEvaluation.Result == WhiteWinStalemate)
				: (minMaxEvaluation.Result == BlackWinMate || minMaxEvaluation.Result == BlackWinNakedMate || minMaxEvaluation.Result == BlackWinStalemate))
				&& minMaxEvaluation.InNMoves == 1)
			{
				break;//we won't find anything better than mate in 1
			}
		}
		if (isWhite ? Evaluation_IsGreaterOrEqual(minMaxEvaluation, beta) : Evaluation_IsLessOrEqual(minMaxEvaluation, alpha))
		{
			break;
		}
		if (isWhite ? Evaluation_IsGreater(minMaxEvaluation, alpha) : Evaluation_IsLess(minMaxEvaluation, beta))
		{
			if (isWhite) { alpha = evaluation; }
			else { beta = evaluation; }
		}
	}
	return (MinMaxResult) { minMaxEvaluation, minMaxMove };
}

static MinMaxResult MinMaxGetTopMoveCore(const MinMaxArguments arguments, const int useFixedDepth, int32_t* deepestPly)
{
	ExecutionData executionData;
	executionData.StartingDepth = arguments.Depth;
	executionData.UseFixedDepth = useFixedDepth;
	executionData.DeepestPlyReached = 0;
	executionData.MaxMovesTracked = arguments.Depth + (useFixedDepth ? 0 : MaxMovesTrackedHardLimitOverStartingDepth);
	executionData.BestEvaluationSoFarForWhite = MinEvaluationForWhite;
	executionData.BestEvaluationSoFarForBlack = MaxEvaluationForBlack;
	executionData.EvaluationModifiers = arguments.EvaluationModifiers;
	int buffersLength = executionData.MaxMovesTracked + 2;
	executionData.Buffers = malloc(sizeof(MoveSimulation) * MaxMovesInPosition * buffersLength);
	executionData.ReusableBuffer = malloc(sizeof(MoveSimulation) * MaxMovesInPosition * 4);
	executionData.Stack = arguments.Stack;
	TriglavChess_PositionStack_EnsureCapacity(executionData.Stack, buffersLength);

	MinMaxResult result = SearchRoot(arguments.Position, &executionData,
		MinEvaluationForWhite, MaxEvaluationForBlack, arguments.OnMove);
	free(executionData.Buffers);
	free(executionData.ReusableBuffer);
	if (deepestPly)
	{
		*deepestPly = executionData.DeepestPlyReached;
	}
	return result;
}

MinMaxResult TriglavChess_External_MinMaxGetTopMove(const MinMaxArguments arguments)
{
	return MinMaxGetTopMoveCore(arguments, 0, NULL);
}

MinMaxResult TriglavChess_External_MinMaxGetTopMoveFixedDepth(const MinMaxArguments arguments, int32_t* deepestPly)
{
	return MinMaxGetTopMoveCore(arguments, 1, deepestPly);
}

static Evaluation Search(const PositionData position, ExecutionData* restrict executionData,
	const int depthToGo, int moveCount, Evaluation alpha, Evaluation beta, const Colours onMove)
{
	if (moveCount > executionData->DeepestPlyReached)
	{
		executionData->DeepestPlyReached = moveCount;
	}
	const int isWhite = onMove == White;
	PositionStack_Push(executionData->Stack, position);
	int moveCountToUse = moveCount / 2;
	Evaluation minMaxEvaluation = isWhite ? MinEvaluationForWhite : MaxEvaluationForBlack;
	Evaluation bestSoFar = isWhite ? executionData->BestEvaluationSoFarForWhite : executionData->BestEvaluationSoFarForBlack;
	if ((isWhite ? (bestSoFar.Result == WhiteWinMate || bestSoFar.Result == WhiteWinNakedMate)
		: (bestSoFar.Result == BlackWinMate || bestSoFar.Result == BlackWinNakedMate))
		&& bestSoFar.InNMoves < moveCountToUse)
	{
		return minMaxEvaluation;
	}
	moveCount++;
	moveCountToUse = moveCount / 2;
	if (executionData->Stack->Count > 4
		&& TriglavChess_PositionsEqual(TriglavChess_PositionStack_Get(executionData->Stack, 0), TriglavChess_PositionStack_Get(executionData->Stack, 3))
		&& TriglavChess_PositionsEqual(TriglavChess_PositionStack_Get(executionData->Stack, 1), TriglavChess_PositionStack_Get(executionData->Stack, 4)))
	{
		//detected simple position repetition
		return (Evaluation) { (int8_t)DrawOrUknown, moveCountToUse, 0 };
	}

	MoveSimulation* simulations = executionData->Buffers + (moveCount * MaxMovesInPosition);
	// moveCount was incremented above; the position is still at moveCount - 1 plies.
	int isMaxDepth = executionData->UseFixedDepth
		? moveCount - 1 >= executionData->MaxMovesTracked
		: depthToGo == 0 || moveCount == executionData->MaxMovesTracked;
	if (isMaxDepth)
	{
		return TriglavChess_External_EvaluateNonTerminalPosition((EvaluationArguments) { position, executionData->EvaluationModifiers, moveCountToUse });
	}
	int simulationsLength = TriglavChess_GetMoves(position, simulations, executionData->ReusableBuffer, onMove);
	if (simulationsLength == 0 || (isWhite ? IsWhiteKingAlone(position) : IsBlackKingAlone(position)))
	{
		Evaluation terminal = TriglavChess_EvaluateTerminalPosition(simulations, simulationsLength, position, onMove, moveCountToUse);
		if (IsTerminal(terminal)) { return terminal; }
	}

	for (int i = 0; i < simulationsLength; i++)
	{
		int newDepth = depthToGo;
		if (!executionData->UseFixedDepth && newDepth < 2)
		{
			newDepth = simulations[i].MoveFlags ? depthToGo + AdditionalDepthForCaptures : depthToGo;
		}
		Evaluation evaluation = Search(simulations[i].Position, executionData,
			newDepth, moveCount, alpha, beta, !onMove);
		PositionStack_Pop(executionData->Stack);
		if (isWhite ? Evaluation_IsGreater(evaluation, minMaxEvaluation) : Evaluation_IsLess(evaluation, minMaxEvaluation))
		{
			minMaxEvaluation = evaluation;
		}
		if (isWhite ? Evaluation_IsGreaterOrEqual(minMaxEvaluation, beta) : Evaluation_IsLessOrEqual(minMaxEvaluation, alpha))
		{
			break;
		}
		if (isWhite ? Evaluation_IsGreater(minMaxEvaluation, alpha) : Evaluation_IsLess(minMaxEvaluation, beta))
		{
			if (isWhite) { alpha = evaluation; }
			else { beta = evaluation; }
		}

		if ((isWhite
			? (minMaxEvaluation.Result == WhiteWinMate || minMaxEvaluation.Result == WhiteWinNakedMate || minMaxEvaluation.Result == WhiteWinStalemate)
			: (minMaxEvaluation.Result == BlackWinMate || minMaxEvaluation.Result == BlackWinNakedMate || minMaxEvaluation.Result == BlackWinStalemate))
			&& minMaxEvaluation.InNMoves == moveCountToUse)
		{
			break;//we won't find anything better than mate
		}
	}
	return minMaxEvaluation;
}
