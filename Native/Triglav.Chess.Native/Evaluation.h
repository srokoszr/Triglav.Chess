#pragma once

#include "Enumerations.h"
#include "InteropTypes.h"
#include <limits.h>


#define EvaluationMax (INT16_MAX)
#define EvaluationMin (INT16_MIN + 1)
#define EvaluationNaN (INT16_MIN)
#define EvaluationFactor 1024

#define Evaluation_IsGreater(left, right) (TriglavChess_Evaluation_CompareTo(left, right) > 0)
#define Evaluation_IsGreaterOrEqual(left, right) (TriglavChess_Evaluation_CompareTo(left, right) >= 0)
#define Evaluation_IsLess(left, right) (TriglavChess_Evaluation_CompareTo(left, right) < 0)
#define Evaluation_IsLessOrEqual(left, right) (TriglavChess_Evaluation_CompareTo(left, right) <= 0)

#define IsDecisive(evaluation) (evaluation.Result != (int8_t)DrawOrUknown)
#define IsTerminal(evaluation) (EvaluationNaN == evaluation.EvaluationValue)

int TriglavChess_Evaluation_IsAssigned(const Evaluation this);

__declspec(dllexport) int32_t TriglavChess_External_Evaluation_CompareTo_External(const EvaluationCompare args);
int32_t TriglavChess_Evaluation_CompareTo(const Evaluation this, const Evaluation other);