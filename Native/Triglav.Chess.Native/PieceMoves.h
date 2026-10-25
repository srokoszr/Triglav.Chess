#pragma once
#include "InteropTypes.h"
#include "Coordinates.h"

#define GetPiece(pieces_0, pieces_1, pieces_2, mask, i) (Pieces)(((pieces_0 & mask) >> i) << 2) | (((pieces_1 & mask) >> i) << 1) | ((pieces_2 & mask) >> i)

#define IsFieldTaken(takenFields, candidate) (takenFields & (MASK << candidate))

#define IsPieceColourWhite(whitePieces, candidate) (whitePieces & (MASK << candidate))

#define IsPieceColourBlack(pieceColours, candidate) (pieceColours & (MASK << candidate))

#define IsFieldEmptyOrWhite(pieceColours, candidate) !(pieceColours & (MASK << candidate))

#define IsFieldEmptyOrBlack(whitePieces, candidate) !(whitePieces & (MASK << candidate))

#define HasWhiteKingMoved(pieces_2, whiteKingLocation) (pieces_2 & (MASK << whiteKingLocation))

#define HasBlackKingMoved(pieces_2, blackKingLocation) (pieces_2 & (MASK << blackKingLocation))

#define IsWhiteKingChecked(position) (TriglavChess_External_GetAttackedFieldsBlack(position) & (MASK << position.WhiteKingLocation))
#define IsBlackKingChecked(position) (TriglavChess_External_GetAttackedFieldsWhite(position) & (MASK << position.BlackKingLocation))

#define IsWhiteKingAlone(position) ((position.TakenFields ^ position.PieceColours) == (MASK << position.WhiteKingLocation))
#define IsBlackKingAlone(position) (position.PieceColours == (MASK << position.BlackKingLocation))

ulong TriglavChess_GetAttackedFieldsWhiteSinglePiece(const ulong takenFields, const int hasWhiteKingMoved, const Pieces piece, const int row, const int column, const int singleCoordinate);

ulong TriglavChess_GetAttackedFieldsBlackSinglePiece(const ulong takenFields, const int hasBlackKingMoved, const Pieces piece, const int row, const int column, const int singleCoordinate);

ulong TriglavChess_GetAttackedFieldsForHeuristicWhiteSinglePiece(const ulong takenFields, const int hasWhiteKingMoved, const Pieces piece, const int row, const int column, const int singleCoordinate);

ulong TriglavChess_GetAttackedFieldsForHeuristicBlackSinglePiece(const ulong takenFields, const int hasBlackKingMoved, const Pieces piece, const int row, const int column, const int singleCoordinate);

ulong TriglavChess_GetAttackedFieldsWhiteMovedPiece(const ulong takenFields, const Pieces piece, const int row, const int column, const int singleCoordinate);

ulong TriglavChess_GetAttackedFieldsBlackMovedPiece(const ulong takenFields, const Pieces piece, const int row, const int column, const int singleCoordinate);