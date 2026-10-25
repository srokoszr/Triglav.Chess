#pragma once
#include "TypeAliases.h"
#include "Enumerations.h"

//8 pieces * two colours
#define PIECES_N 16
//full board
#define SQUARE_N 64

extern ulong ZOBRIST_TABLE[PIECES_N][SQUARE_N];

__declspec(dllexport) ulong* TriglavChess_External_GetZobristTablePointer();

ulong TriglavChess_GetZobrist(Pieces piece, Colours colour, ulong location);