#include "InteropInterface.h"
#include "Coordinates.h"
#include "GlobalConstants.h"
#include "PieceMoves.h"
#include "MovesEnumerator.h"

#include "malloc.h"

MoveSimulation TriglavChess_External_Move(const PositionData position, const MoveWithPieceStruct move, const int32_t onMove)
{
	return onMove ? TriglavChess_External_MoveBlack(position, move) : TriglavChess_External_MoveWhite(position, move);
}

ulong TriglavChess_External_GetAttackedFields(const PositionData position, const int32_t onMove)
{
	return onMove ? TriglavChess_External_GetAttackedFieldsBlack(position) : TriglavChess_External_GetAttackedFieldsWhite(position);
}

int32_t TriglavChess_External_GetMovesNoCheckValidation(const PositionData position, MoveWithPieceStruct* restrict result, const int32_t onMove)
{
	return onMove ? TriglavChess_External_GetMovesNoCheckValidationBlack(position, result) : TriglavChess_External_GetMovesNoCheckValidationWhite(position, result);
}

int32_t TriglavChess_External_Interop_GetMoves(const PositionData position, MoveSimulation* restrict result, const int32_t onMove)
{
	return onMove ? TriglavChess_External_Interop_GetMovesBlack(position, result) : TriglavChess_External_Interop_GetMovesWhite(position, result);
}

int32_t TriglavChess_External_Interop_GetMovesWhite(const PositionData position, MoveSimulation* restrict result)
{
	MoveSimulation* buffer = (MoveSimulation*)malloc(sizeof(MoveSimulation) * MaxMovesInPosition * 4);
	int resultCount =  TriglavChess_GetMovesWhite(position, result, buffer);
	
	free(buffer);
	return resultCount;
}

int32_t TriglavChess_External_Interop_GetMovesBlack(const PositionData position, MoveSimulation* restrict result)
{
	MoveSimulation* buffer = (MoveSimulation*)malloc(sizeof(MoveSimulation) * MaxMovesInPosition * 4);
	int resultCount = TriglavChess_GetMovesBlack(position, result, buffer);

	free(buffer);
	return resultCount;
}

int32_t TriglavChess_External_Interop_GetMovesNoFlags(const PositionData position, MoveSimulation* restrict result, const int32_t onMove)
{
	return onMove ? TriglavChess_GetMovesBlackNoFlags(position, result) : TriglavChess_GetMovesWhiteNoFlags(position, result);
}
