#include "PositionTree.h"
#include "PositionTreeTypes.h"
#include "MovesEnumerator.h"
#include "GlobalConstants.h"
#include "Util.h"
#include "MinMax.h"
#include "PositionEvaluator.h"
#include "InteropInterface.h"
#include "Evaluation.h"
#include "PositionStack.h"

#include "malloc.h"
#include "stdio.h"

#define DeriveBufferPointer(treeBuildingData, currentDepth) (treeBuildingData->Buffers + (currentDepth * MaxMovesInPosition))

#define SpecialChildrenValueForRecursion -1

static const int TargetWidth = 7;

typedef struct TreeBuildingData 
{
	PositionTreeNodeCache* restrict NodeCache;

	MoveSimulation* restrict Buffers;

	HashBucketPool* restrict BucketPool; 

	TreeNodeMovePool* restrict NodesPool;
} TreeBuildingData;

typedef struct MinMaxTreeBuildingData
{
	PositionTreeNodeCache* restrict NodeCache;

	MoveSimulation* restrict Buffers;

	MoveSimulation* restrict ReusableBuffer;

	HashBucketPool* restrict BucketPool;

	TreeNodeMovePool* restrict NodesPool;

	EvaluationModifiersData EvaluationModifiers;

	Evaluation BestEvaluationSoFarForWhite;

	Evaluation BestEvaluationSoFarForBlack;

	PositionStack* Stack;
} MinMaxTreeBuildingData;

static void BuildSubtree(TreeBuildingData* restrict treeBuildingData, PositionTreeNode* restrict parent, const PositionData position,
	const Colours onMove, const int maxDepth, const int currentDepth);

static Evaluation BuildSubtreeMinMax(MinMaxTreeBuildingData* restrict treeBuildingData, PositionTreeNode* restrict parent, const MoveFlags moveFlags, const PositionData position,
	const Colours onMove, const int maxDepth, const int currentDepth, Evaluation alpha, Evaluation beta);

static void CopySubtree(PositionTreeNode* restrict targetNode, PositionTreeNode* restrict sourceNode, const PositionData position, PositionTreeNodeCache* restrict nodeCache, HashBucketPool* restrict bucketPool, TreeNodeMovePool* restrict nodesPool, const Colours onMove, const int depth, const int windowSizeDiff, const int maxDepth);

void TriglavChess_External_PositionTree_free(PositionTree tree)
{
	if (tree.NodeCache)
	{
		TriglavChess_PositionTreeNodeCache_free(tree.NodeCache);
	}
	if (tree.BucketPool)
	{
		TriglavChess_HashBucketPool_free(tree.BucketPool);
	}
	if (tree.NodesPool)
	{
		TriglavChess_TreeNodeMovePool_free(tree.NodesPool);
	}
}

PositionTreeNode* TriglavChess_PositionTree_CopySubtree(PositionTreeNode* restrict startingNode, const PositionData position, PositionTreeNodeCache* restrict nodeCache, HashBucketPool* restrict bucketPool, TreeNodeMovePool* restrict nodesPool, const Colours onMove, const int windowSizeDiff, const int maxDepth)
{
	PositionTreeNode* root = NULL;
	TriglavChess_GetOrAddToCache(nodeCache->Hashtable, &root, TriglavChess_ConvertPositionDataToPositionKey(position), position.Hash, 0, onMove, bucketPool);
	
	CopySubtree(root, startingNode, position, nodeCache, bucketPool, nodesPool, onMove, 0, windowSizeDiff, maxDepth);
	root->Evaluation = startingNode->Evaluation;
	return root;
}


