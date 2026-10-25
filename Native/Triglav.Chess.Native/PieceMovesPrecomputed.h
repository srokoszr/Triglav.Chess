#pragma once


#define WhiteKingFirstMovesLength 14
#define BlackKingFirstMovesLength 14

#define WhiteQueenFirstMovesLength 7
#define BlackQueenFirstMovesLength 7

extern const int PieceMoves_WhiteKingFirstMoves[WhiteKingFirstMovesLength];
extern const int PieceMoves_BlackKingFirstMoves[BlackKingFirstMovesLength];

extern const int PieceMoves_WhiteQueenFirstMoves[WhiteQueenFirstMovesLength];
extern const int PieceMoves_BlackQueenFirstMoves[BlackQueenFirstMovesLength];

extern const ulong PieceMovesAttackedFields_WhiteKingFirstMoves;
extern const ulong PieceMovesAttackedFields_BlackKingFirstMoves;
extern const ulong PieceMovesAttackedFields_WhiteQueenFirstMoves;
extern const ulong PieceMovesAttackedFields_BlackQueenFirstMoves;

extern ulong PieceMovesAttackedFields_WhitePawn[64];
extern ulong PieceMovesAttackedFields_BlackPawn[64];

extern ulong PieceMovesAttackedFields_WhiteBishop[64];
extern ulong PieceMovesAttackedFields_BlackBishop[64];

extern ulong PieceMovesAttackedFields_WhiteKnight[64];
extern ulong PieceMovesAttackedFields_BlackKnight[64];

extern ulong PieceMovesAttackedFields_WhiteKing[64];
extern ulong PieceMovesAttackedFields_BlackKing[64];

extern ulong PieceMovesAttackedFields_WhiteQueen[64];
extern ulong PieceMovesAttackedFields_BlackQueen[64];

__declspec(dllexport) void TriglavChess_External_PrecomputedMoves_Initialise(void);