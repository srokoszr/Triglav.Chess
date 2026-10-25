#pragma once
#include "PositionTreeNodeCacheTypes.h"

//2^16 = 65 536
#define POOL_BUFFER_SIZE 0x00010000
#define BUFFERS_GROWTH_SIZE 64

typedef struct HashBucketPool
{
	PositionTreeNodeHashBucket** Buffers;

	PositionTreeNodeHashBucket* LastBufferNextElementPointer;

	int BuffersCount;

	int CurrentBuffer;

	int LastBufferNextElementOffset;
} HashBucketPool;

typedef struct TreeNodeMovePool
{
	PositionTreeNode*** Buffers;

	PositionTreeNode** LastBufferNextElementPointer;

	int BuffersCount;

	int CurrentBuffer;

	int LastBufferNextElementOffset;

	int ReturnedNodes;
} TreeNodeMovePool;

HashBucketPool* TriglavChess_HashBucketPool_new();
void TriglavChess_HashBucketPool_free(HashBucketPool* restrict pool);

PositionTreeNodeHashBucket* TriglavChess_GetNextHashBucketPointer(HashBucketPool* restrict pool);

TreeNodeMovePool* TriglavChess_TreeNodeMovePool_new();
void TriglavChess_TreeNodeMovePool_free(TreeNodeMovePool* restrict pool);

PositionTreeNode** TriglavChess_GetNextTreeNodeMoveChunk(TreeNodeMovePool* restrict pool, int chunkSize);