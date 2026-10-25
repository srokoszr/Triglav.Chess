#include "PositionStack.h"

#include "malloc.h"
#include "GlobalConstants.h"

PositionStack* TriglavChess_External_PositionStack_new()
{
	PositionData* array = malloc(StackGrowthSize * sizeof(PositionData));
	PositionStack* result = malloc(sizeof(PositionStack));
	*result = (PositionStack){ array, 0, StackGrowthSize };
	return result;
}

void TriglavChess_External_PositionStack_free(PositionStack* restrict stack)
{
	if (stack)
	{
		if (stack->Array)
		{
			free(stack->Array);
		}

		free(stack);
	}
}

void TriglavChess_PositionStack_EnsureCapacity(PositionStack* restrict stack, const int requiredAdditionalCapacity)
{
	if ((stack->Count + requiredAdditionalCapacity) >= stack->Capacity)
	{
		TriglavChess_PositionStack_Resize(stack);
	}
}

void TriglavChess_PositionStack_Resize(PositionStack* restrict stack)
{
	PositionData* tmp = realloc(stack->Array, 2 * stack->Capacity * sizeof(PositionData));
	if (tmp)
	{
		stack->Array = tmp;
	}
	else
	{
		//TODO
	}
}

PositionData TriglavChess_PositionStack_PopWithReturn(PositionStack* restrict stack)
{
	if (stack->Count)
	{
		return stack->Array[--stack->Count];
	}

	return POSITION_EMPTY;
}

void TriglavChess_External_PositionStack_PushExternal(PositionStack* restrict stack, const PositionData position)
{
	PositionStack_Push(stack, position);
}

PositionData TriglavChess_External_PositionStack_PopExternal(PositionStack* restrict stack)
{
	return TriglavChess_PositionStack_PopWithReturn(stack);
}

PositionData TriglavChess_PositionStack_Get(PositionStack* restrict stack, const int index)
{
	if (index >= stack->Count)
	{
		return POSITION_EMPTY;
	}

	return stack->Array[(stack->Count - 1) - index];
}
