#include "PieceMoves.h"
#include "GlobalConstants.h"
#include "InteropInterface.h"
#include "MovesEnumerator.h"
#include "ZobristTable.h"
#include <memory.h>
#include <malloc.h>


int TriglavChess_GetMovesWhite(const PositionData position, MoveSimulation* restrict result, MoveSimulation* restrict buffer)
{
    ulong attackedFields = TriglavChess_External_GetAttackedFieldsForHeuristicWhite(position);
    ulong opponentsAttackedFields = TriglavChess_External_GetAttackedFieldsForHeuristicBlack(position);

    ulong isKingHeuristicallyInDanger = opponentsAttackedFields & (MASK << position.WhiteKingLocation);
    ulong isOpponentsKingHeuristicallyInDanger = attackedFields & (MASK << position.BlackKingLocation);


    MoveWithPieceStruct moveCandidates[MaxMovesInPosition + 1];
    int moveCandidatesCount = TriglavChess_External_GetMovesNoCheckValidationWhite(position, moveCandidates);
    if (moveCandidatesCount == 0)
    {
        return 0;
    }
    int resultCount = 0;

    MoveSimulation* movesToBeOrdered_0 = buffer;
    MoveSimulation* movesToBeOrdered_1 = buffer + moveCandidatesCount;
    MoveSimulation* movesToBeOrdered_2 = buffer + moveCandidatesCount + moveCandidatesCount;
    MoveSimulation* movesToBeOrdered_3 = buffer + moveCandidatesCount + moveCandidatesCount + moveCandidatesCount;

    MoveSimulation* index[4] = { movesToBeOrdered_0, movesToBeOrdered_1, movesToBeOrdered_2, movesToBeOrdered_3 };
    int counts[4] = { 0, 0, 0, 0 };

    MoveWithPieceStruct* moveCandidatesIterator = moveCandidates;
    MoveWithPieceStruct* moveCandidatesIteratorEnd = moveCandidates + moveCandidatesCount;

    while (moveCandidatesIterator < moveCandidatesIteratorEnd)
    {
        MoveWithPieceStruct moveCandidate = *(moveCandidatesIterator++);

        ulong doExactCheck = isKingHeuristicallyInDanger | ((moveCandidate.Move.From == position.WhiteKingLocation) && (opponentsAttackedFields & (MASK << moveCandidate.Move.To)));

        ulong maskToUse = MASK << moveCandidate.Move.To;
        ulong maskFrom = MASK << moveCandidate.Move.From;

        ulong takenFields = (position.TakenFields & ~maskFrom) | maskToUse;
        int moveflags = (int)((position.TakenFields & maskToUse) >> moveCandidate.Move.To);//isCapture

        ulong hash = position.Hash ^ TriglavChess_GetZobrist(GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, maskFrom, moveCandidate.Move.From), White, moveCandidate.Move.From);//GetPiece to handle promotions
        if (moveflags)
        {
            hash ^= TriglavChess_GetZobrist(GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, maskToUse, moveCandidate.Move.To), Black, moveCandidate.Move.To);
        }
        hash ^= TriglavChess_GetZobrist(moveCandidate.Piece, White, moveCandidate.Move.To);

        PositionData simulation = {
            hash,
            takenFields,
            position.PieceColours & ~maskToUse,
            (position.Pieces_0 & ~maskFrom & ~maskToUse) | ((moveCandidate.Piece & PIECEMASK_0) >> 2 << moveCandidate.Move.To),
            (position.Pieces_1 & ~maskFrom & ~maskToUse) | ((moveCandidate.Piece & PIECEMASK_1) >> 1 << moveCandidate.Move.To),
            (position.Pieces_2 & ~maskFrom & ~maskToUse) | ((moveCandidate.Piece & PIECEMASK_2) << moveCandidate.Move.To),
            KingMoved == moveCandidate.Piece ? moveCandidate.Move.To : position.WhiteKingLocation,
            position.BlackKingLocation
        };

        if (doExactCheck)
        {
            //exact check
            doExactCheck = IsWhiteKingChecked(simulation);
        }

        if (doExactCheck)
        {
            continue;
        }

        if (isOpponentsKingHeuristicallyInDanger)
        {
            moveflags |= (IsBlackKingChecked(simulation) != 0) << 1;
        }
        else
        {
            ulong attackedFieldsForCheck = TriglavChess_GetAttackedFieldsWhiteMovedPiece(simulation.TakenFields, moveCandidate.Piece,
                CoordinatesGetRow(moveCandidate.Move.To), CoordinatesGetColumn(moveCandidate.Move.To), moveCandidate.Move.To);

            moveflags |= ((attackedFieldsForCheck & (MASK << position.BlackKingLocation)) >> position.BlackKingLocation) << 1;
        }

        *(index[moveflags] + counts[moveflags]++) = (MoveSimulation){ simulation, moveCandidate, moveflags };
    }

    memcpy(result, movesToBeOrdered_3, sizeof(MoveSimulation) * counts[3]);
    resultCount += counts[3];
    memcpy(result + resultCount, movesToBeOrdered_2, sizeof(MoveSimulation) * counts[2]);
    resultCount += counts[2];
    memcpy(result + resultCount, movesToBeOrdered_1, sizeof(MoveSimulation) * counts[1]);
    resultCount += counts[1];
    memcpy(result + resultCount, movesToBeOrdered_0, sizeof(MoveSimulation) * counts[0]);
    resultCount += counts[0];

    return resultCount;
}

