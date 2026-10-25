using System;
using System.Collections.Generic;
using System.Text;

namespace Triglav.Chess.Engine.Util
{
    public static class PopcountHelper
    {
        public static int Popcount(ulong x)
        {
            unchecked
            {
                //magic (http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel)
                x = x - ((x >> 1) & 0x5555555555555555U);

                x = (x & 0x3333333333333333U) +
                    ((x >> 2) & 0x3333333333333333U);

                x = (x + (x >> 4)) & 0x0F0F0F0F0F0F0F0FU;
                return (int)((x * 0x0101010101010101U) >> 56);
            }
        }

        public static int Popcount(int x)
        {
            int c = x;
            // magic (http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel)
            unchecked
            {
                c = c - (c >> 1 & 0x55555555);
                c = (c & 0x33333333) + (c >> 2 & 0x33333333);
                c = (c + (c >> 4) & 0xF0F0F0F) * 0x1010101 >> 24;
            }

            return c;
        }
    }
}
