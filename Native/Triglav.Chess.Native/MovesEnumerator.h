#pragma once
#include "InteropTypes.h"

int32_t TriglavChess_GetMovesWhiteNoFlags(const PositionData position, MoveSimulation* restrict result);
int32_t TriglavChess_GetMovesBlackNoFlags(const PositionData position, MoveSimulation* restrict result);

int32_t TriglavChess_GetMoves(const PositionData position, MoveSimulation* restrict result, MoveSimulation* restrict buffer, const Colours onMove);
int32_t TriglavChess_GetMovesNoFlags(const PositionData position, MoveSimulation* restrict result, const Colours onMove);

int32_t TriglavChess_GetMovesWhite(const PositionData position, MoveSimulation* restrict result, MoveSimulation* restrict buffer);
int32_t TriglavChess_GetMovesBlack(const PositionData position, MoveSimulation* restrict result, MoveSimulation* restrict buffer);