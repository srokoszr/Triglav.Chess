#include "PieceMoves.h"
#include "GlobalConstants.h"
#include "PieceMovesPrecomputed.h"
#include "InteropInterface.h"
#include "Util.h"


ulong TriglavChess_External_GetAttackedFieldsWhite(const PositionData position)
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

		result |= TriglavChess_GetAttackedFieldsWhiteSinglePiece(takenFields, hasWhiteKingMoved,
			GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, mask, i),
			CoordinatesGetRow(i), CoordinatesGetColumn(i), i);
	}

	return result;
}

ulong TriglavChess_External_GetAttackedFieldsBlack(const PositionData position)
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

		result |= TriglavChess_GetAttackedFieldsBlackSinglePiece(takenFields, hasBlackKingMoved,
			GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, mask, i),
			CoordinatesGetRow(i), CoordinatesGetColumn(i), i);
	}

	return result;
}

static ulong TriglavChess_GetAttackedFieldsWhitePrecomputed(const ulong takenFields, const int hasBlackKingMoved, const Pieces piece, const int row, const int column, const int singleCoordinate);
static ulong TriglavChess_GetAttackedFieldsBlackPrecomputed(const ulong takenFields, const int hasBlackKingMoved, const Pieces piece, const int row, const int column, const int singleCoordinate);

ulong TriglavChess_External_GetAttackedFieldsWhite_BitwiseLoop(const PositionData position)
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

		result |= TriglavChess_GetAttackedFieldsWhitePrecomputed(takenFields, hasWhiteKingMoved,
			GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, mask, i),
			CoordinatesGetRow(i), CoordinatesGetColumn(i), i);
	}

	return result;
}

ulong TriglavChess_External_GetAttackedFieldsBlack_BitwiseLoop(const PositionData position)
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

		result |= TriglavChess_GetAttackedFieldsBlackPrecomputed(takenFields, hasBlackKingMoved,
			GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, mask, i),
			CoordinatesGetRow(i), CoordinatesGetColumn(i), i);
	}

	return result;
}

