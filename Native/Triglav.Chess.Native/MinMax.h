#pragma once

#include "InteropTypes.h"

extern const Evaluation MinEvaluationForWhite;
extern const Evaluation MaxEvaluationForBlack;

__declspec(dllexport) MinMaxResult TriglavChess_External_MinMaxGetTopMove(const MinMaxArguments arguments);

__declspec(dllexport) MinMaxResult TriglavChess_External_MinMaxGetTopMoveFixedDepth(const MinMaxArguments arguments, int32_t* deepestPly);
