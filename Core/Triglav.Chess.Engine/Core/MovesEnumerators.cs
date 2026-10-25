using System;
using Triglav.Chess.Model.Structs;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Core
{
    public static class MovesEnumerators
    {
        public const int MaxMovesInPosition = 14 * 2 + 8 * 2 + 4 * 2 + 8 + 10 + 7 * 8;

        public static unsafe Span<MoveStruct> GetMovesNoCheckValidation(this Position position)
        {
            MoveWithPieceStruct* result = stackalloc MoveWithPieceStruct[MaxMovesInPosition];

            var resultCount = position.OnMove == ColourConstants.White ? position.GetMovesNoCheckValidationWhite(result) : position.GetMovesNoCheckValidationBlack(result);

            var resultArr = new MoveStruct[resultCount];
            for(int i = 0; i< resultCount; i++)
            {
                resultArr[i] = (*(result + i)).Move;
            }
            return resultArr;
        }
    }
}