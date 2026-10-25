#pragma once
#include "PositionTreeTypes.h"
/*
#define NodeHashBucketItemsSize 2
//2^20 = 1 048 576
#define NodeHashTableSize 0x00100000
//2^20 - 1 = 1 048 575
#define NodeHashTablMask 0x000FFFFFUL*/
/*#define NodeHashBucketItemsSize 2
//2^24 = 16 777 216
#define NodeHashTableSize 0x01000000
//2^24 - 1 = 16 777 215
#define NodeHashTablMask 0x00FFFFFFUL*/

#define NodeHashBucketItemsSize 1
//2^26 = 67 108 864
#define NodeHashTableSize 0x08000000
//2^26 - 1 = 67 108 863
#define NodeHashTableMask 0x03FFFFFFUL

#define NodeHashTableColourMask 0x04000000UL

typedef struct PositionTreeNodeHashBucket
{
	//fixed size array of nodes
	PositionTreeNode Item[NodeHashBucketItemsSize];

	struct PositionTreeNodeHashBucket* Next;
} PositionTreeNodeHashBucket;

typedef struct PositionTreeNodeHashTable
{
	PositionTreeNodeHashBucket** Buckets;

	int OperationsCount;

	int UniqueItems;

	int UsedBuckets;
} PositionTreeNodeHashTable;

typedef struct PositionTreeNodeCache
{
	PositionTreeNodeHashTable* Hashtable;
} PositionTreeNodeCache;