inline ulong TriglavChess_GetAttackedFieldsWhiteSinglePiece(const ulong takenFields, const int hasWhiteKingMoved, const Pieces piece, const int row, const int column, const int singleCoordinate)
{
    ulong candidates = 0;
    int candidate;
    int shift = 1;

    switch (piece)
    {
        case Pawn:
        {
            /*
            if (column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column + 1);
            }

            if (column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column - 1);
            }*/

            /*return ((column <= 6) * (MASK << GetSingleCoordinate(row - 1, column + 1)))
                | ((column > 0) * (MASK << GetSingleCoordinate(row - 1, column - 1)));*/
            return ((column <= 6) * (MASK << (singleCoordinate - 8 + 1)))
                | ((column > 0) * (MASK << (singleCoordinate - 8 - 1)));

            //break;
        }
        case Rook:
        {
            while (row + shift <= 7)
            {
                candidate = GetSingleCoordinate(row + shift, column);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    break;
                }
                shift++;
            }

            shift = 1;
            while (column + shift <= 7)
            {
                candidate = GetSingleCoordinate(row, column + shift);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    break;
                }
                shift++;
            }

            shift = 1;
            while (row - shift >= 0)
            {
                candidate = GetSingleCoordinate(row - shift, column);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    break;
                }
                shift++;
            }

            shift = 1;
            while (column - shift >= 0)
            {
                candidate = GetSingleCoordinate(row, column - shift);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    break;
                }
                shift++;
            }

            break;
        }
        case Knight:
        {
            if (row <= 5 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row + 2, column + 1);
            }

            if (row <= 5 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row + 2, column - 1);
            }

            if (row <= 6 && column <= 5)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column + 2);
            }

            if (row <= 6 && column > 1)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column - 2);
            }

            if (row > 0 && column <= 5)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column + 2);
            }

            if (row > 0 && column > 1)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column - 2);
            }

            if (row > 1 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row - 2, column + 1);
            }

            if (row > 1 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row - 2, column - 1);
            }

            break;
        }
        case Bishop:
        {
            /*if (row <= 5 && column <= 5)
            {
                candidates |= MASK << GetSingleCoordinate(row + 2, column + 2);
            }
            if (row <= 5 && column > 1)
            {
                candidates |= MASK << GetSingleCoordinate(row + 2, column - 2);
            }

            if (row > 1 && column <= 5)
            {
                candidates |= MASK << GetSingleCoordinate(row - 2, column + 2);
            }

            if (row > 1 && column > 1)
            {
                candidates |= MASK << GetSingleCoordinate(row - 2, column - 2);
            }

            break;*/

            return ((row <= 5 && column <= 5) * (MASK << GetSingleCoordinate(row + 2, column + 2)))
                | ((row <= 5 && column > 1) * (MASK << GetSingleCoordinate(row + 2, column - 2)))
                | ((row > 1 && column <= 5) * (MASK << GetSingleCoordinate(row - 2, column + 2)))
                | ((row > 1 && column > 1) * (MASK << GetSingleCoordinate(row - 2, column - 2)));
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
            if (row <= 6 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column + 1);
            }

            if (row <= 6 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column - 1);
            }

            if (row > 0 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column + 1);
            }

            if (row > 0 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column - 1);
            }

            break;
        }
        case King:
        {
            return PieceMovesAttackedFields_WhiteKingFirstMoves;
        }
        case KingMoved:
        {
            if (row <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column);
            }

            if (row <= 6 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column + 1);
            }

            if (row <= 6 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column - 1);
            }

            if (column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row, column + 1);
            }

            if (column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row, column - 1);
            }

            if (row > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column);
            }

            if (row > 0 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column + 1);
            }

            if (row > 0 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column - 1);
            }

            break;
        }
    }

    return candidates;
}

inline ulong TriglavChess_GetAttackedFieldsBlackSinglePiece(const ulong takenFields, const int hasBlackKingMoved, const Pieces piece, const int row, const int column, const int singleCoordinate)
{
    ulong candidates = 0;
    int candidate;
    int shift = 1;

    switch (piece)
    {
        case Pawn:
        {
            /*
            if (column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column + 1);
            }

            if (column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column - 1);
            }*/

            /*return ((column <= 6) * (MASK << GetSingleCoordinate(row + 1, column + 1)))
                | ((column > 0) * (MASK << GetSingleCoordinate(row + 1, column - 1)));*/

            return ((column <= 6) * (MASK << (singleCoordinate + 8 + 1)))
                | ((column > 0) * (MASK << (singleCoordinate + 8 - 1)));

            //break;
        }
        case Rook:
        {
            while (row + shift <= 7)
            {
                candidate = GetSingleCoordinate(row + shift, column);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    break;
                }
                shift++;
            }

            shift = 1;
            while (column + shift <= 7)
            {
                candidate = GetSingleCoordinate(row, column + shift);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    break;
                }
                shift++;
            }

            shift = 1;
            while (row - shift >= 0)
            {
                candidate = GetSingleCoordinate(row - shift, column);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    break;
                }
                shift++;
            }

            shift = 1;
            while (column - shift >= 0)
            {
                candidate = GetSingleCoordinate(row, column - shift);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    break;
                }
                shift++;
            }

            break;
        }
        case Knight:
        {
            if (row <= 5 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row + 2, column + 1);
            }

            if (row <= 5 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row + 2, column - 1);
            }

            if (row <= 6 && column <= 5)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column + 2);
            }

            if (row <= 6 && column > 1)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column - 2);
            }

            if (row > 0 && column <= 5)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column + 2);
            }

            if (row > 0 && column > 1)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column - 2);
            }

            if (row > 1 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row - 2, column + 1);
            }

            if (row > 1 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row - 2, column - 1);
            }

            break;
        }
        case Bishop:
        {
            /*if (row <= 5 && column <= 5)
            {
                candidates |= MASK << GetSingleCoordinate(row + 2, column + 2);
            }
            if (row <= 5 && column > 1)
            {
                candidates |= MASK << GetSingleCoordinate(row + 2, column - 2);
            }

            if (row > 1 && column <= 5)
            {
                candidates |= MASK << GetSingleCoordinate(row - 2, column + 2);
            }

            if (row > 1 && column > 1)
            {
                candidates |= MASK << GetSingleCoordinate(row - 2, column - 2);
            }

            break;*/

            return ((row <= 5 && column <= 5) * (MASK << GetSingleCoordinate(row + 2, column + 2)))
                | ((row <= 5 && column > 1) * (MASK << GetSingleCoordinate(row + 2, column - 2)))
                | ((row > 1 && column <= 5) * (MASK << GetSingleCoordinate(row - 2, column + 2)))
                | ((row > 1 && column > 1) * (MASK << GetSingleCoordinate(row - 2, column - 2)));
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
            if (row <= 6 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column + 1);
            }

            if (row <= 6 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column - 1);
            }

            if (row > 0 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column + 1);
            }

            if (row > 0 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column - 1);
            }

            break;
        }
        case King:
        {
            return PieceMovesAttackedFields_BlackKingFirstMoves;
        }
        case KingMoved:
        {
            if (row <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column);
            }

            if (row <= 6 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column + 1);
            }

            if (row <= 6 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column - 1);
            }

            if (column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row, column + 1);
            }

            if (column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row, column - 1);
            }


            if (row > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column);
            }

            if (row > 0 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column + 1);
            }

            if (row > 0 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column - 1);
            }

            break;
        }
    }

    return candidates;
}