PositionTree TriglavChess_External_PositionTree_Build(const PositionData position, const int32_t onMove, const int32_t maxDepth)
{
	//allow odd values
	//maxDepth *= 2;//half-moves
	PositionTreeNodeCache* nodeCache = TriglavChess_PositionTreeNodeCache_new(maxDepth);
	HashBucketPool* bucketPool = TriglavChess_HashBucketPool_new();
	TreeNodeMovePool* nodesPool = TriglavChess_TreeNodeMovePool_new();

	MoveSimulation* buffers = (MoveSimulation*)malloc(sizeof(MoveSimulation) * MaxMovesInPosition * maxDepth);

	PositionTreeNode* root = NULL;
	TriglavChess_GetOrAddToCache(nodeCache->Hashtable, &root, TriglavChess_ConvertPositionDataToPositionKey(position), position.Hash, 0, onMove, bucketPool);
	
	root->Position = TriglavChess_ConvertPositionDataToPositionKey(position);
	TreeBuildingData treeBuildingData = { nodeCache, buffers, bucketPool, nodesPool };

	BuildSubtree(&treeBuildingData, root, position, (Colours)onMove, maxDepth - 1, 0);

	free(buffers);

	return (PositionTree){ root, nodeCache, bucketPool, nodesPool };
}

PositionTree TriglavChess_External_PositionTree_Extend(PositionTree tree, PositionTreeNode* restrict startingNode, const PositionData position, const int32_t onMove, const int32_t maxDepth)
{
	MoveSimulation* buffers = (MoveSimulation*)malloc(sizeof(MoveSimulation) * MaxMovesInPosition * maxDepth);

	TreeBuildingData treeBuildingData = { tree.NodeCache, buffers, tree.BucketPool, tree.NodesPool };
	BuildSubtree(&treeBuildingData, startingNode, position, (Colours)onMove, maxDepth - 1, 0);

	free(buffers);
	return tree;
}

PositionTree TriglavChess_External_PositionTree_MinMaxBuild(const MinMaxArguments arguments)
{
	int maxDepth = (arguments.Depth * 2) + 1;

	PositionTreeNodeCache* nodeCache = TriglavChess_PositionTreeNodeCache_new(maxDepth);
	HashBucketPool* bucketPool = TriglavChess_HashBucketPool_new();
	TreeNodeMovePool* nodesPool = TriglavChess_TreeNodeMovePool_new();

	MoveSimulation* buffers = (MoveSimulation*)malloc(sizeof(MoveSimulation) * MaxMovesInPosition * maxDepth);
	MoveSimulation* reusableBuffer = (MoveSimulation*)malloc(sizeof(MoveSimulation) * MaxMovesInPosition * 4);

	PositionTreeNode* root = NULL;
	TriglavChess_GetOrAddToCache(nodeCache->Hashtable, &root, TriglavChess_ConvertPositionDataToPositionKey(arguments.Position), arguments.Position.Hash, 0, arguments.OnMove, bucketPool);

	root->Position = TriglavChess_ConvertPositionDataToPositionKey(arguments.Position);
	MinMaxTreeBuildingData treeBuildingData = { nodeCache, buffers, reusableBuffer, bucketPool, nodesPool, arguments.EvaluationModifiers, MinEvaluationForWhite, MaxEvaluationForBlack, arguments.Stack };

	TriglavChess_PositionStack_EnsureCapacity(arguments.Stack, maxDepth);
	if (arguments.Depth <= 100)
	{
		Evaluation result = BuildSubtreeMinMax(&treeBuildingData, root, IsCapture | IsCheck, arguments.Position, (Colours)arguments.OnMove, maxDepth - 1, 0, MinEvaluationForWhite, MaxEvaluationForBlack);
	}
	else
	{
		for (int i = 6; i < maxDepth; i += 2)
		{
			treeBuildingData = (MinMaxTreeBuildingData){ nodeCache, buffers, reusableBuffer, bucketPool, nodesPool, arguments.EvaluationModifiers, MinEvaluationForWhite, MaxEvaluationForBlack, arguments.Stack };
			Evaluation result = BuildSubtreeMinMax(&treeBuildingData, root, IsCapture | IsCheck, arguments.Position, (Colours)arguments.OnMove, i, 0, MinEvaluationForWhite, MaxEvaluationForBlack);
		}
	}

	free(buffers);
	free(reusableBuffer);
	/*printf("Returning tree with Evaluation Result: %d, EvaluationValue: %f, InNMoves: %d\n",
		root->Evaluation.Result, root->Evaluation.EvaluationValue, root->Evaluation.InNMoves);*/

	return (PositionTree) { root, nodeCache, bucketPool, nodesPool };
}

