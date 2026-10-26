#include "PieceMoves.h"
#include "GlobalConstants.h"
#include "PieceMovesPrecomputed.h"
#include "InteropInterface.h"

ulong TriglavChess_External_GetAttackedFieldsForHeuristicWhite(const PositionData position)
{
    const ulong takenFields = position.TakenFields;
    const ulong pieceColours = position.PieceColours;
	ulong whitePieces = takenFields ^ pieceColours;

    ulong result = 0;
    ulong mask = MASK;
    int hasWhiteKingMoved = HasWhiteKingMoved(position.Pieces_2, position.WhiteKingLocation);
	int i;
	while (whitePieces)
	{
		i = TriglavChess_GetFirstSetBitIndex(whitePieces);
		mask = MASK << i;
		whitePieces &= ~mask;

		result |= TriglavChess_GetAttackedFieldsForHeuristicWhiteSinglePiece(takenFields, hasWhiteKingMoved,
			GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, mask, i),
			CoordinatesGetRow(i), CoordinatesGetColumn(i), i);
	}

    return result;
}

ulong TriglavChess_External_GetAttackedFieldsForHeuristicBlack(const PositionData position)
{
    const ulong takenFields = position.TakenFields;
    const ulong pieceColours = position.PieceColours;
	ulong blackPieces = takenFields & pieceColours;

    ulong result = 0;
    ulong mask = MASK;
    int hasBlackKingMoved = HasBlackKingMoved(position.Pieces_2, position.BlackKingLocation);
	int i;
	while (blackPieces)
	{
		i = TriglavChess_GetFirstSetBitIndex(blackPieces);
		mask = MASK << i;
		blackPieces &= ~mask;

		result |= TriglavChess_GetAttackedFieldsForHeuristicBlackSinglePiece(takenFields, hasBlackKingMoved,
			GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, mask, i),
			CoordinatesGetRow(i), CoordinatesGetColumn(i), i);
	}

    return result;
}

inline ulong TriglavChess_GetAttackedFieldsForHeuristicWhiteSinglePiece(const ulong takenFields, const int hasWhiteKingMoved, const Pieces piece, const int row, const int column, const int singleCoordinate)
{
    ulong candidates = 0;

    int candidate;
    int shift = 1;
    int obstacles = 0;

    switch (piece)
    {
        case Pawn:
        {
			return PieceMovesAttackedFields_WhitePawn[singleCoordinate];
        }
        case Rook:
        {
            while (row + shift <= 7)
            {
                candidate = GetSingleCoordinate(row + shift, column);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    if (obstacles == 2)
                    {
                        break;
                    }
                    else
                    {
                        obstacles++;
                    }
                }
                shift++;
            }

            shift = 1;
            obstacles = 0;
            while (column + shift <= 7)
            {
                candidate = GetSingleCoordinate(row, column + shift);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    if (obstacles == 2)
                    {
                        break;
                    }
                    else
                    {
                        obstacles++;
                    }
                }
                shift++;
            }

            shift = 1;
            obstacles = 0;
            while (row - shift >= 0)
            {
                candidate = GetSingleCoordinate(row - shift, column);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    if (obstacles == 2)
                    {
                        break;
                    }
                    else
                    {
                        obstacles++;
                    }
                }
                shift++;
            }

            shift = 1;
            obstacles = 0;
            while (column - shift >= 0)
            {
                candidate = GetSingleCoordinate(row, column - shift);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    if (obstacles == 2)
                    {
                        break;
                    }
                    else
                    {
                        obstacles++;
                    }
                }
                shift++;
            }

            break;
        }
		case Knight:
		{
			return PieceMovesAttackedFields_WhiteKnight[singleCoordinate];
		}
		case Bishop:
		{
			return PieceMovesAttackedFields_WhiteBishop[singleCoordinate];
		}
        case Queen:
        {
            if (hasWhiteKingMoved)
            {
                if (WhiteQueenStartingPosition == singleCoordinate)
                {
                    return PieceMovesAttackedFields_WhiteQueenFirstMoves;
                }

                candidates |= MASK << GetSingleCoordinate(row + 2, column);

                if (column <= 5)
                {
                    candidates |= MASK << GetSingleCoordinate(row, column + 2);
                }
                if (column > 1)
                {
                    candidates |= MASK << GetSingleCoordinate(row, column - 2);
                }

                if (column <= 5)
                {
                    candidates |= MASK << GetSingleCoordinate(row + 2, column + 2);
                }
                if (column > 1)
                {
                    candidates |= MASK << GetSingleCoordinate(row + 2, column - 2);
                }

                if (column <= 6)
                {
                    candidates |= MASK << GetSingleCoordinate(row + 1, column + 1);
                }
                if (column > 0)
                {
                    candidates |= MASK << GetSingleCoordinate(row + 1, column - 1);
                }
            }

            break;
        }
		case QueenMoved:
		{
			return PieceMovesAttackedFields_WhiteQueen[singleCoordinate];
		}
		case King:
		{
			return PieceMovesAttackedFields_WhiteKingFirstMoves;
		}
		case KingMoved:
		{
			return PieceMovesAttackedFields_WhiteKing[singleCoordinate];
		}
    }

    return candidates;
}