ulong TriglavChess_GetAttackedFieldsWhiteForPrecompute(const ulong takenFields, const int hasWhiteKingMoved, const Pieces piece, const int row, const int column, const int singleCoordinate)
{
	ulong candidates = 0;
	int candidate;
	int shift = 1;

	switch (piece)
	{
	case Pawn:
	{
		/*
		if (column <= 6)
		{
			candidates |= MASK << GetSingleCoordinate(row - 1, column + 1);
		}

		if (column > 0)
		{
			candidates |= MASK << GetSingleCoordinate(row - 1, column - 1);
		}*/

		/*return ((column <= 6) * (MASK << GetSingleCoordinate(row - 1, column + 1)))
			| ((column > 0) * (MASK << GetSingleCoordinate(row - 1, column - 1)));*/
		return ((column <= 6) * (MASK << (singleCoordinate - 8 + 1)))
			| ((column > 0) * (MASK << (singleCoordinate - 8 - 1)));

		//break;
	}
	case Rook:
	{
		while (row + shift <= 7)
		{
			candidate = GetSingleCoordinate(row + shift, column);
			candidates |= MASK << candidate;
			if (IsFieldTaken(takenFields, candidate))
			{
				break;
			}
			shift++;
		}

		shift = 1;
		while (column + shift <= 7)
		{
			candidate = GetSingleCoordinate(row, column + shift);
			candidates |= MASK << candidate;
			if (IsFieldTaken(takenFields, candidate))
			{
				break;
			}
			shift++;
		}

		shift = 1;
		while (row - shift >= 0)
		{
			candidate = GetSingleCoordinate(row - shift, column);
			candidates |= MASK << candidate;
			if (IsFieldTaken(takenFields, candidate))
			{
				break;
			}
			shift++;
		}

		shift = 1;
		while (column - shift >= 0)
		{
			candidate = GetSingleCoordinate(row, column - shift);
			candidates |= MASK << candidate;
			if (IsFieldTaken(takenFields, candidate))
			{
				break;
			}
			shift++;
		}

		break;
	}
	case Knight:
	{
		if (row <= 5 && column <= 6)
		{
			candidates |= MASK << GetSingleCoordinate(row + 2, column + 1);
		}

		if (row <= 5 && column > 0)
		{
			candidates |= MASK << GetSingleCoordinate(row + 2, column - 1);
		}

		if (row <= 6 && column <= 5)
		{
			candidates |= MASK << GetSingleCoordinate(row + 1, column + 2);
		}

		if (row <= 6 && column > 1)
		{
			candidates |= MASK << GetSingleCoordinate(row + 1, column - 2);
		}

		if (row > 0 && column <= 5)
		{
			candidates |= MASK << GetSingleCoordinate(row - 1, column + 2);
		}

		if (row > 0 && column > 1)
		{
			candidates |= MASK << GetSingleCoordinate(row - 1, column - 2);
		}

		if (row > 1 && column <= 6)
		{
			candidates |= MASK << GetSingleCoordinate(row - 2, column + 1);
		}

		if (row > 1 && column > 0)
		{
			candidates |= MASK << GetSingleCoordinate(row - 2, column - 1);
		}

		break;
	}
	case Bishop:
	{
		/*if (row <= 5 && column <= 5)
		{
			candidates |= MASK << GetSingleCoordinate(row + 2, column + 2);
		}
		if (row <= 5 && column > 1)
		{
			candidates |= MASK << GetSingleCoordinate(row + 2, column - 2);
		}

		if (row > 1 && column <= 5)
		{
			candidates |= MASK << GetSingleCoordinate(row - 2, column + 2);
		}

		if (row > 1 && column > 1)
		{
			candidates |= MASK << GetSingleCoordinate(row - 2, column - 2);
		}

		break;*/

		return ((row <= 5 && column <= 5) * (MASK << GetSingleCoordinate(row + 2, column + 2)))
			| ((row <= 5 && column > 1) * (MASK << GetSingleCoordinate(row + 2, column - 2)))
			| ((row > 1 && column <= 5) * (MASK << GetSingleCoordinate(row - 2, column + 2)))
			| ((row > 1 && column > 1) * (MASK << GetSingleCoordinate(row - 2, column - 2)));
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
		if (row <= 6 && column <= 6)
		{
			candidates |= MASK << GetSingleCoordinate(row + 1, column + 1);
		}

		if (row <= 6 && column > 0)
		{
			candidates |= MASK << GetSingleCoordinate(row + 1, column - 1);
		}

		if (row > 0 && column <= 6)
		{
			candidates |= MASK << GetSingleCoordinate(row - 1, column + 1);
		}

		if (row > 0 && column > 0)
		{
			candidates |= MASK << GetSingleCoordinate(row - 1, column - 1);
		}

		break;
	}
	case King:
	{
		return PieceMovesAttackedFields_WhiteKingFirstMoves;
	}
	case KingMoved:
	{
		if (row <= 6)
		{
			candidates |= MASK << GetSingleCoordinate(row + 1, column);
		}

		if (row <= 6 && column <= 6)
		{
			candidates |= MASK << GetSingleCoordinate(row + 1, column + 1);
		}

		if (row <= 6 && column > 0)
		{
			candidates |= MASK << GetSingleCoordinate(row + 1, column - 1);
		}

		if (column <= 6)
		{
			candidates |= MASK << GetSingleCoordinate(row, column + 1);
		}

		if (column > 0)
		{
			candidates |= MASK << GetSingleCoordinate(row, column - 1);
		}

		if (row > 0)
		{
			candidates |= MASK << GetSingleCoordinate(row - 1, column);
		}

		if (row > 0 && column <= 6)
		{
			candidates |= MASK << GetSingleCoordinate(row - 1, column + 1);
		}

		if (row > 0 && column > 0)
		{
			candidates |= MASK << GetSingleCoordinate(row - 1, column - 1);
		}

		break;
	}
	}

	return candidates;
}

