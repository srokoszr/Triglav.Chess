using System;
using System.Collections.Generic;
using Triglav.Chess.Model.Structs;

namespace Triglav.Chess.Engine.Generators
{
    public static class UpTo6EndgamesGenerator
    {
        public static IEnumerable<OptimisedPositionUpTo6> Generate()
        {
            for (UInt64 i = 0b00010000_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
                 i < 0b00111000_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
                 i++)
            {
                var candidate = new OptimisedPositionUpTo6(i << 2);
                yield return candidate;
            }
        }
    }
}