int TriglavChess_GetMovesBlack(const PositionData position, MoveSimulation* restrict result, MoveSimulation* restrict buffer)
{
    ulong attackedFields = TriglavChess_External_GetAttackedFieldsForHeuristicBlack(position);
    ulong opponentsAttackedFields = TriglavChess_External_GetAttackedFieldsForHeuristicWhite(position);

    ulong isKingHeuristicallyInDanger = opponentsAttackedFields & (MASK << position.BlackKingLocation);
    ulong isOpponentsKingHeuristicallyInDanger = attackedFields & (MASK << position.WhiteKingLocation);

    MoveWithPieceStruct moveCandidates[MaxMovesInPosition + 1];
    int moveCandidatesCount = TriglavChess_External_GetMovesNoCheckValidationBlack(position, moveCandidates);
    if (moveCandidatesCount == 0)
    {
        return 0;
    }
    int resultCount = 0;

    MoveSimulation* movesToBeOrdered_0 = buffer;
    MoveSimulation* movesToBeOrdered_1 = buffer + moveCandidatesCount;
    MoveSimulation* movesToBeOrdered_2 = buffer + moveCandidatesCount + moveCandidatesCount;
    MoveSimulation* movesToBeOrdered_3 = buffer + moveCandidatesCount + moveCandidatesCount + moveCandidatesCount;

    MoveSimulation* index[4] = { movesToBeOrdered_0, movesToBeOrdered_1, movesToBeOrdered_2, movesToBeOrdered_3 };
    int counts[4] = { 0, 0, 0, 0 };

    MoveWithPieceStruct* moveCandidatesIterator = moveCandidates;
    MoveWithPieceStruct* moveCandidatesIteratorEnd = moveCandidates + moveCandidatesCount;

    while (moveCandidatesIterator < moveCandidatesIteratorEnd)
    {
        MoveWithPieceStruct moveCandidate = *(moveCandidatesIterator++);

        ulong doExactCheck = isKingHeuristicallyInDanger | ((moveCandidate.Move.From == position.BlackKingLocation) && (opponentsAttackedFields & (MASK << moveCandidate.Move.To)));

        ulong maskToUse = MASK << moveCandidate.Move.To;
        ulong maskFrom = MASK << moveCandidate.Move.From;

        ulong takenFields = (position.TakenFields & ~maskFrom) | maskToUse;
        int moveflags = (int)((position.TakenFields & maskToUse) >> moveCandidate.Move.To);//isCapture

        ulong hash = position.Hash ^ TriglavChess_GetZobrist(GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, maskFrom, moveCandidate.Move.From), Black, moveCandidate.Move.From);//GetPiece to handle promotions
        if (moveflags)
        {
            hash ^= TriglavChess_GetZobrist(GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, maskToUse, moveCandidate.Move.To), White, moveCandidate.Move.To);
        }
        hash ^= TriglavChess_GetZobrist(moveCandidate.Piece, Black, moveCandidate.Move.To);

        PositionData simulation = {
            hash,
            takenFields,
            (position.PieceColours & ~maskFrom) | maskToUse,
            (position.Pieces_0 & ~maskFrom & ~maskToUse) | ((moveCandidate.Piece & PIECEMASK_0) >> 2 << moveCandidate.Move.To),
            (position.Pieces_1 & ~maskFrom & ~maskToUse) | ((moveCandidate.Piece & PIECEMASK_1) >> 1 << moveCandidate.Move.To),
            (position.Pieces_2 & ~maskFrom & ~maskToUse) | ((moveCandidate.Piece & PIECEMASK_2) << moveCandidate.Move.To),
            position.WhiteKingLocation,
            KingMoved == moveCandidate.Piece ? moveCandidate.Move.To : position.BlackKingLocation
        };

        if (doExactCheck)
        {
            //exact check
            doExactCheck = IsBlackKingChecked(simulation);
        }

        if (doExactCheck)
        {
            continue;
        }

        if (isOpponentsKingHeuristicallyInDanger)
        {
            moveflags |= (IsWhiteKingChecked(simulation) != 0) << 1;
        }
        else
        {
            ulong attackedFieldsForCheck = TriglavChess_GetAttackedFieldsBlackMovedPiece(simulation.TakenFields, moveCandidate.Piece,
                CoordinatesGetRow(moveCandidate.Move.To), CoordinatesGetColumn(moveCandidate.Move.To), moveCandidate.Move.To);

            moveflags |= ((attackedFieldsForCheck & (MASK << position.WhiteKingLocation)) >> position.WhiteKingLocation) << 1;
        }

        *(index[moveflags] + counts[moveflags]++) = (MoveSimulation){ simulation, moveCandidate, moveflags };
    }

    memcpy(result, movesToBeOrdered_3, sizeof(MoveSimulation) * counts[3]);
    resultCount += counts[3];
    memcpy(result + resultCount, movesToBeOrdered_2, sizeof(MoveSimulation) * counts[2]);
    resultCount += counts[2];
    memcpy(result + resultCount, movesToBeOrdered_1, sizeof(MoveSimulation) * counts[1]);
    resultCount += counts[1];
    memcpy(result + resultCount, movesToBeOrdered_0, sizeof(MoveSimulation) * counts[0]);
    resultCount += counts[0];

    return resultCount;
}

