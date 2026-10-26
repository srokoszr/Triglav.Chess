#include "InteropTypes.h"
#include "GlobalConstants.h"
#include "PieceMovesPrecomputed.h"
#include "PieceMoves.h"



const int PieceMoves_WhiteKingFirstMoves[WhiteKingFirstMovesLength] = {
			D1,
			D2,
			E2,
			F2,
			F1,

			C1,
			C2,
			C3,
			D3,
			E3,
			F3,
			G3,
			G2,
			G1,
};
const int PieceMoves_BlackKingFirstMoves[BlackKingFirstMovesLength] = {
			D8,
			D7,
			E7,
			F7,
			F8,

			C8,
			C7,
			C6,
			D6,
			E6,
			F6,
			G6,
			G7,
			G8,
};
const int PieceMoves_WhiteQueenFirstMoves[WhiteQueenFirstMovesLength] =
{
	C2,
	E2,

	B1,
	B3,
	D3,
	F3,
	F1,
	/*
	"c1",
	"d2",
	"e1",
	*/
};
const int PieceMoves_BlackQueenFirstMoves[BlackQueenFirstMovesLength] =
{
	C7,
	E7,

	B8,
	B6,
	D6,
	F6,
	F8,
	/*
	"d8",
	"e7",
	"f8",
	*/
};



const ulong PieceMovesAttackedFields_WhiteKingFirstMoves =
(MASK << D1) |
(MASK << D2) |
(MASK << E2) |
(MASK << F2) |
(MASK << F1) |

(MASK << C1) |
(MASK << C2) |
(MASK << C3) |
(MASK << D3) |
(MASK << E3) |
(MASK << F3) |
(MASK << G3) |
(MASK << G2) |
(MASK << G1);

const ulong PieceMovesAttackedFields_BlackKingFirstMoves =
(MASK << D8) |
(MASK << D7) |
(MASK << E7) |
(MASK << F7) |
(MASK << F8) |

(MASK << C8) |
(MASK << C7) |
(MASK << C6) |
(MASK << D6) |
(MASK << E6) |
(MASK << F6) |
(MASK << G6) |
(MASK << G7) |
(MASK << G8);

const ulong PieceMovesAttackedFields_WhiteQueenFirstMoves =
(MASK << C2) |
(MASK << E2) |

(MASK << B1) |
(MASK << B3) |
(MASK << D3) |
(MASK << F3) |
(MASK << F1)
/*
"c1" |
"d2" |
"e1" |
*/
;

const ulong PieceMovesAttackedFields_BlackQueenFirstMoves =
(MASK << C7) |
(MASK << E7) |

(MASK << B8) |
(MASK << B6) |
(MASK << D6) |
(MASK << F6) |
(MASK << F8)
/*
"d8" |
"e7" |
"f8" |
*/
;


ulong PieceMovesAttackedFields_WhitePawn[64];
ulong PieceMovesAttackedFields_BlackPawn[64];

ulong PieceMovesAttackedFields_WhiteBishop[64];
ulong PieceMovesAttackedFields_BlackBishop[64];

ulong PieceMovesAttackedFields_WhiteKnight[64];
ulong PieceMovesAttackedFields_BlackKnight[64];

ulong PieceMovesAttackedFields_WhiteKing[64];
ulong PieceMovesAttackedFields_BlackKing[64];

ulong PieceMovesAttackedFields_WhiteQueen[64];
ulong PieceMovesAttackedFields_BlackQueen[64];

void TriglavChess_External_PrecomputedMoves_Initialise(void)
{
	ulong mask = MASK;

	for (int i = 0; i < 64; i++)
	{
		mask = MASK << i;

		PieceMovesAttackedFields_WhitePawn[i] = TriglavChess_GetAttackedFieldsWhiteForPrecompute(0, 0, Pawn, CoordinatesGetRow(i), CoordinatesGetColumn(i), i);
		PieceMovesAttackedFields_BlackPawn[i] = TriglavChess_GetAttackedFieldsBlackForPrecompute(0, 0, Pawn, CoordinatesGetRow(i), CoordinatesGetColumn(i), i);

		PieceMovesAttackedFields_WhiteBishop[i] = TriglavChess_GetAttackedFieldsWhiteForPrecompute(0, 0, Bishop, CoordinatesGetRow(i), CoordinatesGetColumn(i), i);
		PieceMovesAttackedFields_BlackBishop[i] = TriglavChess_GetAttackedFieldsBlackForPrecompute(0, 0, Bishop, CoordinatesGetRow(i), CoordinatesGetColumn(i), i);

		PieceMovesAttackedFields_WhiteKnight[i] = TriglavChess_GetAttackedFieldsWhiteForPrecompute(0, 0, Knight, CoordinatesGetRow(i), CoordinatesGetColumn(i), i);
		PieceMovesAttackedFields_BlackKnight[i] = TriglavChess_GetAttackedFieldsBlackForPrecompute(0, 0, Knight, CoordinatesGetRow(i), CoordinatesGetColumn(i), i);

		PieceMovesAttackedFields_WhiteKing[i] = TriglavChess_GetAttackedFieldsWhiteForPrecompute(0, 0, KingMoved, CoordinatesGetRow(i), CoordinatesGetColumn(i), i);
		PieceMovesAttackedFields_BlackKing[i] = TriglavChess_GetAttackedFieldsBlackForPrecompute(0, 0, KingMoved, CoordinatesGetRow(i), CoordinatesGetColumn(i), i);

		PieceMovesAttackedFields_WhiteQueen[i] = TriglavChess_GetAttackedFieldsWhiteForPrecompute(0, 0, QueenMoved, CoordinatesGetRow(i), CoordinatesGetColumn(i), i);
		PieceMovesAttackedFields_BlackQueen[i] = TriglavChess_GetAttackedFieldsBlackForPrecompute(0, 0, QueenMoved, CoordinatesGetRow(i), CoordinatesGetColumn(i), i);
	}
}