PositionTree TriglavChess_External_PositionTree_MinMaxBuild_WithCopying(const MinMaxArguments arguments)
{
	int maxDepth = (arguments.Depth * 2) + 1;
	PositionTreeNodeCache* nodeCache = TriglavChess_PositionTreeNodeCache_new(maxDepth);
	HashBucketPool* bucketPool = TriglavChess_HashBucketPool_new();
	TreeNodeMovePool* nodesPool = TriglavChess_TreeNodeMovePool_new();

	MoveSimulation* buffers = (MoveSimulation*)malloc(sizeof(MoveSimulation) * MaxMovesInPosition * maxDepth);
	MoveSimulation* reusableBuffer = (MoveSimulation*)malloc(sizeof(MoveSimulation) * MaxMovesInPosition * 4);

	PositionTreeNode* root = NULL;
	TriglavChess_GetOrAddToCache(nodeCache->Hashtable, &root, TriglavChess_ConvertPositionDataToPositionKey(arguments.Position), arguments.Position.Hash, 0, arguments.OnMove, bucketPool);

	root->Position = TriglavChess_ConvertPositionDataToPositionKey(arguments.Position);
	MinMaxTreeBuildingData treeBuildingData = { nodeCache, buffers, reusableBuffer, bucketPool, nodesPool, arguments.EvaluationModifiers, MinEvaluationForWhite, MaxEvaluationForBlack, arguments.Stack };
	
	TriglavChess_PositionStack_EnsureCapacity(arguments.Stack, maxDepth);
	if (arguments.Depth <= 3)
	{
		Evaluation result = BuildSubtreeMinMax(&treeBuildingData, root, IsCapture | IsCheck, arguments.Position, (Colours)arguments.OnMove, maxDepth - 1, 0, MinEvaluationForWhite, MaxEvaluationForBlack);
	}
	else
	{
		int windowSize = (maxDepth - 1) - 4;//TODO smarter heuristic
		
		int i_start = windowSize;
		if (i_start > 8)
		{
			i_start = 8;
		}
		for (int i = 2; i < maxDepth; i += 2)
		{
			treeBuildingData = (MinMaxTreeBuildingData){ nodeCache, buffers, reusableBuffer, bucketPool, nodesPool, arguments.EvaluationModifiers, MinEvaluationForWhite, MaxEvaluationForBlack, arguments.Stack };
			Evaluation result = BuildSubtreeMinMax(&treeBuildingData, root, IsCapture | IsCheck, arguments.Position, (Colours)arguments.OnMove, i, 0, MinEvaluationForWhite, MaxEvaluationForBlack);
			if (IsDecisive(result))
			{
				//we already found a winning sequence for either of sides
				break;
			}

			if (i >= i_start)
			{
				PositionTreeNodeCache* tmpNodeCache = TriglavChess_PositionTreeNodeCache_new(maxDepth);
				HashBucketPool* tmpBucketPool = TriglavChess_HashBucketPool_new();
				TreeNodeMovePool* tmpNodesPool = TriglavChess_TreeNodeMovePool_new();
				int windowSizeDiff = i - windowSize;
				if (windowSizeDiff < 0)
				{
					windowSizeDiff = 0;
				}
				root = TriglavChess_PositionTree_CopySubtree(root, arguments.Position, tmpNodeCache, tmpBucketPool, tmpNodesPool, arguments.OnMove, windowSizeDiff, maxDepth);
				TriglavChess_PositionTreeNodeCache_free(nodeCache);
				TriglavChess_HashBucketPool_free(bucketPool);
				TriglavChess_TreeNodeMovePool_free(nodesPool);
				nodeCache = tmpNodeCache;
				bucketPool = tmpBucketPool;
				nodesPool = tmpNodesPool;
			}
		}
	}

	free(buffers);
	free(reusableBuffer);

	return (PositionTree) { root, nodeCache, bucketPool, nodesPool };
}