int TriglavChess_GetMovesWhiteNoFlags(const PositionData position, MoveSimulation* restrict result)
{
    ulong opponentsAttackedFields = TriglavChess_External_GetAttackedFieldsForHeuristicBlack(position);

    ulong isKingHeuristicallyInDanger = opponentsAttackedFields & (MASK << position.WhiteKingLocation);

    MoveWithPieceStruct moveCandidates[MaxMovesInPosition + 1];
    int moveCandidatesCount = TriglavChess_External_GetMovesNoCheckValidationWhite(position, moveCandidates);
    if (moveCandidatesCount == 0)
    {
        return 0;
    }
    int resultCount = 0;

    MoveWithPieceStruct* moveCandidatesIterator = moveCandidates;
    MoveWithPieceStruct* moveCandidatesIteratorEnd = moveCandidates + moveCandidatesCount;

    while (moveCandidatesIterator < moveCandidatesIteratorEnd)
    {
        MoveWithPieceStruct moveCandidate = *(moveCandidatesIterator++);

        ulong doExactCheck = isKingHeuristicallyInDanger | ((moveCandidate.Move.From == position.WhiteKingLocation) && (opponentsAttackedFields & (MASK << moveCandidate.Move.To)));

        ulong maskToUse = MASK << moveCandidate.Move.To;
        ulong maskFrom = MASK << moveCandidate.Move.From;

        ulong takenFields = (position.TakenFields & ~maskFrom) | maskToUse;
        int moveflags = (int)((position.TakenFields & maskToUse) >> moveCandidate.Move.To);//isCapture

        ulong hash = position.Hash ^ TriglavChess_GetZobrist(GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, maskFrom, moveCandidate.Move.From), White, moveCandidate.Move.From);//GetPiece to handle promotions
        if (moveflags)
        {
            hash ^= TriglavChess_GetZobrist(GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, maskToUse, moveCandidate.Move.To), Black, moveCandidate.Move.To);
        }
        hash ^= TriglavChess_GetZobrist(moveCandidate.Piece, White, moveCandidate.Move.To);

        PositionData simulation = {
            hash,
            takenFields,
            position.PieceColours & ~maskToUse,
            (position.Pieces_0 & ~maskFrom & ~maskToUse) | ((moveCandidate.Piece & PIECEMASK_0) >> 2 << moveCandidate.Move.To),
            (position.Pieces_1 & ~maskFrom & ~maskToUse) | ((moveCandidate.Piece & PIECEMASK_1) >> 1 << moveCandidate.Move.To),
            (position.Pieces_2 & ~maskFrom & ~maskToUse) | ((moveCandidate.Piece & PIECEMASK_2) << moveCandidate.Move.To),
            KingMoved == moveCandidate.Piece ? moveCandidate.Move.To : position.WhiteKingLocation,
            position.BlackKingLocation
        };

        if (doExactCheck)
        {
            //exact check
            doExactCheck = IsWhiteKingChecked(simulation);
        }

        if (doExactCheck)
        {
            continue;
        }

        *(result + resultCount++) = (MoveSimulation){ simulation, moveCandidate, 0 };
    }

    return resultCount;
}

