#include "PieceMoves.h"
#include "GlobalConstants.h"
#include "PieceMovesPrecomputed.h"
#include "InteropInterface.h"

int TriglavChess_External_GetMovesNoCheckValidationWhite(const PositionData position, MoveWithPieceStruct* restrict result)
{
    const ulong takenFields = position.TakenFields;
    const ulong pieceColours = position.PieceColours;
	const ulong whitePieces = takenFields ^ pieceColours;
	ulong whitePiecesItr = whitePieces;
        
    ulong mask = MASK;
    int resultCount = 0;
    int shift;
    int candidate;

	int i;
	while (whitePiecesItr)
	{
		i = TriglavChess_GetFirstSetBitIndex(whitePiecesItr);
		mask = MASK << i;
		whitePiecesItr &= ~mask;

		int row = CoordinatesGetRow(i);
		int column = CoordinatesGetColumn(i);

		switch (GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, mask, i))
		{
		case Pawn:
		{
			if (column <= 6)
			{
				candidate = GetSingleCoordinate(row - 1, column + 1);
				if (IsPieceColourBlack(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, row == 1 ? Queen : Pawn };
				}
			}

			if (column > 0)
			{
				candidate = GetSingleCoordinate(row - 1, column - 1);
				if (IsPieceColourBlack(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, row == 1 ? Queen : Pawn };
				}
			}

			candidate = GetSingleCoordinate(row - 1, column);
			if (!IsFieldTaken(takenFields, candidate))
			{
				*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, row == 1 ? Queen : Pawn };

				if (row == 6)
				{
					candidate = GetSingleCoordinate(row - 2, column);
					if (!IsFieldTaken(takenFields, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Pawn };
					}
				}
			}

			break;
		}
		case Rook:
		{
			shift = 1;
			while (row + shift <= 7)
			{
				candidate = GetSingleCoordinate(row + shift, column);
				if (!IsFieldTaken(takenFields, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Rook };
				}
				else
				{
					if (IsPieceColourBlack(pieceColours, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Rook };
					}
					break;
				}
				shift++;
			}

			shift = 1;
			while (column + shift <= 7)
			{
				candidate = GetSingleCoordinate(row, column + shift);
				if (!IsFieldTaken(takenFields, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Rook };
				}
				else
				{
					if (IsPieceColourBlack(pieceColours, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Rook };
					}
					break;
				}
				shift++;
			}

			shift = 1;
			while (row - shift >= 0)
			{
				candidate = GetSingleCoordinate(row - shift, column);
				if (!IsFieldTaken(takenFields, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Rook };
				}
				else
				{
					if (IsPieceColourBlack(pieceColours, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Rook };
					}
					break;
				}
				shift++;
			}

			shift = 1;
			while (column - shift >= 0)
			{
				candidate = GetSingleCoordinate(row, column - shift);
				if (!IsFieldTaken(takenFields, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Rook };
				}
				else
				{
					if (IsPieceColourBlack(pieceColours, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Rook };
					}
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
				candidate = GetSingleCoordinate(row + 2, column + 1);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Knight };
				}
			}

			if (row <= 5 && column > 0)
			{
				candidate = GetSingleCoordinate(row + 2, column - 1);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Knight };
				}
			}

			if (row <= 6 && column <= 5)
			{
				candidate = GetSingleCoordinate(row + 1, column + 2);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Knight };
				}
			}

			if (row <= 6 && column > 1)
			{
				candidate = GetSingleCoordinate(row + 1, column - 2);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Knight };
				}
			}

			if (row > 0 && column <= 5)
			{
				candidate = GetSingleCoordinate(row - 1, column + 2);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Knight };
				}
			}

			if (row > 0 && column > 1)
			{
				candidate = GetSingleCoordinate(row - 1, column - 2);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Knight };
				}
			}

			if (row > 1 && column <= 6)
			{
				candidate = GetSingleCoordinate(row - 2, column + 1);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Knight };
				}
			}

			if (row > 1 && column > 0)
			{
				candidate = GetSingleCoordinate(row - 2, column - 1);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Knight };
				}
			}

			break;
		}
		case Bishop:
		{
			if (row <= 5 && column <= 5)
			{
				candidate = GetSingleCoordinate(row + 2, column + 2);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Bishop };
				}
			}

			if (row <= 5 && column > 1)
			{
				candidate = GetSingleCoordinate(row + 2, column - 2);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Bishop };
				}
			}

			if (row > 1 && column <= 5)
			{
				candidate = GetSingleCoordinate(row - 2, column + 2);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Bishop };
				}
			}

			if (row > 1 && column > 1)
			{
				candidate = GetSingleCoordinate(row - 2, column - 2);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Bishop };
				}
			}

			break;
		}
		case Queen:
		{
			// Promoted pawns do not have to wait for their king to move.
			if (i != WhiteQueenStartingPosition || HasWhiteKingMoved(position.Pieces_2, position.WhiteKingLocation))
			{
				if (WhiteQueenStartingPosition == i)
				{
					for (int j = 0; j < WhiteQueenFirstMovesLength; j++)
					{
						if (IsFieldEmptyOrBlack(whitePieces, PieceMoves_WhiteQueenFirstMoves[j]))
						{
							*(result + resultCount++) = (MoveWithPieceStruct){ {i, PieceMoves_WhiteQueenFirstMoves[j]}, QueenMoved };
						}
					}

					break;
				}

				// Forward is off the board after promotion; no backward straight jump.
				if (column <= 5)
				{
					candidate = GetSingleCoordinate(row, column + 2);
					if (IsFieldEmptyOrBlack(whitePieces, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
					}
				}
				if (column > 1)
				{
					candidate = GetSingleCoordinate(row, column - 2);
					if (IsFieldEmptyOrBlack(whitePieces, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
					}
				}

				if (column <= 5)
				{
					candidate = GetSingleCoordinate(row + 2, column + 2);
					if (IsFieldEmptyOrBlack(whitePieces, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
					}
				}
				if (column > 1)
				{
					candidate = GetSingleCoordinate(row + 2, column - 2);
					if (IsFieldEmptyOrBlack(whitePieces, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
					}
				}

				if (column <= 6)
				{
					candidate = GetSingleCoordinate(row + 1, column + 1);
					if (IsFieldEmptyOrBlack(whitePieces, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
					}
				}
				if (column > 0)
				{
					candidate = GetSingleCoordinate(row + 1, column - 1);
					if (IsFieldEmptyOrBlack(whitePieces, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
					}
				}
			}

			break;
		}
		case QueenMoved:
		{
			if (row <= 6 && column <= 6)
			{
				candidate = GetSingleCoordinate(row + 1, column + 1);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
				}
			}

			if (row <= 6 && column > 0)
			{
				candidate = GetSingleCoordinate(row + 1, column - 1);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
				}
			}

			if (row > 0 && column <= 6)
			{
				candidate = GetSingleCoordinate(row - 1, column + 1);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
				}
			}

			if (row > 0 && column > 0)
			{
				candidate = GetSingleCoordinate(row - 1, column - 1);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
				}
			}

			break;
		}
		case King:
		{
			for (int j = 0; j < WhiteKingFirstMovesLength; j++)
			{
				if (IsFieldEmptyOrBlack(whitePieces, PieceMoves_WhiteKingFirstMoves[j]))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, PieceMoves_WhiteKingFirstMoves[j]}, KingMoved };
				}
			}

			break;
		}
		case KingMoved:
		{
			if (row <= 6)
			{
				candidate = GetSingleCoordinate(row + 1, column);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, KingMoved };
				}
			}

			if (row <= 6 && column <= 6)
			{
				candidate = GetSingleCoordinate(row + 1, column + 1);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, KingMoved };
				}
			}

			if (row <= 6 && column > 0)
			{
				candidate = GetSingleCoordinate(row + 1, column - 1);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, KingMoved };
				}
			}

			if (column <= 6)
			{
				candidate = GetSingleCoordinate(row, column + 1);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, KingMoved };
				}
			}

			if (column > 0)
			{
				candidate = GetSingleCoordinate(row, column - 1);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, KingMoved };
				}
			}

			if (row > 0)
			{
				candidate = GetSingleCoordinate(row - 1, column);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, KingMoved };
				}
			}

			if (row > 0 && column <= 6)
			{
				candidate = GetSingleCoordinate(row - 1, column + 1);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, KingMoved };
				}
			}

			if (row > 0 && column > 0)
			{
				candidate = GetSingleCoordinate(row - 1, column - 1);
				if (IsFieldEmptyOrBlack(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, KingMoved };
				}
			}

			break;
		}
		}
	}

    return resultCount;
}