ulong TriglavChess_GetAttackedFieldsBlackForPrecompute(const ulong takenFields, const int hasBlackKingMoved, const Pieces piece, const int row, const int column, const int singleCoordinate)
{
	ulong candidates = 0;
	int candidate;
	int shift = 1;

	switch (piece)
	{
	case Pawn:
	{
		/*
		if (column <= 6)
		{
			candidates |= MASK << GetSingleCoordinate(row + 1, column + 1);
		}

		if (column > 0)
		{
			candidates |= MASK << GetSingleCoordinate(row + 1, column - 1);
		}*/

		/*return ((column <= 6) * (MASK << GetSingleCoordinate(row + 1, column + 1)))
			| ((column > 0) * (MASK << GetSingleCoordinate(row + 1, column - 1)));*/

		return ((column <= 6) * (MASK << (singleCoordinate + 8 + 1)))
			| ((column > 0) * (MASK << (singleCoordinate + 8 - 1)));

		//break;
	}
	case Rook:
	{
		while (row + shift <= 7)
		{
			candidate = GetSingleCoordinate(row + shift, column);
			candidates |= MASK << candidate;
			if (IsFieldTaken(takenFields, candidate))
			{
				break;
			}
			shift++;
		}

		shift = 1;
		while (column + shift <= 7)
		{
			candidate = GetSingleCoordinate(row, column + shift);
			candidates |= MASK << candidate;
			if (IsFieldTaken(takenFields, candidate))
			{
				break;
			}
			shift++;
		}

		shift = 1;
		while (row - shift >= 0)
		{
			candidate = GetSingleCoordinate(row - shift, column);
			candidates |= MASK << candidate;
			if (IsFieldTaken(takenFields, candidate))
			{
				break;
			}
			shift++;
		}

		shift = 1;
		while (column - shift >= 0)
		{
			candidate = GetSingleCoordinate(row, column - shift);
			candidates |= MASK << candidate;
			if (IsFieldTaken(takenFields, candidate))
			{
				break;
			}
			shift++;
		}

		break;
	}
	case Knight:
	{
		if (row <= 5 && column <= 6)
		{
			candidates |= MASK << GetSingleCoordinate(row + 2, column + 1);
		}

		if (row <= 5 && column > 0)
		{
			candidates |= MASK << GetSingleCoordinate(row + 2, column - 1);
		}

		if (row <= 6 && column <= 5)
		{
			candidates |= MASK << GetSingleCoordinate(row + 1, column + 2);
		}

		if (row <= 6 && column > 1)
		{
			candidates |= MASK << GetSingleCoordinate(row + 1, column - 2);
		}

		if (row > 0 && column <= 5)
		{
			candidates |= MASK << GetSingleCoordinate(row - 1, column + 2);
		}

		if (row > 0 && column > 1)
		{
			candidates |= MASK << GetSingleCoordinate(row - 1, column - 2);
		}

		if (row > 1 && column <= 6)
		{
			candidates |= MASK << GetSingleCoordinate(row - 2, column + 1);
		}

		if (row > 1 && column > 0)
		{
			candidates |= MASK << GetSingleCoordinate(row - 2, column - 1);
		}

		break;
	}
	case Bishop:
	{
		/*if (row <= 5 && column <= 5)
		{
			candidates |= MASK << GetSingleCoordinate(row + 2, column + 2);
		}
		if (row <= 5 && column > 1)
		{
			candidates |= MASK << GetSingleCoordinate(row + 2, column - 2);
		}

		if (row > 1 && column <= 5)
		{
			candidates |= MASK << GetSingleCoordinate(row - 2, column + 2);
		}

		if (row > 1 && column > 1)
		{
			candidates |= MASK << GetSingleCoordinate(row - 2, column - 2);
		}

		break;*/

		return ((row <= 5 && column <= 5) * (MASK << GetSingleCoordinate(row + 2, column + 2)))
			| ((row <= 5 && column > 1) * (MASK << GetSingleCoordinate(row + 2, column - 2)))
			| ((row > 1 && column <= 5) * (MASK << GetSingleCoordinate(row - 2, column + 2)))
			| ((row > 1 && column > 1) * (MASK << GetSingleCoordinate(row - 2, column - 2)));
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
		if (row <= 6 && column <= 6)
		{
			candidates |= MASK << GetSingleCoordinate(row + 1, column + 1);
		}

		if (row <= 6 && column > 0)
		{
			candidates |= MASK << GetSingleCoordinate(row + 1, column - 1);
		}

		if (row > 0 && column <= 6)
		{
			candidates |= MASK << GetSingleCoordinate(row - 1, column + 1);
		}

		if (row > 0 && column > 0)
		{
			candidates |= MASK << GetSingleCoordinate(row - 1, column - 1);
		}

		break;
	}
	case King:
	{
		return PieceMovesAttackedFields_BlackKingFirstMoves;
	}
	case KingMoved:
	{
		if (row <= 6)
		{
			candidates |= MASK << GetSingleCoordinate(row + 1, column);
		}

		if (row <= 6 && column <= 6)
		{
			candidates |= MASK << GetSingleCoordinate(row + 1, column + 1);
		}

		if (row <= 6 && column > 0)
		{
			candidates |= MASK << GetSingleCoordinate(row + 1, column - 1);
		}

		if (column <= 6)
		{
			candidates |= MASK << GetSingleCoordinate(row, column + 1);
		}

		if (column > 0)
		{
			candidates |= MASK << GetSingleCoordinate(row, column - 1);
		}


		if (row > 0)
		{
			candidates |= MASK << GetSingleCoordinate(row - 1, column);
		}

		if (row > 0 && column <= 6)
		{
			candidates |= MASK << GetSingleCoordinate(row - 1, column + 1);
		}

		if (row > 0 && column > 0)
		{
			candidates |= MASK << GetSingleCoordinate(row - 1, column - 1);
		}

		break;
	}
	}

	return candidates;
}

