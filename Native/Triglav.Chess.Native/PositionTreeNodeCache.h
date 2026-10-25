#pragma once
#include "ObjectPool.h"

PositionTreeNodeHashBucket* TriglavChess_PositionTreeNodeHashBucket_new(HashBucketPool* restrict bucketPool);
PositionTreeNodeHashTable* TriglavChess_PositionTreeNodeHashTable_new();
PositionTreeNodeCache* TriglavChess_PositionTreeNodeCache_new();

void TriglavChess_PositionTreeNodeHashBucket_free(PositionTreeNodeHashBucket* bucket);
void TriglavChess_PositionTreeNodeHashTable_free(PositionTreeNodeHashTable* hashTable);
void TriglavChess_PositionTreeNodeCache_free(PositionTreeNodeCache* cache);

//returns 1 when it was a Get, 0 when an Add
int TriglavChess_GetOrAddToCache(PositionTreeNodeHashTable* hashTable, PositionTreeNode** targetNodePtr, const PositionKey position, ulong hash, ulong depth, ulong onMove, HashBucketPool* restrict bucketPool);

