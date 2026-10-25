#pragma once
#include "InteropTypes.h"

#define StackGrowthSize 64

typedef struct PositionStack
{
	PositionData* Array;

	int Count;

	int Capacity;
} PositionStack;

__declspec(dllexport) PositionStack* TriglavChess_External_PositionStack_new();
__declspec(dllexport) void TriglavChess_External_PositionStack_free(PositionStack* restrict stack);

void TriglavChess_PositionStack_EnsureCapacity(PositionStack* restrict stack, const int requiredAdditionalCapacity);

void TriglavChess_PositionStack_Resize(PositionStack* restrict stack);

#define PositionStack_Pop(stack) --stack->Count
PositionData TriglavChess_PositionStack_PopWithReturn(PositionStack* restrict stack);

#define PositionStack_Push(stack, position) TriglavChess_PositionStack_EnsureCapacity(stack, 1); \
stack->Array[stack->Count++] = position

__declspec(dllexport) void TriglavChess_External_PositionStack_PushExternal(PositionStack* restrict stack, const PositionData position);
__declspec(dllexport) PositionData TriglavChess_External_PositionStack_PopExternal(PositionStack* restrict stack);

PositionData TriglavChess_PositionStack_Get(PositionStack* restrict stack, const int index);