int TriglavChess_GetMovesBlackNoFlags(const PositionData position, MoveSimulation* restrict result)
{
    ulong opponentsAttackedFields = TriglavChess_External_GetAttackedFieldsForHeuristicWhite(position);

    ulong isKingHeuristicallyInDanger = opponentsAttackedFields & (MASK << position.BlackKingLocation);

    MoveWithPieceStruct moveCandidates[MaxMovesInPosition + 1];
    int moveCandidatesCount = TriglavChess_External_GetMovesNoCheckValidationBlack(position, moveCandidates);
    if (moveCandidatesCount == 0)
    {
        return 0;
    }
    int resultCount = 0;

    MoveWithPieceStruct* moveCandidatesIterator = moveCandidates;
    MoveWithPieceStruct* moveCandidatesIteratorEnd = moveCandidates + moveCandidatesCount;

    while (moveCandidatesIterator < moveCandidatesIteratorEnd)
    {
        MoveWithPieceStruct moveCandidate = *(moveCandidatesIterator++);

        ulong doExactCheck = isKingHeuristicallyInDanger | ((moveCandidate.Move.From == position.BlackKingLocation) && (opponentsAttackedFields & (MASK << moveCandidate.Move.To)));

        ulong maskToUse = MASK << moveCandidate.Move.To;
        ulong maskFrom = MASK << moveCandidate.Move.From;

        ulong takenFields = (position.TakenFields & ~maskFrom) | maskToUse;
        int moveflags = (int)((position.TakenFields & maskToUse) >> moveCandidate.Move.To);//isCapture

        ulong hash = position.Hash ^ TriglavChess_GetZobrist(GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, maskFrom, moveCandidate.Move.From), Black, moveCandidate.Move.From);//GetPiece to handle promotions
        if (moveflags)
        {
            hash ^= TriglavChess_GetZobrist(GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, maskToUse, moveCandidate.Move.To), White, moveCandidate.Move.To);
        }
        hash ^= TriglavChess_GetZobrist(moveCandidate.Piece, Black, moveCandidate.Move.To);

        PositionData simulation = {
            hash,
            takenFields,
            (position.PieceColours & ~maskFrom) | maskToUse,
            (position.Pieces_0 & ~maskFrom & ~maskToUse) | ((moveCandidate.Piece & PIECEMASK_0) >> 2 << moveCandidate.Move.To),
            (position.Pieces_1 & ~maskFrom & ~maskToUse) | ((moveCandidate.Piece & PIECEMASK_1) >> 1 << moveCandidate.Move.To),
            (position.Pieces_2 & ~maskFrom & ~maskToUse) | ((moveCandidate.Piece & PIECEMASK_2) << moveCandidate.Move.To),
            position.WhiteKingLocation,
            KingMoved == moveCandidate.Piece ? moveCandidate.Move.To : position.BlackKingLocation
        };

        if (doExactCheck)
        {
            //exact check
            doExactCheck = IsBlackKingChecked(simulation);
        }

        if (doExactCheck)
        {
            continue;
        }

        *(result + resultCount++) = (MoveSimulation){ simulation, moveCandidate, 0 };
    }

    return resultCount;
}

int32_t TriglavChess_GetMoves(const PositionData position, MoveSimulation* restrict result, MoveSimulation* restrict buffer, const Colours onMove)
{
    return onMove ? TriglavChess_GetMovesBlack(position, result, buffer) : TriglavChess_GetMovesWhite(position, result, buffer);
}

int32_t TriglavChess_GetMovesNoFlags(const PositionData position, MoveSimulation* restrict result, const Colours onMove)
{
    return onMove ? TriglavChess_GetMovesBlackNoFlags(position, result) : TriglavChess_GetMovesWhiteNoFlags(position, result);
}