#include "Evaluation.h"
#include "InteropTypes.h"
#include "InteropInterface.h"
#include "PieceMoves.h"
#include "Util.h"
#include "TypeAliases.h"
#include "GlobalConstants.h"
#include "PositionEvaluator.h"
#include "MovesEnumerator.h"
#include "Enumerations.h"

Evaluation TriglavChess_External_EvaluateWhite(const EvaluationArguments arguments)
{
    Evaluation terminal = TriglavChess_External_EvaluateTerminalPositionWhite(arguments);

    if (IsTerminal(terminal))
    {
        return terminal;
    }

    return TriglavChess_External_EvaluateNonTerminalPosition(arguments);
}

Evaluation TriglavChess_External_EvaluateBlack(const EvaluationArguments arguments)
{
    Evaluation terminal = TriglavChess_External_EvaluateTerminalPositionBlack(arguments);

    if (IsTerminal(terminal))
    {
        return terminal;
    }

    return TriglavChess_External_EvaluateNonTerminalPosition(arguments);
}

Evaluation TriglavChess_External_EvaluateTerminalPositionWhite(const EvaluationArguments arguments)
{
    MoveSimulation simulations[MaxMovesInPosition];

    int simulationsLength = TriglavChess_GetMovesWhiteNoFlags(arguments.Position, simulations);
    return TriglavChess_EvaluateTerminalPositionForWhite(simulations, simulationsLength, arguments.Position, arguments.Move);
}

Evaluation TriglavChess_External_EvaluateTerminalPositionBlack(const EvaluationArguments arguments)
{
    MoveSimulation simulations[MaxMovesInPosition];

    int simulationsLength = TriglavChess_GetMovesBlackNoFlags(arguments.Position, simulations);
    return TriglavChess_EvaluateTerminalPositionForBlack(simulations, simulationsLength, arguments.Position, arguments.Move);
}

Evaluation TriglavChess_EvaluateTerminalPosition(const MoveSimulation* restrict simulations, const int32_t simulationsLength, const PositionData position, const Colours onMove, const int32_t move)
{
    return onMove == White ? TriglavChess_EvaluateTerminalPositionForWhite(simulations, simulationsLength, position, move) : TriglavChess_EvaluateTerminalPositionForBlack(simulations, simulationsLength, position, move);
}

Evaluation TriglavChess_EvaluateTerminalPositionForWhite(const MoveSimulation* restrict simulations, const int32_t simulationsLength, const PositionData position, const int32_t move)
{
    if (simulationsLength == 0)
    {
        return IsWhiteKingChecked(position)
            ? (Evaluation) { (int8_t)BlackWinMate, move, EvaluationNaN }
        : (Evaluation) { (int8_t)BlackWinStalemate, move, EvaluationNaN };
    }

    if (IsWhiteKingAlone(position))
    {
        for (int i = 0; i < simulationsLength; i++)
        {
            if (IsBlackKingAlone(simulations[i].Position))
            {
                //Double naked mate
                return (Evaluation) { (int8_t)DrawOrUknown, move, EvaluationNaN };
            }
        }

        return (Evaluation) { (int8_t)BlackWinNakedMate,  move, EvaluationNaN };
    }

    return (Evaluation) { (int8_t)DrawOrUknown, 0, 0 };//default
}

Evaluation TriglavChess_EvaluateTerminalPositionForBlack(const MoveSimulation* restrict simulations, const int32_t simulationsLength, const PositionData position, const int32_t move)
{
    if (simulationsLength == 0)
    {
        return IsBlackKingChecked(position)
            ? (Evaluation) { (int8_t)WhiteWinMate, move, EvaluationNaN }
        : (Evaluation) { (int8_t)WhiteWinStalemate, move, EvaluationNaN };
    }

    if (IsBlackKingAlone(position))
    {
        for (int i = 0; i < simulationsLength; i++)
        {
            if (IsWhiteKingAlone(simulations[i].Position))
            {
                //Double naked mate
                return (Evaluation) { (int8_t)DrawOrUknown, move, EvaluationNaN };
            }
        }

        return (Evaluation){ (int8_t)WhiteWinNakedMate, move, EvaluationNaN };
    }

    return (Evaluation) { (int8_t)DrawOrUknown, 0, 0 };//default
}

