#pragma once
#include "InteropTypes.h"

Evaluation TriglavChess_EvaluateTerminalPosition(const MoveSimulation* restrict simulations, const int32_t simulationsLength, const PositionData position, const Colours onMove, const int32_t move);

Evaluation TriglavChess_EvaluateTerminalPositionForWhite(const MoveSimulation* restrict simulations, const int32_t simulationsLength, const PositionData position, const int32_t move);

Evaluation TriglavChess_EvaluateTerminalPositionForBlack(const MoveSimulation* restrict simulations, const int32_t simulationsLength, const PositionData position, const int32_t move);