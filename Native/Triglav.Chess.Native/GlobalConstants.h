#pragma once

#define MASK 0x00000001ULL

#define ZERO 0x00000000ULL

#define PIECEMASK_0 0x00000004ULL

#define PIECEMASK_1 0x00000002ULL

#define PIECEMASK_2 0x00000001ULL

#define MaxMovesInPosition (14 * 2 + 8 * 2 + 4 * 2 + 8 + 10 + 7 * 8)

#define PreAllocatedBufferSize (42 * 4)

extern const PositionData POSITION_EMPTY;
extern const PositionKey POSITION_KEY_EMPTY;