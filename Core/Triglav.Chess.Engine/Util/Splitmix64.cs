using System;
using System.Collections.Generic;
using System.Text;

namespace Triglav.Chess.Engine.Util
{
    /// <summary>
    /// https://rosettacode.org/wiki/Pseudo-random_numbers/Splitmix64
    /// </summary>
    public class Splitmix64
    {
        private ulong state;                                  /* The state can be seeded with any (upto) 64 bit integer value. */

        public Splitmix64(ulong seed)
        {
            this.state = seed;
        }

        public ulong Next()
        {
            this.state += 0x9e3779b97f4a7c15;              /* increment the state variable */
            ulong z = this.state;                          /* copy the state to a working variable */
            z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9;  /* xor the variable with the variable right bit shifted 30 then multiply by a constant */
            z = (z ^ (z >> 27)) * 0x94d049bb133111eb;  /* xor the variable with the variable right bit shifted 27 then multiply by a constant */
            return z ^ (z >> 31);                    /* return the variable xored with itself right bit shifted 31 */
        }
    }
}