PositionTree TriglavChess_External_PositionTree_MinMaxExtend(const MinMaxArguments arguments, PositionTree tree, PositionTreeNode* restrict startingNode)
{
	return tree;
}

PositionTree TriglavChess_External_PositionTree_MinMaxExtendNoPrunning(const MinMaxArguments arguments, PositionTree tree, PositionTreeNode* restrict startingNode)
{
	return tree;
}

static void CopySubtree(PositionTreeNode* restrict targetNode, PositionTreeNode* restrict sourceNode, const PositionData position, PositionTreeNodeCache* restrict nodeCache, HashBucketPool* restrict bucketPool, TreeNodeMovePool* restrict nodesPool, const Colours onMove, const int depth, const int windowSizeDiff, const int maxDepth)
{
	if (NULL == targetNode || NULL == sourceNode)
	{
		return;
	}

	targetNode->Evaluation = sourceNode->Evaluation;
	//don't copy MinMax evaluations
	targetNode->Position = sourceNode->Position;
	if (sourceNode->AnalysedMovesCount == 0 || depth == maxDepth)
	{
		return;
	}

	int childrenLength = sourceNode->AnalysedMovesCount;
	if (depth < windowSizeDiff)
	{
		if (childrenLength <= TargetWidth)
		{
			childrenLength = childrenLength;
		}
		else if (childrenLength <= (TargetWidth * 2))
		{
			childrenLength = TargetWidth;
		}
		else
		{
			childrenLength = (childrenLength + 1) / 2;
		}
	}

	int childrenCapacity = sourceNode->LegalMovesCount;
	if (childrenCapacity < childrenLength)
	{
		childrenCapacity = childrenLength;
	}

	//Reserve buffer for all legal moves, but copy only the analysed, width-trimmed prefix due to optimisations.
	targetNode->AvailableMovesCount = childrenLength;
	targetNode->AnalysedMovesCount = sourceNode->AnalysedMovesCount < childrenLength ? sourceNode->AnalysedMovesCount : childrenLength;
	targetNode->LegalMovesCount = childrenCapacity;
	targetNode->Children = TriglavChess_GetNextTreeNodeMoveChunk(nodesPool, childrenCapacity);
	for (int i = childrenLength; i < childrenCapacity; i++)
	{
		targetNode->Children[i] = NULL;
	}

	for (int i = 0; i < childrenLength; i++)
	{
		MoveSimulation move = TriglavChess_External_MoveBasedOnTargetPositionKey(position, sourceNode->Children[i]->Position, onMove);
		PositionData simulatedPosition = move.Position;

		//TODO smarter way to prevent list of children to be reorganised multiple times at different levels
		if (TriglavChess_GetOrAddToCache(nodeCache->Hashtable, targetNode->Children + i, TriglavChess_ConvertPositionDataToPositionKey(simulatedPosition), simulatedPosition.Hash, depth, onMove, bucketPool))
		{
			//position already encountered and cached in hashtable
			continue;
		}
		else
		{
			//new position
			CopySubtree(targetNode->Children[i], sourceNode->Children[i], simulatedPosition, nodeCache, bucketPool, nodesPool, !onMove, depth + 1, windowSizeDiff, maxDepth);
		}
	}
}