inline static ulong TriglavChess_GetAttackedFieldsWhitePrecomputed(const ulong takenFields, const int hasWhiteKingMoved, const Pieces piece, const int row, const int column, const int singleCoordinate)
{
	ulong candidates = 0;
	int candidate;
	int shift = 1;

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
				break;
			}
			shift++;
		}

		shift = 1;
		while (column + shift <= 7)
		{
			candidate = GetSingleCoordinate(row, column + shift);
			candidates |= MASK << candidate;
			if (IsFieldTaken(takenFields, candidate))
			{
				break;
			}
			shift++;
		}

		shift = 1;
		while (row - shift >= 0)
		{
			candidate = GetSingleCoordinate(row - shift, column);
			candidates |= MASK << candidate;
			if (IsFieldTaken(takenFields, candidate))
			{
				break;
			}
			shift++;
		}

		shift = 1;
		while (column - shift >= 0)
		{
			candidate = GetSingleCoordinate(row, column - shift);
			candidates |= MASK << candidate;
			if (IsFieldTaken(takenFields, candidate))
			{
				break;
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

inline static ulong TriglavChess_GetAttackedFieldsBlackPrecomputed(const ulong takenFields, const int hasBlackKingMoved, const Pieces piece, const int row, const int column, const int singleCoordinate)
{
	ulong candidates = 0;
	int candidate;
	int shift = 1;

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
				break;
			}
			shift++;
		}

		shift = 1;
		while (column + shift <= 7)
		{
			candidate = GetSingleCoordinate(row, column + shift);
			candidates |= MASK << candidate;
			if (IsFieldTaken(takenFields, candidate))
			{
				break;
			}
			shift++;
		}

		shift = 1;
		while (row - shift >= 0)
		{
			candidate = GetSingleCoordinate(row - shift, column);
			candidates |= MASK << candidate;
			if (IsFieldTaken(takenFields, candidate))
			{
				break;
			}
			shift++;
		}

		shift = 1;
		while (column - shift >= 0)
		{
			candidate = GetSingleCoordinate(row, column - shift);
			candidates |= MASK << candidate;
			if (IsFieldTaken(takenFields, candidate))
			{
				break;
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



ulong TriglavChess_GetAttackedFieldsWhiteMovedPiece(const ulong takenFields, const Pieces piece, const int row, const int column, const int singleCoordinate)
{
    ulong candidates = 0;
    int candidate;
    int shift = 1;

    switch (piece)
    {
        case Pawn:
        {
            /*
            if (column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column + 1);
            }

            if (column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column - 1);
            }*/

            /*return ((column <= 6) * (MASK << GetSingleCoordinate(row - 1, column + 1)))
                | ((column > 0) * (MASK << GetSingleCoordinate(row - 1, column - 1)));*/
            return ((column <= 6) * (MASK << (singleCoordinate - 8 + 1)))
                | ((column > 0) * (MASK << (singleCoordinate - 8 - 1)));

            //break;
        }
        case Rook:
        {
            while (row + shift <= 7)
            {
                candidate = GetSingleCoordinate(row + shift, column);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    break;
                }
                shift++;
            }

            shift = 1;
            while (column + shift <= 7)
            {
                candidate = GetSingleCoordinate(row, column + shift);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    break;
                }
                shift++;
            }

            shift = 1;
            while (row - shift >= 0)
            {
                candidate = GetSingleCoordinate(row - shift, column);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    break;
                }
                shift++;
            }

            shift = 1;
            while (column - shift >= 0)
            {
                candidate = GetSingleCoordinate(row, column - shift);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    break;
                }
                shift++;
            }

            break;
        }
        case Knight:
        {
            if (row <= 5 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row + 2, column + 1);
            }

            if (row <= 5 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row + 2, column - 1);
            }

            if (row <= 6 && column <= 5)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column + 2);
            }

            if (row <= 6 && column > 1)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column - 2);
            }

            if (row > 0 && column <= 5)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column + 2);
            }

            if (row > 0 && column > 1)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column - 2);
            }

            if (row > 1 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row - 2, column + 1);
            }

            if (row > 1 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row - 2, column - 1);
            }

            break;
        }
        case Bishop:
        {
            /*if (row <= 5 && column <= 5)
            {
                candidates |= MASK << GetSingleCoordinate(row + 2, column + 2);
            }
            if (row <= 5 && column > 1)
            {
                candidates |= MASK << GetSingleCoordinate(row + 2, column - 2);
            }

            if (row > 1 && column <= 5)
            {
                candidates |= MASK << GetSingleCoordinate(row - 2, column + 2);
            }

            if (row > 1 && column > 1)
            {
                candidates |= MASK << GetSingleCoordinate(row - 2, column - 2);
            }

            break;*/

            return ((row <= 5 && column <= 5) * (MASK << GetSingleCoordinate(row + 2, column + 2)))
                | ((row <= 5 && column > 1) * (MASK << GetSingleCoordinate(row + 2, column - 2)))
                | ((row > 1 && column <= 5) * (MASK << GetSingleCoordinate(row - 2, column + 2)))
                | ((row > 1 && column > 1) * (MASK << GetSingleCoordinate(row - 2, column - 2)));
        }
        case QueenMoved:
        {
            if (row <= 6 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column + 1);
            }

            if (row <= 6 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column - 1);
            }

            if (row > 0 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column + 1);
            }

            if (row > 0 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column - 1);
            }

            break;
        }
        case KingMoved:
        {
            if (row <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column);
            }

            if (row <= 6 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column + 1);
            }

            if (row <= 6 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column - 1);
            }

            if (column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row, column + 1);
            }

            if (column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row, column - 1);
            }

            if (row > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column);
            }

            if (row > 0 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column + 1);
            }

            if (row > 0 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column - 1);
            }

            break;
        }
        default:
        {
            return 0;
        }
    }

    return candidates;
}

