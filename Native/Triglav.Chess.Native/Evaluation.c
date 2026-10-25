#include "Evaluation.h"
#include "Enumerations.h"

static inline int CompareToInt(int this, int value)
{
    if (this < value) return -1;
    if (this > value) return 1;
    return 0;
}
static inline int CompareToByte(uint8_t this, uint8_t value)
{
    if (this < value) return -1;
    if (this > value) return 1;
    return 0;
}
static inline int CompareToSByte(int8_t this, int8_t value)
{
    if (this < value) return -1;
    if (this > value) return 1;
    return 0;
}
static inline int CompareToShort(int16_t this, int16_t value)
{
    if (this < value) return -1;
    if (this > value) return 1;
    return 0;
}
static inline int CompareToFloat(float this, float value)
{
    if (this < value) return -1;
    if (this > value) return 1;
    return 0;
}

int32_t TriglavChess_Evaluation_CompareTo(const Evaluation this, const Evaluation other)
{
    int c = CompareToSByte(this.Result, other.Result);
    if (c != 0)
    {
        return c;
    }
    if (IsTerminal(this) && IsTerminal(other))
    {
        c = CompareToByte(other.InNMoves, this.InNMoves);
        if (c != 0)
        {
            return c;
        }
    }
    return CompareToShort(this.EvaluationValue, other.EvaluationValue);
}

int32_t TriglavChess_External_Evaluation_CompareTo_External(const EvaluationCompare args)
{
    return TriglavChess_Evaluation_CompareTo(args.This, args.Other);
}

int TriglavChess_Evaluation_IsAssigned(const Evaluation this)
{
    return this.Result || this.InNMoves;
}