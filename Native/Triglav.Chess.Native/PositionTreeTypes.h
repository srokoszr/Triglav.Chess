#pragma once

#include "InteropTypes.h"

/*
Not very realistic example, however it illustrates the general scheme, that nodes with position can repeat, 
however from parent nodes (layer A) there might be multiple moves to get to position B

			1 Move
A Node ->			-> B position Node
			2 Move
*/

struct PositionTreeNodeMove;

//A TreeNode equals position
typedef struct PositionTreeNode
{
	PositionKey Position;

	//array of enumerated Children positions;
	//if -1 then it means current node is being recursively analysed on lower depth and we've got position repetition
	struct PositionTreeNode** Children;

	//current node evaluation for leaf nodes and MinMax evaluation for other nodes
	Evaluation Evaluation;

	//number of child positions searched before alpha-beta cutoff
	uint8_t AnalysedMovesCount;

	//number of all legal moves in the position; copied internal nodes reserve this many Children slots - should be the same as AvailableMovesCount
	uint8_t LegalMovesCount;

	//number of initialized child pointers in Children, so this if actual buffer length
	//in normal scenario this is the same as LegalMovesCount
	uint8_t AvailableMovesCount;
} PositionTreeNode;
/*
typedef struct PositionTreeNodeMove
{
	//ownership by pool/hashtable
	struct PositionTreeNode* ResultingPositionNode;

	int32_t MoveFlags;

	MoveWithPieceStructPacked MoveStruct;
} PositionTreeNodeMove;
*/