ulong TriglavChess_GetAttackedFieldsBlackMovedPiece(const ulong takenFields, const Pieces piece, const int row, const int column, const int singleCoordinate)
{
    ulong candidates = 0;
    int candidate;
    int shift = 1;

    switch (piece)
    {
        case Pawn:
        {
            /*
            if (column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column + 1);
            }

            if (column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column - 1);
            }*/

            /*return ((column <= 6) * (MASK << GetSingleCoordinate(row + 1, column + 1)))
                | ((column > 0) * (MASK << GetSingleCoordinate(row + 1, column - 1)));*/

            return ((column <= 6) * (MASK << (singleCoordinate + 8 + 1)))
                | ((column > 0) * (MASK << (singleCoordinate + 8 - 1)));

            //break;
        }
        case Rook:
        {
            while (row + shift <= 7)
            {
                candidate = GetSingleCoordinate(row + shift, column);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    break;
                }
                shift++;
            }

            shift = 1;
            while (column + shift <= 7)
            {
                candidate = GetSingleCoordinate(row, column + shift);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    break;
                }
                shift++;
            }

            shift = 1;
            while (row - shift >= 0)
            {
                candidate = GetSingleCoordinate(row - shift, column);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    break;
                }
                shift++;
            }

            shift = 1;
            while (column - shift >= 0)
            {
                candidate = GetSingleCoordinate(row, column - shift);
                candidates |= MASK << candidate;
                if (IsFieldTaken(takenFields, candidate))
                {
                    break;
                }
                shift++;
            }

            break;
        }
        case Knight:
        {
            if (row <= 5 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row + 2, column + 1);
            }

            if (row <= 5 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row + 2, column - 1);
            }

            if (row <= 6 && column <= 5)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column + 2);
            }

            if (row <= 6 && column > 1)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column - 2);
            }

            if (row > 0 && column <= 5)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column + 2);
            }

            if (row > 0 && column > 1)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column - 2);
            }

            if (row > 1 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row - 2, column + 1);
            }

            if (row > 1 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row - 2, column - 1);
            }

            break;
        }
        case Bishop:
        {
            /*if (row <= 5 && column <= 5)
            {
                candidates |= MASK << GetSingleCoordinate(row + 2, column + 2);
            }
            if (row <= 5 && column > 1)
            {
                candidates |= MASK << GetSingleCoordinate(row + 2, column - 2);
            }

            if (row > 1 && column <= 5)
            {
                candidates |= MASK << GetSingleCoordinate(row - 2, column + 2);
            }

            if (row > 1 && column > 1)
            {
                candidates |= MASK << GetSingleCoordinate(row - 2, column - 2);
            }

            break;*/

            return ((row <= 5 && column <= 5) * (MASK << GetSingleCoordinate(row + 2, column + 2)))
                | ((row <= 5 && column > 1) * (MASK << GetSingleCoordinate(row + 2, column - 2)))
                | ((row > 1 && column <= 5) * (MASK << GetSingleCoordinate(row - 2, column + 2)))
                | ((row > 1 && column > 1) * (MASK << GetSingleCoordinate(row - 2, column - 2)));
        }
        case QueenMoved:
        {
            if (row <= 6 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column + 1);
            }

            if (row <= 6 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column - 1);
            }

            if (row > 0 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column + 1);
            }

            if (row > 0 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column - 1);
            }

            break;
        }
        case KingMoved:
        {
            if (row <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column);
            }

            if (row <= 6 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column + 1);
            }

            if (row <= 6 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row + 1, column - 1);
            }

            if (column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row, column + 1);
            }

            if (column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row, column - 1);
            }


            if (row > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column);
            }

            if (row > 0 && column <= 6)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column + 1);
            }

            if (row > 0 && column > 0)
            {
                candidates |= MASK << GetSingleCoordinate(row - 1, column - 1);
            }

            break;
        }
        default:
        {
            return 0;
        }
    }

    return candidates;
}