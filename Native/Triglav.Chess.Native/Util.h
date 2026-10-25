#pragma once
#include "TypeAliases.h"
#include "InteropTypes.h"

int TriglavChess_popcount(ulong x);

int8_t TriglavChess_GetSetBitIndex(ulong x);
int TriglavChess_GetFirstSetBitIndex(ulong x);

int TriglavChess_PositionsEqual(const PositionData left, const PositionData right);
int TriglavChess_PositionKeysEqual(const PositionKey left, const PositionKey right);
//int TriglavChess_PositionsEqual(const PositionKey left, const PositionData right);

//https://zimbry.blogspot.com/2011/09/better-bit-mixing-improving-on.html
ulong TriglavChess_MurmurHash3Mixer(ulong key);

MoveWithPieceStructPacked TriglavChess_ConvertMoveWithPieceToPacked(const MoveWithPieceStruct move);
MoveWithPieceStruct TriglavChess_ConvertMoveWithPieceToUnpacked(const MoveWithPieceStructPacked move);

PositionKey TriglavChess_ConvertPositionDataToPositionKey(const PositionData position);