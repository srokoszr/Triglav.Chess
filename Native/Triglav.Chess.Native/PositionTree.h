#pragma once
#include "InteropTypes.h"
#include "PositionTreeNodeCache.h"

typedef struct PositionTree
{
	PositionTreeNode* Root;

	PositionTreeNodeCache* NodeCache;

	HashBucketPool* BucketPool;

	TreeNodeMovePool* NodesPool;
} PositionTree;

__declspec(dllexport) void TriglavChess_External_PositionTree_free(PositionTree tree);

PositionTreeNode* TriglavChess_PositionTree_CopySubtree(PositionTreeNode* restrict startingNode, const PositionData position, PositionTreeNodeCache* restrict nodeCache, HashBucketPool* restrict bucketPool, TreeNodeMovePool* restrict nodesPool, const Colours onMove, const int windowSizeDiff, const int maxDepth);

//maxDepth is not multiplied by 2
__declspec(dllexport) PositionTree TriglavChess_External_PositionTree_Build(const PositionData position, const int32_t onMove, const int32_t maxDepth);

//maxDepth is not multiplied by 2
__declspec(dllexport) PositionTree TriglavChess_External_PositionTree_Extend(PositionTree tree, PositionTreeNode* restrict startingNode, const PositionData position, const int32_t onMove, const int32_t maxDepth);

__declspec(dllexport) PositionTree TriglavChess_External_PositionTree_MinMaxBuild(const MinMaxArguments arguments);

__declspec(dllexport) PositionTree TriglavChess_External_PositionTree_MinMaxBuild_WithCopying(const MinMaxArguments arguments);

__declspec(dllexport) PositionTree TriglavChess_External_PositionTree_MinMaxExtend(const MinMaxArguments arguments, PositionTree tree, PositionTreeNode* restrict startingNode);

__declspec(dllexport) PositionTree TriglavChess_External_PositionTree_MinMaxExtendNoPrunning(const MinMaxArguments arguments, PositionTree tree, PositionTreeNode* restrict startingNode);