inline ulong TriglavChess_GetAttackedFieldsForHeuristicBlackSinglePiece(const ulong takenFields, const int hasBlackKingMoved, const Pieces piece, const int row, const int column, const int singleCoordinate)
{
    ulong candidates = 0;

    int candidate;
    int shift = 1;
    int obstacles = 0;

    switch (piece)
    {
        case Pawn:
        {
			return PieceMovesAttackedFields_BlackPawn[singleCoordinate];
        }
        case Rook:
        {
            while (row + shift <= 7)
            {
                candidate = GetSingleCoordinate(row + shift, column);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    if (obstacles == 2)
                    {
                        break;
                    }
                    else
                    {
                        obstacles++;
                    }
                }
                shift++;
            }

            shift = 1;
            obstacles = 0;
            while (column + shift <= 7)
            {
                candidate = GetSingleCoordinate(row, column + shift);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    if (obstacles == 2)
                    {
                        break;
                    }
                    else
                    {
                        obstacles++;
                    }
                }
                shift++;
            }

            shift = 1;
            obstacles = 0;
            while (row - shift >= 0)
            {
                candidate = GetSingleCoordinate(row - shift, column);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    if (obstacles == 2)
                    {
                        break;
                    }
                    else
                    {
                        obstacles++;
                    }
                }
                shift++;
            }

            shift = 1;
            obstacles = 0;
            while (column - shift >= 0)
            {
                candidate = GetSingleCoordinate(row, column - shift);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    if (obstacles == 2)
                    {
                        break;
                    }
                    else
                    {
                        obstacles++;
                    }
                }
                shift++;
            }

            break;
        }
		case Knight:
		{
			return PieceMovesAttackedFields_BlackKnight[singleCoordinate];
		}
		case Bishop:
		{
			return PieceMovesAttackedFields_BlackBishop[singleCoordinate];
		}
        case Queen:
        {
            if (hasBlackKingMoved)
            {
                if (BlackQueenStartingPosition == singleCoordinate)
                {
                    return PieceMovesAttackedFields_BlackQueenFirstMoves;
                }

                candidates |= MASK << GetSingleCoordinate(row - 2, column);

                if (column <= 5)
                {
                    candidates |= MASK << GetSingleCoordinate(row, column + 2);
                }
                if (column > 1)
                {
                    candidates |= MASK << GetSingleCoordinate(row, column - 2);
                }

                if (column <= 5)
                {
                    candidates |= MASK << GetSingleCoordinate(row - 2, column + 2);
                }
                if (column > 1)
                {
                    candidates |= MASK << GetSingleCoordinate(row - 2, column - 2);
                }

                if (column <= 6)
                {
                    candidates |= MASK << GetSingleCoordinate(row - 1, column + 1);
                }
                if (column > 0)
                {
                    candidates |= MASK << GetSingleCoordinate(row - 1, column - 1);
                }
            }

            break;
        }
		case QueenMoved:
		{
			return PieceMovesAttackedFields_BlackQueen[singleCoordinate];
		}
		case King:
		{
			return PieceMovesAttackedFields_BlackKingFirstMoves;
		}
		case KingMoved:
		{
			return PieceMovesAttackedFields_BlackKing[singleCoordinate];
		}
    }

    return candidates;
}