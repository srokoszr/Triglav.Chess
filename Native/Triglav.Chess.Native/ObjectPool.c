#include "ObjectPool.h"

#include "malloc.h"

static void HashBucketPool_resize(HashBucketPool* restrict pool)
{
	pool->Buffers = realloc(pool->Buffers, (pool->BuffersCount + BUFFERS_GROWTH_SIZE) * sizeof(PositionTreeNodeHashBucket*));
	pool->BuffersCount += BUFFERS_GROWTH_SIZE;
	for (int i = pool->CurrentBuffer; i < pool->BuffersCount; i++)
	{
		pool->Buffers[i] = malloc(sizeof(PositionTreeNodeHashBucket) * POOL_BUFFER_SIZE);
	}
}

HashBucketPool* TriglavChess_HashBucketPool_new()
{
	HashBucketPool* result = calloc(1, sizeof(HashBucketPool));

	HashBucketPool_resize(result);

	result->LastBufferNextElementPointer = &result->Buffers[0][0];

	return result;
}

void TriglavChess_HashBucketPool_free(HashBucketPool* restrict pool)
{
	if (pool)
	{
		for (int i = 0; i < pool->BuffersCount; i++)
		{
			PositionTreeNodeHashBucket* buffer = pool->Buffers[i];
			free(buffer);
		}

		free(pool->Buffers);
		free(pool);
	}
}

PositionTreeNodeHashBucket* TriglavChess_GetNextHashBucketPointer(HashBucketPool* restrict pool)
{
	if (pool->LastBufferNextElementOffset < POOL_BUFFER_SIZE)
	{
		pool->LastBufferNextElementOffset++;
		return pool->LastBufferNextElementPointer++;
	}

	//advance to next Buffer. Allocate more buffers if we reached end
	if ((++pool->CurrentBuffer) == pool->BuffersCount)
	{
		HashBucketPool_resize(pool);
	}

	pool->LastBufferNextElementOffset = 1;
	pool->LastBufferNextElementPointer = pool->Buffers[pool->CurrentBuffer];

	return pool->LastBufferNextElementPointer++;
}

static void TreeNodeMovePool_resize(TreeNodeMovePool* restrict pool)
{
	pool->Buffers = realloc(pool->Buffers, (pool->BuffersCount + BUFFERS_GROWTH_SIZE) * sizeof(PositionTreeNode**));
	pool->BuffersCount += BUFFERS_GROWTH_SIZE;
	for (int i = pool->CurrentBuffer; i < pool->BuffersCount; i++)
	{
		pool->Buffers[i] = malloc(sizeof(PositionTreeNode*) * POOL_BUFFER_SIZE);
	}
}

TreeNodeMovePool* TriglavChess_TreeNodeMovePool_new()
{
	TreeNodeMovePool* result = calloc(1, sizeof(TreeNodeMovePool));

	TreeNodeMovePool_resize(result);

	result->LastBufferNextElementPointer = &result->Buffers[0][0];

	return result;
}

void TriglavChess_TreeNodeMovePool_free(TreeNodeMovePool* restrict pool)
{
	if (pool)
	{
		for (int i = 0; i < pool->BuffersCount; i++)
		{
			PositionTreeNode** buffer = pool->Buffers[i];
			free(buffer);
		}

		free(pool->Buffers);
		free(pool);
	}
}

PositionTreeNode** TriglavChess_GetNextTreeNodeMoveChunk(TreeNodeMovePool* restrict pool, int chunkSize)
{
	PositionTreeNode** result = NULL;
	if (!chunkSize)
	{
		return result;
	}

	if ((pool->LastBufferNextElementOffset + chunkSize) < POOL_BUFFER_SIZE)
	{
		result = pool->LastBufferNextElementPointer;
		pool->LastBufferNextElementOffset += chunkSize;
		pool->LastBufferNextElementPointer += chunkSize;
		pool->ReturnedNodes += chunkSize;
		return result;
	}

	//advance to next Buffer. Allocate more buffers if we reached end
	if ((++pool->CurrentBuffer) == pool->BuffersCount)
	{
		TreeNodeMovePool_resize(pool);
	}

	pool->LastBufferNextElementOffset = chunkSize;
	result = pool->LastBufferNextElementPointer = pool->Buffers[pool->CurrentBuffer];
	pool->LastBufferNextElementPointer += chunkSize;
	pool->ReturnedNodes += chunkSize;

	return result;
}