static void BuildSubtree(TreeBuildingData* restrict treeBuildingData, PositionTreeNode* restrict parent, const PositionData position,
	const Colours onMove, const int maxDepth, const int currentDepth)
{
	MoveSimulation* moves = treeBuildingData->Buffers + (currentDepth * MaxMovesInPosition);
	int32_t movesCount = TriglavChess_GetMovesNoFlags(position, moves, onMove);

	parent->AnalysedMovesCount = movesCount;
	parent->LegalMovesCount = movesCount;
	parent->AvailableMovesCount = movesCount;

	parent->Children = TriglavChess_GetNextTreeNodeMoveChunk(treeBuildingData->NodesPool, movesCount);
	if (parent->Children)
	{
		for (int i = 0; i < movesCount; i++)
		{
			if (TriglavChess_GetOrAddToCache(treeBuildingData->NodeCache->Hashtable, parent->Children + i, TriglavChess_ConvertPositionDataToPositionKey(moves[i].Position), moves[i].Position.Hash, currentDepth, onMove, treeBuildingData->BucketPool))
			{
				//position already encountered and cached in hashtable
				continue;
			}
			else
			{
				//new position
				if (currentDepth < maxDepth)
				{
					BuildSubtree(treeBuildingData, parent->Children[i], moves[i].Position, !onMove, maxDepth, currentDepth + 1);
				}
			}
		}
	}

	return;
}