Evaluation TriglavChess_External_EvaluateNonTerminalPosition(const EvaluationArguments arguments)
{
    const ulong takenFields = arguments.Position.TakenFields;
    const ulong pieceColours = arguments.Position.PieceColours;
    const ulong whitePieces = takenFields ^ pieceColours;

    const ulong whiteAttackedFields = TriglavChess_External_GetAttackedFieldsWhite(arguments.Position);
    const ulong blackAttackedFields = TriglavChess_External_GetAttackedFieldsBlack(arguments.Position);

    int whiteAttackedFieldsCount = TriglavChess_popcount(whiteAttackedFields);
    int blackAttackedFieldsCount = TriglavChess_popcount(blackAttackedFields);

    int countWhite = TriglavChess_popcount(whitePieces);
    int countBlack = TriglavChess_popcount(pieceColours);

    float result = ((whiteAttackedFieldsCount * arguments.EvaluationModifiers.AttackedFieldsModifier) / 32.f) - ((blackAttackedFieldsCount * arguments.EvaluationModifiers.AttackedFieldsModifier) / 32.f);

    float counterWhite = 0.f;
    float counterBlack = 0.f;

    ulong mask = 1;
    for (int i = 0; i < 64; i++, mask <<= 1)
    {
        if (whitePieces & mask)
        {
            Pieces whitePiece = GetPiece(arguments.Position.Pieces_0, arguments.Position.Pieces_1, arguments.Position.Pieces_2, mask, i);
            int row = CoordinatesGetRow(i);

            switch (whitePiece)
            {
                case Pawn:
                {
                    result += arguments.EvaluationModifiers.PawnValue + (6.f - row);
                    break;
                }
                case Rook:
                {
                    result += arguments.EvaluationModifiers.RookValue;
                    break;
                }
                case Knight:
                {
                    result += arguments.EvaluationModifiers.KnightValue;
                    break;
                }
                case Bishop:
                {
                    result += arguments.EvaluationModifiers.BishopValue;
                    break;
                }
                case Queen:
                {
                    result += arguments.EvaluationModifiers.QueenValue;
                    break;
                }
                case QueenMoved:
                {
                    result += arguments.EvaluationModifiers.QueenMovedValue;
                    break;
                }
            }

            if (whiteAttackedFields & mask)
            {
                counterWhite += 1.f;
            }
        }

        if (pieceColours & mask)
        {
            Pieces blackPiece = GetPiece(arguments.Position.Pieces_0, arguments.Position.Pieces_1, arguments.Position.Pieces_2, mask, i);
            int row = CoordinatesGetRow(i);

            switch (blackPiece)
            {
                case Pawn:
                {
                    result -= arguments.EvaluationModifiers.PawnValue + (row - 1.f);
                    break;
                }
                case Rook:
                {
                    result -= arguments.EvaluationModifiers.RookValue;
                    break;
                }
                case Knight:
                {
                    result -= arguments.EvaluationModifiers.KnightValue;
                    break;
                }
                case Bishop:
                {
                    result -= arguments.EvaluationModifiers.BishopValue;
                    break;
                }
                case Queen:
                {
                    result -= arguments.EvaluationModifiers.QueenValue;
                    break;
                }
                case QueenMoved:
                {
                    result -= arguments.EvaluationModifiers.QueenMovedValue;
                    break;
                }
            }

            if (blackAttackedFields & mask)
            {
                counterBlack += 1.f;
            }
        }
    }

    result += (counterWhite / countWhite) * arguments.EvaluationModifiers.CoveredPiecesPercentageModifier;
    result += ((whiteAttackedFieldsCount - counterWhite) / 32.f) * arguments.EvaluationModifiers.AttackedFieldsWihtoutOwnPiecesModifier;

    result -= (counterBlack / countBlack) * arguments.EvaluationModifiers.CoveredPiecesPercentageModifier;
    result -= ((blackAttackedFieldsCount - counterBlack) / 32.f) * arguments.EvaluationModifiers.AttackedFieldsWihtoutOwnPiecesModifier;

    result *= EvaluationFactor;
    if (result >= EvaluationMax)
    {
        result = EvaluationMax;
    }
    else if(result <= EvaluationMin)
    {
        result = EvaluationMin;
    }

    return (Evaluation){ (int8_t)DrawOrUknown, 0, (int16_t)result };
}

Evaluation TriglavChess_External_EvaluateTerminalPositionExternal(const EvaluationArguments arguments)
{
    MoveSimulation simulations[MaxMovesInPosition];
    MoveSimulation buffer[MaxMovesInPosition * 4];

    int simulationsLength = TriglavChess_GetMoves(arguments.Position, simulations, buffer, arguments.OnMove);

    return TriglavChess_EvaluateTerminalPosition(simulations, simulationsLength, arguments.Position, arguments.OnMove, arguments.Move);
}
