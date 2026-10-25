#include "PositionTreeNodeCache.h"
#include "Util.h"

#include "malloc.h"
#include "ObjectPool.h"

PositionTreeNodeHashBucket* TriglavChess_PositionTreeNodeHashBucket_new(HashBucketPool* restrict bucketPool)
{
	PositionTreeNodeHashBucket* result = TriglavChess_GetNextHashBucketPointer(bucketPool);
	//result->Count = 0;
	result->Next = NULL;
	return result;
}

PositionTreeNodeHashTable* TriglavChess_PositionTreeNodeHashTable_new()
{
	PositionTreeNodeHashTable* result = calloc(1, sizeof(PositionTreeNodeHashTable));
	result->Buckets = calloc(NodeHashTableSize, sizeof(PositionTreeNodeHashBucket*));

	return result;
}

PositionTreeNodeCache* TriglavChess_PositionTreeNodeCache_new()
{
	PositionTreeNodeHashTable* cache = TriglavChess_PositionTreeNodeHashTable_new();

	PositionTreeNodeCache* result = malloc(sizeof(PositionTreeNodeCache));
	*result = (PositionTreeNodeCache) { cache };

	return result;
}

void TriglavChess_PositionTreeNodeHashBucket_free(PositionTreeNodeHashBucket* bucket)
{
	int first = 1;
	PositionTreeNodeHashBucket* tmp;
	while (bucket)
	{
		/*if (bucket->Items)
		{
			free(bucket->Items);
		}*/
		tmp = bucket->Next;
		if (first)
		{
			free(bucket);
			first = 0;
		}
		bucket = tmp;
	}
}

void TriglavChess_PositionTreeNodeHashTable_free(PositionTreeNodeHashTable* hashTable)
{
	if (hashTable)
	{
		//underlying buckets are owned by the pool
		free(hashTable->Buckets);
		free(hashTable);
	}
}

void TriglavChess_PositionTreeNodeCache_free(PositionTreeNodeCache* cache)
{
	if (cache)
	{
		TriglavChess_PositionTreeNodeHashTable_free(cache->Hashtable);

		free(cache);
	}
}

int TriglavChess_GetOrAddToCache(PositionTreeNodeHashTable* hashTable, PositionTreeNode** targetNodePtr, const PositionKey position, ulong hash, ulong depth, ulong onMove, HashBucketPool* restrict bucketPool)
{
	PositionTreeNode* targetNode;
	hash = ((TriglavChess_MurmurHash3Mixer(hash)) & NodeHashTableMask) | (NodeHashTableColourMask * onMove);

	hashTable->OperationsCount++;
	PositionTreeNodeHashBucket* bucket = hashTable->Buckets[hash];
	if (!bucket)
	{
		hashTable->UsedBuckets++;
		hashTable->UniqueItems++;
		bucket = TriglavChess_PositionTreeNodeHashBucket_new(bucketPool);
		hashTable->Buckets[hash] = bucket;

		targetNode = bucket->Item;
		targetNode->Position = position;
		targetNode->Children = NULL;
		targetNode->AnalysedMovesCount = 0;
		targetNode->LegalMovesCount = 0;
		targetNode->AvailableMovesCount = 0;
		targetNode->Evaluation = (Evaluation){ 0,0,0 };

		*targetNodePtr = targetNode;
		return 0;
	}

	while (1)
	{
		if (TriglavChess_PositionKeysEqual(bucket->Item->Position, position))
		{
			targetNode = bucket->Item;
			//found cached entry
			*targetNodePtr = targetNode;
			return 1;
		}

		if (bucket->Next)
		{
			//move to next bucket
			bucket = bucket->Next;
		}
		else
		{
			//add new bucket
			bucket->Next = TriglavChess_PositionTreeNodeHashBucket_new(bucketPool);
			bucket = bucket->Next;
			break;
		}
	}

	hashTable->UniqueItems++;
	targetNode = bucket->Item;
	targetNode->Position = position;
	targetNode->Children = NULL;
	targetNode->AnalysedMovesCount = 0;
	targetNode->LegalMovesCount = 0;
	targetNode->AvailableMovesCount = 0;
	targetNode->Evaluation = (Evaluation){ 0,0,0 };

	*targetNodePtr = targetNode;
	return 0;
}
