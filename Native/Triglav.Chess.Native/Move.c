#include "PieceMoves.h"
#include "GlobalConstants.h"
#include "InteropInterface.h"
#include "ZobristTable.h"
#include "Util.h"

MoveSimulation TriglavChess_External_MoveWhite(const PositionData position, const MoveWithPieceStruct move)
{
    MoveSimulation result;
    ulong maskToUse = MASK << move.Move.To;
    ulong maskFrom = MASK << move.Move.From;
    ulong takenFields = (position.TakenFields & ~maskFrom) | maskToUse;

    result.MoveStruct = move;
    result.MoveFlags = (position.TakenFields & maskToUse) >> move.Move.To;

    result.Position.Hash = position.Hash ^ TriglavChess_GetZobrist(GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, maskFrom, move.Move.From), White, move.Move.From);//GetPiece to handle promotions
    result.Position.Hash ^= result.MoveFlags * TriglavChess_GetZobrist(GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, maskToUse, move.Move.To), Black, move.Move.To);
    result.Position.Hash ^= TriglavChess_GetZobrist(move.Piece, White, move.Move.To);

    result.Position.TakenFields = takenFields;
    result.Position.PieceColours = position.PieceColours & ~maskToUse;
    result.Position.Pieces_0 = (position.Pieces_0 & ~maskFrom & ~maskToUse) | ((move.Piece & PIECEMASK_0) >> 2 << move.Move.To);
    result.Position.Pieces_1 = (position.Pieces_1 & ~maskFrom & ~maskToUse) | ((move.Piece & PIECEMASK_1) >> 1 << move.Move.To);
    result.Position.Pieces_2 = (position.Pieces_2 & ~maskFrom & ~maskToUse) | ((move.Piece & PIECEMASK_2) << move.Move.To);
    result.Position.WhiteKingLocation = KingMoved == move.Piece ? move.Move.To : position.WhiteKingLocation;
    result.Position.BlackKingLocation = position.BlackKingLocation;

    return result;
}

MoveSimulation TriglavChess_External_MoveBlack(const PositionData position, const MoveWithPieceStruct move)
{
    MoveSimulation result;
    ulong maskToUse = MASK << move.Move.To;
    ulong maskFrom = MASK << move.Move.From;

    ulong takenFields = (position.TakenFields & ~maskFrom) | maskToUse;

    result.MoveStruct = move;
    result.MoveFlags = (position.TakenFields & maskToUse) >> move.Move.To;

    result.Position.Hash = position.Hash ^ TriglavChess_GetZobrist(GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, maskFrom, move.Move.From), Black, move.Move.From);//GetPiece to handle promotions
    result.Position.Hash ^= result.MoveFlags * TriglavChess_GetZobrist(GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, maskToUse, move.Move.To), White, move.Move.To);
    result.Position.Hash ^= TriglavChess_GetZobrist(move.Piece, Black, move.Move.To);

    result.Position.TakenFields = takenFields;
    result.Position.PieceColours = (position.PieceColours & ~maskFrom) | maskToUse;
    result.Position.Pieces_0 = (position.Pieces_0 & ~maskFrom & ~maskToUse) | ((move.Piece & PIECEMASK_0) >> 2 << move.Move.To);
    result.Position.Pieces_1 = (position.Pieces_1 & ~maskFrom & ~maskToUse) | ((move.Piece & PIECEMASK_1) >> 1 << move.Move.To);
    result.Position.Pieces_2 = (position.Pieces_2 & ~maskFrom & ~maskToUse) | ((move.Piece & PIECEMASK_2) << move.Move.To);
    result.Position.WhiteKingLocation = position.WhiteKingLocation;
    result.Position.BlackKingLocation = KingMoved == move.Piece ? move.Move.To : position.BlackKingLocation;

    return result;
}

ulong TriglavChess_External_GetTakenFields(const PositionKey position)
{
    //this now has eveything, except White unmoved King
    ulong takenFields = position.PieceColours |
        position.Pieces_0 |
        position.Pieces_1 |
        position.Pieces_2;

    //MovedKing     001
    //(UnMoved)King 000
    ulong whiteMovedKing = position.Pieces_2 &
        ~position.Pieces_1 &
        ~position.Pieces_0 &
        ~position.PieceColours;

    //if White has no KingMoved piece, it means unmoved King is implicitly on E1.
    if (whiteMovedKing == 0)
    {
        takenFields |= MASK << E1;
    }

    return takenFields;
}

MoveWithPieceStruct TriglavChess_External_GetMoveBasedOnTargetPositionKey(const PositionData position, const PositionKey target)
{
    ulong takenOnTarget = TriglavChess_External_GetTakenFields(target);
    //in from field always changes taken value from 1 to 0
    ulong fromMask = ~takenOnTarget & position.TakenFields;
    //to changes taken from 0 to 1, or does not change it, but Colour would chnage from 1 to 0, or 0 to 1 and taken is 1 all time
    ulong toMask = ~fromMask & ((takenOnTarget & ~position.TakenFields) | (position.PieceColours ^ target.PieceColours));

    int8_t from = TriglavChess_GetSetBitIndex(fromMask);
    int8_t to = TriglavChess_GetSetBitIndex(toMask);

    Pieces piece = GetPiece(target.Pieces_0, target.Pieces_1, target.Pieces_2, toMask, to);

    return (MoveWithPieceStruct){ (MoveStruct) { from, to }, piece };
}

MoveSimulation TriglavChess_External_MoveBasedOnTargetPositionKey(const PositionData position, const PositionKey target, const int32_t onMove)
{
    MoveWithPieceStruct move = TriglavChess_External_GetMoveBasedOnTargetPositionKey(position, target);

    return onMove ? TriglavChess_External_MoveBlack(position, move) : TriglavChess_External_MoveWhite(position, move);
}