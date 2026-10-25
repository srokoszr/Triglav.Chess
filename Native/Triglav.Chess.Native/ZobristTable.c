#include "ZobristTable.h"

ulong ZOBRIST_TABLE[PIECES_N][SQUARE_N];

ulong* TriglavChess_External_GetZobristTablePointer()
{
    return &ZOBRIST_TABLE[0][0];
}

ulong TriglavChess_GetZobrist(Pieces piece, Colours colour, ulong location)
{
    return ZOBRIST_TABLE[piece | (colour << 3)][location];
}