static Evaluation BuildSubtreeMinMax(MinMaxTreeBuildingData* restrict treeBuildingData, PositionTreeNode* restrict node, const MoveFlags moveFlags, const PositionData position, const Colours onMove,
	const int maxDepth, const int currentDepth, Evaluation alpha, Evaluation beta)
{
	Evaluation currentNodeEvaluation;
	int isMaxDepth = currentDepth == maxDepth;
	uint8_t moveCountToUse = (uint8_t)((currentDepth) / 2);
	MoveSimulation* moves = NULL;
	int32_t movesCount = node->AvailableMovesCount;
	PositionTreeNode** childrenTmp = NULL;
	int32_t iteratedChildrenNodes = 0;

	if (node->Children == SpecialChildrenValueForRecursion)
	{
		//this means we've recursively reached a node we are already analysing. To avaoid messing up children arrays let's assign such repetition a draw value
		return (Evaluation) { (int8_t)DrawOrUknown, moveCountToUse, 0 };
		//this is an exception from MinMaxReturnLogic since we don't want to store this evaluation on the node
	}

	if (currentDepth <= 1)
	{
		//always push when entering node on depth 0 or 1
		PositionStack_Push(treeBuildingData->Stack, position);
	}

	if (movesCount && node->Children)
	{
		//this is the situation where we already have cached children nodes and at least one exists
		moves = DeriveBufferPointer(treeBuildingData, currentDepth);
		for (int i = 0; i < movesCount; i++)
		{
			moves[i] = TriglavChess_External_MoveBasedOnTargetPositionKey(position, node->Children[i]->Position, onMove);
		}
	}

	//check for faster exit
	if (onMove == White)
	{
		currentNodeEvaluation = MinEvaluationForWhite;
		if (WhiteWinMate == treeBuildingData->BestEvaluationSoFarForWhite.Result || WhiteWinNakedMate == treeBuildingData->BestEvaluationSoFarForWhite.Result)
		{
			if (treeBuildingData->BestEvaluationSoFarForWhite.InNMoves < moveCountToUse)
			{
				//this means there is already a winning sequence found, there is no point in looking further than length of that sequence
				goto MinMaxReturnLogic;
			}
		}
	}
	else
	{
		currentNodeEvaluation = MaxEvaluationForBlack;
		if (BlackWinMate == treeBuildingData->BestEvaluationSoFarForBlack.Result || BlackWinNakedMate == treeBuildingData->BestEvaluationSoFarForBlack.Result)
		{
			if (treeBuildingData->BestEvaluationSoFarForBlack.InNMoves < moveCountToUse)
			{
				//this means there is already a winning sequence found, there is no point in looking further than length of that sequence
				goto MinMaxReturnLogic;
			}
		}
	}
	moveCountToUse = (uint8_t)((currentDepth + 1) / 2);

	//check if already evaluated -> early exit
	if (TriglavChess_Evaluation_IsAssigned(node->Evaluation))
	{
		if ((node->Evaluation.InNMoves < moveCountToUse) || IsTerminal(node->Evaluation))
		{
			//if the node already has an evaluation assigned, either directly or from MinMax
			//and we don't have a chance to go deeper in this branch there is no chance on improving this evaluation,
			//so, let's just return it
			currentNodeEvaluation = node->Evaluation;

			goto MinMaxReturnLogic;
		}
	}

	//threefold repetition check
	if (currentDepth <= 1)
	{
		int matches = 2;
		for (int i = 3; i < treeBuildingData->Stack->Count; i++)
		{
			if (TriglavChess_PositionsEqual(position, TriglavChess_PositionStack_Get(treeBuildingData->Stack, i)))
			{
				matches--;
				if (!matches)
				{
					//threefold repetition detected
					PositionStack_Pop(treeBuildingData->Stack);
					return (Evaluation) { (int8_t)DrawOrUknown, moveCountToUse, 0 };
					//this is an exception from MinMaxReturnLogic since we don't want to store this evaluation on a node
				}
			}
		}
	}


	//current position is a result of a capture, or check, so we need to check if it isn't terminal
	if (moveFlags)
	{
		if (!moves)
		{
			//it's important to check legal moves here, so if there are none it means position is terminal
			moves = DeriveBufferPointer(treeBuildingData, currentDepth);
			movesCount = TriglavChess_GetMoves(position, moves, treeBuildingData->ReusableBuffer, onMove);
		}
		//don't want to override currentNodeEvaluation if not needed
		Evaluation terminal = TriglavChess_EvaluateTerminalPosition(moves, movesCount, position, onMove, moveCountToUse);

		//it is indeed a terminal position
		if (IsTerminal(terminal))
		{
			currentNodeEvaluation = terminal;
			goto MinMaxReturnLogic;
		}
	}

	//we are in a leaf node
	if (isMaxDepth)
	{
		currentNodeEvaluation = TriglavChess_External_EvaluateNonTerminalPosition((EvaluationArguments) { position, treeBuildingData->EvaluationModifiers, moveCountToUse });

		goto MinMaxReturnLogic;
	}

	//not a leaf, not a terminal position
	if (!moves)
	{
		//moves were not cached already
		moves = DeriveBufferPointer(treeBuildingData, currentDepth);
		movesCount = TriglavChess_GetMoves(position, moves, treeBuildingData->ReusableBuffer, onMove);
	}

	//no moves -> terminal position
	if (movesCount == 0)
	{
		currentNodeEvaluation = TriglavChess_EvaluateTerminalPosition(moves, movesCount, position, onMove, moveCountToUse);

		goto MinMaxReturnLogic;
	}

	//children array is unassigned
	if (!node->Children)
	{
		node->LegalMovesCount = movesCount;
		node->AvailableMovesCount = movesCount;
		node->Children = TriglavChess_GetNextTreeNodeMoveChunk(treeBuildingData->NodesPool, movesCount);

		for (int i = 0; i < movesCount; i++)
		{
			//TODO smarter way to prevent list of children to be reorganised multiple times at different levels
			(void)TriglavChess_GetOrAddToCache(treeBuildingData->NodeCache->Hashtable, node->Children + i, TriglavChess_ConvertPositionDataToPositionKey(moves[i].Position), moves[i].Position.Hash, (ulong)currentDepth, onMove, treeBuildingData->BucketPool);
		}
	}

	if (node->Children && moves)
	{
		childrenTmp = node->Children;
		node->Children = SpecialChildrenValueForRecursion;

		Evaluation childrenEvaluations[MaxMovesInPosition];
		//PositionTreeNode* evaluatedChildren[MaxMovesInPosition];
		//Evaluation evaluatedChildrenEvaluations[MaxMovesInPosition];
		int evaluatedChildrenCount = 0;
		int childrenCapacity = node->LegalMovesCount;
		int loopStart = 0;

	RestartSearch:
		iteratedChildrenNodes = 0;
		//int searchStoppedEarly = 0;
		for (int i = loopStart; i < movesCount; i++)
		{
			iteratedChildrenNodes++;

			MoveSimulation simulation = moves[i];
			Evaluation evaluation;

			evaluation = BuildSubtreeMinMax(treeBuildingData, childrenTmp[i], simulation.MoveFlags, simulation.Position, !onMove, maxDepth, currentDepth + 1, alpha, beta);
			evaluatedChildrenCount++;
			childrenEvaluations[i] = evaluation;

			if (onMove == White)
			{
				//insertion sort
				int j = i;
				while (j >= 1 && Evaluation_IsGreater(childrenEvaluations[j], childrenEvaluations[j - 1]))
				{
					PositionTreeNode* tmp = childrenTmp[j];
					Evaluation tmpEvaluation = childrenEvaluations[j];
					childrenTmp[j] = childrenTmp[j - 1];
					childrenEvaluations[j] = childrenEvaluations[j - 1];
					childrenTmp[j - 1] = tmp;
					childrenEvaluations[j - 1] = tmpEvaluation;
					j--;
				}
				if (Evaluation_IsGreater(evaluation, currentNodeEvaluation))
				{
					//we found better move
					currentNodeEvaluation = evaluation;
					if (currentDepth == 0)
					{
						//special logic for root node
						if (Evaluation_IsGreater(evaluation, treeBuildingData->BestEvaluationSoFarForWhite))
						{
							treeBuildingData->BestEvaluationSoFarForWhite = evaluation;
						}
						if ((WhiteWinMate == treeBuildingData->BestEvaluationSoFarForWhite.Result || WhiteWinNakedMate == treeBuildingData->BestEvaluationSoFarForWhite.Result) && treeBuildingData->BestEvaluationSoFarForWhite.InNMoves == 1)
						{
							goto AfterLoop;//we won't find anything better than win in 1
						}
					}
				}
				if (Evaluation_IsGreaterOrEqual(currentNodeEvaluation, beta))
				{
					goto AfterLoop;
				}
				if (Evaluation_IsGreater(currentNodeEvaluation, alpha))
				{
					alpha = evaluation;
				}
			}
			else
			{
				//insertion sort
				int j = i;
				while (j >= 1 && Evaluation_IsLess(childrenEvaluations[j], childrenEvaluations[j - 1]))
				{
					PositionTreeNode* tmp = childrenTmp[j];
					Evaluation tmpEvaluation = childrenEvaluations[j];
					childrenTmp[j] = childrenTmp[j - 1];
					childrenEvaluations[j] = childrenEvaluations[j - 1];
					childrenTmp[j - 1] = tmp;
					childrenEvaluations[j - 1] = tmpEvaluation;
					j--;
				}
				if (Evaluation_IsLess(evaluation, currentNodeEvaluation))
				{
					//we found better move
					currentNodeEvaluation = evaluation;
					if (currentDepth == 0)
					{
						//special logic for root node
						if (Evaluation_IsLess(evaluation, treeBuildingData->BestEvaluationSoFarForBlack))
						{
							treeBuildingData->BestEvaluationSoFarForBlack = evaluation;
						}
						if ((BlackWinMate == treeBuildingData->BestEvaluationSoFarForBlack.Result || BlackWinNakedMate == treeBuildingData->BestEvaluationSoFarForBlack.Result) && treeBuildingData->BestEvaluationSoFarForBlack.InNMoves == 1)
						{
							goto AfterLoop;//we won't find anything better than win in 1
						}
					}
				}
				if (Evaluation_IsLessOrEqual(currentNodeEvaluation, alpha))
				{
					goto AfterLoop;
				}
				if (Evaluation_IsLess(currentNodeEvaluation, beta))
				{
					beta = evaluation;
				}
			}
		}

		//loop iterated to end, with no alpha-beta cutoff
		if (node->AvailableMovesCount < node->LegalMovesCount)
		{
			//Keep the evaluated prefix in its search order and append children omitted by CopySubtree.
			int generatedMovesCount = TriglavChess_GetMoves(position, moves, treeBuildingData->ReusableBuffer, onMove);
			if (generatedMovesCount > childrenCapacity)//veery defensive programming
			{
				//reallocate buffer
				PositionTreeNode** expandedChildren = TriglavChess_GetNextTreeNodeMoveChunk(treeBuildingData->NodesPool, generatedMovesCount);
				for (int i = 0; i < node->AvailableMovesCount; i++)
				{
					expandedChildren[i] = childrenTmp[i];
				}
				childrenTmp = expandedChildren;
				childrenCapacity = generatedMovesCount;
			}

			//save pointer to next node to evaluate
			int recoveredMovesCount = node->AvailableMovesCount;
			//optimisation so the loop doesn't start from beginning
			loopStart = recoveredMovesCount;
			for (int i = 0; i < generatedMovesCount; i++)
			{
				PositionKey generatedPosition = TriglavChess_ConvertPositionDataToPositionKey(moves[i].Position);
				int positionAlreadyAvailable = 0;
				for (int j = 0; j < recoveredMovesCount; j++)
				{
					//let's check if current search has already evaluated this position - this may happen since earlier search might have first reordered psotions and only later they were trimmed
					if (TriglavChess_PositionKeysEqual(childrenTmp[j]->Position, generatedPosition))
					{
						positionAlreadyAvailable = 1;
						break;
					}
				}

				if (!positionAlreadyAvailable)
				{
					//standard add to cache - this position will be analysed
					(void)TriglavChess_GetOrAddToCache(treeBuildingData->NodeCache->Hashtable, childrenTmp + recoveredMovesCount, generatedPosition, moves[i].Position.Hash, (ulong)currentDepth, onMove, treeBuildingData->BucketPool);
					recoveredMovesCount++;
				}
			}

			node->LegalMovesCount = generatedMovesCount;
			node->AvailableMovesCount = recoveredMovesCount;
			movesCount = recoveredMovesCount;
			//recover moves as well to get MoveFlags
			for (int i = 0; i < movesCount; i++)
			{
				moves[i] = TriglavChess_External_MoveBasedOnTargetPositionKey(position, childrenTmp[i]->Position, onMove);
			}
			//The array changed while searching, so process it again from the beginning. Results already calculated during this search are reused by clever checks above that verify if nodes will be reordered.
			//we may think of not restarting the loop from beginning, but in theory rerunning MinMax on the same subtrees should be fast, since it will read results stored on nodes.
			goto RestartSearch;
		}

		//common logic to save MinMax result and update Children array
		AfterLoop:

		//assign Childen array back
		node->Children = childrenTmp;
		node->AnalysedMovesCount = iteratedChildrenNodes;

		//best move is always the first one in the array, therefore current node evaulation is the one of best move's subtree
		currentNodeEvaluation = childrenEvaluations[0];
		if (!IsTerminal(currentNodeEvaluation))
		{
			currentNodeEvaluation = (Evaluation){ currentNodeEvaluation.Result, moveCountToUse, currentNodeEvaluation.EvaluationValue };
		}
	}

MinMaxReturnLogic:
	if (currentDepth <= 1)
	{
		//always pop from stack before returning on depth 0 or 1
		PositionStack_Pop(treeBuildingData->Stack);
	}
	node->Evaluation = currentNodeEvaluation;
	return currentNodeEvaluation;
}
