#include "Util.h"
#include "InteropTypes.h"
#include <assert.h>
#include <intrin.h>

int TriglavChess_popcount(ulong x)
{
	//magic hardware function
	return __popcnt64(x);
	/*
    //magic (http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel)
    x = x - ((x >> 1) & 0x5555555555555555ULL);

    x = (x & 0x3333333333333333ULL) + ((x >> 2) & 0x3333333333333333ULL);

    x = (x + (x >> 4)) & 0x0F0F0F0F0F0F0F0FULL;

    return (x * 0x0101010101010101ULL) >> 56;*/
}

int8_t TriglavChess_GetSetBitIndex(ulong x)
{
#if defined(_DEBUG)
	assert(x == 0);
#endif

    unsigned long index;
    //Microsoft magic function
    (void)_BitScanForward64(&index, x);
    //we assume that there is always exactly 1 bit set
    return (int8_t)index;
}

int TriglavChess_GetFirstSetBitIndex(ulong x)
{
#if defined(_DEBUG)
	assert(x == 0);
#endif
	unsigned long index;
	//Microsoft magic function
	(void)_BitScanForward64(&index, x);

	return (int)index;
}

int TriglavChess_PositionsEqual(PositionData left, PositionData right)
{
    return (left.Hash == right.Hash)
        && (left.TakenFields == right.TakenFields)
        && (left.PieceColours == right.PieceColours)
        && (left.Pieces_0 == right.Pieces_0)
        && (left.Pieces_1 == right.Pieces_1)
        && (left.Pieces_2 == right.Pieces_2);
}

int TriglavChess_PositionKeysEqual(PositionKey left, PositionKey right)
{
    return (left.PieceColours == right.PieceColours)
        && (left.Pieces_0 == right.Pieces_0)
        && (left.Pieces_1 == right.Pieces_1)
        && (left.Pieces_2 == right.Pieces_2);
}
/*
int TriglavChess_PositionsEqual(PositionKey left, PositionData right)
{
    return (left.TakenFields == right.TakenFields)
        && (left.PieceColours == right.PieceColours)
        && (left.Pieces_0 == right.Pieces_0)
        && (left.Pieces_1 == right.Pieces_1)
        && (left.Pieces_2 == right.Pieces_2);
}*/

ulong TriglavChess_MurmurHash3Mixer(ulong key)
{
    key ^= (key >> 33);
    key *= 0xff51afd7ed558ccd;
    key ^= (key >> 33);
    key *= 0xc4ceb9fe1a85ec53;
    key ^= (key >> 33);

    return key;
}

MoveWithPieceStructPacked TriglavChess_ConvertMoveWithPieceToPacked(const MoveWithPieceStruct move)
{
    return (MoveWithPieceStructPacked) {(uint8_t)move.Move.From, (uint8_t)move.Move.To, (uint8_t)move.Piece};
}

MoveWithPieceStruct TriglavChess_ConvertMoveWithPieceToUnpacked(const MoveWithPieceStructPacked move)
{
    return (MoveWithPieceStruct) { (MoveStruct){ move.From, move.To }, move.Piece };
}

PositionKey TriglavChess_ConvertPositionDataToPositionKey(const PositionData position)
{
    return (PositionKey) { position.PieceColours, position.Pieces_0, position.Pieces_1, position.Pieces_2 };
}