int TriglavChess_External_GetMovesNoCheckValidationBlack(const PositionData position, MoveWithPieceStruct* restrict result)
{
    const ulong takenFields = position.TakenFields;
    const ulong pieceColours = position.PieceColours;
	const ulong whitePieces = takenFields ^ pieceColours;
	ulong blackPieces = takenFields & pieceColours;

    ulong mask = MASK;
    int resultCount = 0;
    int shift;
    int candidate;

	int i;
	while (blackPieces)
	{
		i = TriglavChess_GetFirstSetBitIndex(blackPieces);
		mask = MASK << i;
		blackPieces &= ~mask;

		int row = CoordinatesGetRow(i);
		int column = CoordinatesGetColumn(i);

		switch (GetPiece(position.Pieces_0, position.Pieces_1, position.Pieces_2, mask, i))
		{
		case Pawn:
		{
			if (column <= 6)
			{
				candidate = GetSingleCoordinate(row + 1, column + 1);
				if (IsPieceColourWhite(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, row == 6 ? Queen : Pawn };
				}
			}

			if (column > 0)
			{
				candidate = GetSingleCoordinate(row + 1, column - 1);
				if (IsPieceColourWhite(whitePieces, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, row == 6 ? Queen : Pawn };
				}
			}

			candidate = GetSingleCoordinate(row + 1, column);
			if (!IsFieldTaken(takenFields, candidate))
			{
				*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, row == 6 ? Queen : Pawn };

				if (row == 1)
				{
					candidate = GetSingleCoordinate(row + 2, column);
					if (!IsFieldTaken(takenFields, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Pawn };
					}
				}
			}

			break;
		}
		case Rook:
		{
			shift = 1;
			while (row + shift <= 7)
			{
				candidate = GetSingleCoordinate(row + shift, column);
				if (!IsFieldTaken(takenFields, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Rook };
				}
				else
				{
					if (IsPieceColourWhite(whitePieces, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Rook };
					}
					break;
				}
				shift++;
			}

			shift = 1;
			while (column + shift <= 7)
			{
				candidate = GetSingleCoordinate(row, column + shift);
				if (!IsFieldTaken(takenFields, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Rook };
				}
				else
				{
					if (IsPieceColourWhite(whitePieces, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Rook };
					}
					break;
				}
				shift++;
			}

			shift = 1;
			while (row - shift >= 0)
			{
				candidate = GetSingleCoordinate(row - shift, column);
				if (!IsFieldTaken(takenFields, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Rook };
				}
				else
				{
					if (IsPieceColourWhite(whitePieces, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Rook };
					}
					break;
				}
				shift++;
			}

			shift = 1;
			while (column - shift >= 0)
			{
				candidate = GetSingleCoordinate(row, column - shift);
				if (!IsFieldTaken(takenFields, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Rook };
				}
				else
				{
					if (IsPieceColourWhite(whitePieces, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Rook };
					}
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
				candidate = GetSingleCoordinate(row + 2, column + 1);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Knight };
				}
			}

			if (row <= 5 && column > 0)
			{
				candidate = GetSingleCoordinate(row + 2, column - 1);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Knight };
				}
			}

			if (row <= 6 && column <= 5)
			{
				candidate = GetSingleCoordinate(row + 1, column + 2);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Knight };
				}
			}

			if (row <= 6 && column > 1)
			{
				candidate = GetSingleCoordinate(row + 1, column - 2);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Knight };
				}
			}

			if (row > 0 && column <= 5)
			{
				candidate = GetSingleCoordinate(row - 1, column + 2);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Knight };
				}
			}

			if (row > 0 && column > 1)
			{
				candidate = GetSingleCoordinate(row - 1, column - 2);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Knight };
				}
			}

			if (row > 1 && column <= 6)
			{
				candidate = GetSingleCoordinate(row - 2, column + 1);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Knight };
				}
			}

			if (row > 1 && column > 0)
			{
				candidate = GetSingleCoordinate(row - 2, column - 1);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Knight };
				}
			}

			break;
		}
		case Bishop:
		{
			if (row <= 5 && column <= 5)
			{
				candidate = GetSingleCoordinate(row + 2, column + 2);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Bishop };
				}
			}

			if (row <= 5 && column > 1)
			{
				candidate = GetSingleCoordinate(row + 2, column - 2);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Bishop };
				}
			}

			if (row > 1 && column <= 5)
			{
				candidate = GetSingleCoordinate(row - 2, column + 2);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Bishop };
				}
			}

			if (row > 1 && column > 1)
			{
				candidate = GetSingleCoordinate(row - 2, column - 2);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, Bishop };
				}
			}

			break;
		}
		case Queen:
		{
			// Promoted pawns do not have to wait for their king to move.
			if (i != BlackQueenStartingPosition || HasBlackKingMoved(position.Pieces_2, position.BlackKingLocation))
			{
				if (BlackQueenStartingPosition == i)
				{
					for (int j = 0; j < BlackQueenFirstMovesLength; j++)
					{
						if (IsFieldEmptyOrWhite(pieceColours, PieceMoves_BlackQueenFirstMoves[j]))
						{
							*(result + resultCount++) = (MoveWithPieceStruct){ {i, PieceMoves_BlackQueenFirstMoves[j]}, QueenMoved };
						}
					}

					break;
				}

				// Forward is off the board after promotion; no backward straight jump.
				if (column <= 5)
				{
					candidate = GetSingleCoordinate(row, column + 2);
					if (IsFieldEmptyOrWhite(pieceColours, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
					}
				}
				if (column > 1)
				{
					candidate = GetSingleCoordinate(row, column - 2);
					if (IsFieldEmptyOrWhite(pieceColours, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
					}
				}

				if (column <= 5)
				{
					candidate = GetSingleCoordinate(row - 2, column + 2);
					if (IsFieldEmptyOrWhite(pieceColours, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
					}
				}
				if (column > 1)
				{
					candidate = GetSingleCoordinate(row - 2, column - 2);
					if (IsFieldEmptyOrWhite(pieceColours, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
					}
				}

				if (column <= 6)
				{
					candidate = GetSingleCoordinate(row - 1, column + 1);
					if (IsFieldEmptyOrWhite(pieceColours, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
					}
				}
				if (column > 0)
				{
					candidate = GetSingleCoordinate(row - 1, column - 1);
					if (IsFieldEmptyOrWhite(pieceColours, candidate))
					{
						*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
					}
				}
			}

			break;
		}
		case QueenMoved:
		{
			if (row <= 6 && column <= 6)
			{
				candidate = GetSingleCoordinate(row + 1, column + 1);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
				}
			}

			if (row <= 6 && column > 0)
			{
				candidate = GetSingleCoordinate(row + 1, column - 1);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
				}
			}

			if (row > 0 && column <= 6)
			{
				candidate = GetSingleCoordinate(row - 1, column + 1);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
				}
			}

			if (row > 0 && column > 0)
			{
				candidate = GetSingleCoordinate(row - 1, column - 1);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, QueenMoved };
				}
			}

			break;
		}
		case King:
		{
			for (int j = 0; j < BlackKingFirstMovesLength; j++)
			{
				if (IsFieldEmptyOrWhite(pieceColours, PieceMoves_BlackKingFirstMoves[j]))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, PieceMoves_BlackKingFirstMoves[j]}, KingMoved };
				}
			}

			break;
		}
		case KingMoved:
		{
			if (row <= 6)
			{
				candidate = GetSingleCoordinate(row + 1, column);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, KingMoved };
				}
			}

			if (row <= 6 && column <= 6)
			{
				candidate = GetSingleCoordinate(row + 1, column + 1);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, KingMoved };
				}
			}

			if (row <= 6 && column > 0)
			{
				candidate = GetSingleCoordinate(row + 1, column - 1);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, KingMoved };
				}
			}

			if (column <= 6)
			{
				candidate = GetSingleCoordinate(row, column + 1);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, KingMoved };
				}
			}

			if (column > 0)
			{
				candidate = GetSingleCoordinate(row, column - 1);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, KingMoved };
				}
			}

			if (row > 0)
			{
				candidate = GetSingleCoordinate(row - 1, column);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, KingMoved };
				}
			}

			if (row > 0 && column <= 6)
			{
				candidate = GetSingleCoordinate(row - 1, column + 1);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, KingMoved };
				}
			}

			if (row > 0 && column > 0)
			{
				candidate = GetSingleCoordinate(row - 1, column - 1);
				if (IsFieldEmptyOrWhite(pieceColours, candidate))
				{
					*(result + resultCount++) = (MoveWithPieceStruct){ {i, candidate}, KingMoved };
				}
			}

			break;
		}
		}
	}

    return resultCount;
}
