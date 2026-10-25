#pragma once
#include "Enumerations.h"
#include "TypeAliases.h"
#include <stdint.h>

typedef struct PositionData
{
    ulong Hash;

    ulong TakenFields;

    ulong PieceColours;

    ulong Pieces_0;

    ulong Pieces_1;

    ulong Pieces_2;

    int32_t WhiteKingLocation;

    int32_t BlackKingLocation;
} PositionData;

typedef struct PositionKey
{
    ulong PieceColours;

    ulong Pieces_0;

    ulong Pieces_1;

    ulong Pieces_2;
} PositionKey;

typedef struct MoveStruct
{
    int32_t From;

    int32_t To;
} MoveStruct;

typedef struct MoveWithPieceStruct
{
    MoveStruct Move;

    Pieces Piece;
} MoveWithPieceStruct;

typedef struct MoveWithPieceStructPacked
{
    uint8_t From;

    uint8_t To;

    uint8_t Piece;
} MoveWithPieceStructPacked;

typedef struct MoveSimulation
{
    PositionData Position;

    MoveWithPieceStruct MoveStruct;

    int32_t MoveFlags;
} MoveSimulation;

typedef struct EvaluationModifiersData
{
    float MaterialValue;

    float PawnValue;

    float RookValue;

    float KnightValue;

    float BishopValue;

    float QueenValue;

    float QueenMovedValue;

    float AttackedFieldsModifier;

    float AttackedFieldsWihtoutOwnPiecesModifier;

    float CoveredPiecesPercentageModifier;
} EvaluationModifiersData;

typedef struct Evaluation
{
    int8_t Result;

    uint8_t InNMoves;

    int16_t EvaluationValue;
} Evaluation;

typedef struct EvaluationCompare
{
    Evaluation This;
    Evaluation Other;
} EvaluationCompare;

typedef struct MinMaxResult
{
    Evaluation Evaluation;

    MoveWithPieceStruct Move;
} MinMaxResult;

typedef struct MinMaxArguments
{
    PositionData Position;

    EvaluationModifiersData EvaluationModifiers;

    int32_t Depth;

    int32_t OnMove;

    struct PositionStack* Stack;
} MinMaxArguments;

typedef struct EvaluationArguments
{
    PositionData Position;

    EvaluationModifiersData EvaluationModifiers;

    int32_t Move;

    int32_t OnMove;
} EvaluationArguments;