#pragma once
#include "interopTypes.h"
#include "MinMax.h"
#include <stdint.h>
#include "PositionEvaluator.h"

//this header file contains all exported functions that can be called directly from C#

#ifdef __cplusplus
extern "C" {  // only need to export C interface if
    // used by C++ source code
#endif

    __declspec(dllexport) MoveSimulation TriglavChess_External_Move(const PositionData position, const MoveWithPieceStruct move, const int32_t onMove);

    __declspec(dllexport) MoveSimulation TriglavChess_External_MoveWhite(const PositionData position, const MoveWithPieceStruct move);

    __declspec(dllexport) MoveSimulation TriglavChess_External_MoveBlack(const PositionData position, const MoveWithPieceStruct move);

    __declspec(dllexport) MoveWithPieceStruct TriglavChess_External_GetMoveBasedOnTargetPositionKey(const PositionData position, const PositionKey target);

    __declspec(dllexport) MoveSimulation TriglavChess_External_MoveBasedOnTargetPositionKey(const PositionData position, const PositionKey target, const int32_t onMove);
    
    __declspec(dllexport) ulong TriglavChess_External_GetTakenFields(const PositionKey position);

    __declspec(dllexport) ulong TriglavChess_External_GetAttackedFields(const PositionData position, const int32_t onMove);

    __declspec(dllexport) ulong TriglavChess_External_GetAttackedFieldsWhite(const PositionData position);

    __declspec(dllexport) ulong TriglavChess_External_GetAttackedFieldsBlack(const PositionData position);

	__declspec(dllexport) ulong TriglavChess_External_GetAttackedFieldsWhite_BitwiseLoop(const PositionData position);

	__declspec(dllexport) ulong TriglavChess_External_GetAttackedFieldsBlack_BitwiseLoop(const PositionData position);

    __declspec(dllexport) ulong TriglavChess_External_GetAttackedFieldsForHeuristicWhite(const PositionData position);

    __declspec(dllexport) ulong TriglavChess_External_GetAttackedFieldsForHeuristicBlack(const PositionData position);

    __declspec(dllexport) int32_t TriglavChess_External_GetMovesNoCheckValidation(const PositionData position, MoveWithPieceStruct* restrict result, const int32_t onMove);

    __declspec(dllexport) int32_t TriglavChess_External_GetMovesNoCheckValidationWhite(const PositionData position, MoveWithPieceStruct* restrict result);

    __declspec(dllexport) int32_t TriglavChess_External_GetMovesNoCheckValidationBlack(const PositionData position, MoveWithPieceStruct* restrict result);

    __declspec(dllexport) int32_t TriglavChess_External_Interop_GetMoves(const PositionData position, MoveSimulation* restrict result, const int32_t onMove);

    __declspec(dllexport) int32_t TriglavChess_External_Interop_GetMovesWhite(const PositionData position, MoveSimulation* restrict result);

    __declspec(dllexport) int32_t TriglavChess_External_Interop_GetMovesBlack(const PositionData position, MoveSimulation* restrict result);

    __declspec(dllexport) int32_t TriglavChess_External_Interop_GetMovesNoFlags(const PositionData position, MoveSimulation* restrict result, const int32_t onMove);

    __declspec(dllexport) Evaluation TriglavChess_External_EvaluateWhite(const EvaluationArguments arguments);

    __declspec(dllexport) Evaluation TriglavChess_External_EvaluateBlack(const EvaluationArguments arguments);

    __declspec(dllexport) Evaluation TriglavChess_External_EvaluateTerminalPositionBlack(const EvaluationArguments arguments);

    __declspec(dllexport) Evaluation TriglavChess_External_EvaluateTerminalPositionWhite(const EvaluationArguments arguments);

    __declspec(dllexport) Evaluation TriglavChess_External_EvaluateNonTerminalPosition(const EvaluationArguments arguments);

    __declspec(dllexport) Evaluation TriglavChess_External_EvaluateTerminalPositionExternal(const EvaluationArguments arguments);

#ifdef __cplusplus
}
#endif
