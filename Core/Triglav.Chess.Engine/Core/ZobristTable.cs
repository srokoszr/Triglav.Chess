using System;
using System.Collections.Generic;
using System.Text;
using Triglav.Chess.Engine.Util;
using Triglav.Chess.Interop;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Core
{
    /// <summary>
    /// https://en.wikipedia.org/wiki/Zobrist_hashing
    /// </summary>
    public static class ZobristTable
    {
        public const int PIECES_N = 16;

        public const int SQUARE_N = 64;

        public const int TABLE_SIZE_N = PIECES_N * SQUARE_N;

        private static bool HasBeenAlreadyInit = false;
        private static ulong[,] table = new ulong[PIECES_N, SQUARE_N];

        public static ulong[,] Table
        {
            get
            {
                if (!HasBeenAlreadyInit)
                {
                    Init();
                }

                return table;
            }
        }

        public static void Init() => Init(42);

        public static unsafe void Init(ulong seed)
        {
            var random = new Splitmix64(seed);

            ulong* data = NativeInterface.GetZobristTablePointer();

            for (int p = 0; p < 16; p++)
            {
                for (int sq = 0; sq < 64; sq++)
                {
                    table[p, sq] = data[p * 64 + sq] = (ulong)random.Next();
                }
            }

            HasBeenAlreadyInit = true;
        }

        public static ulong GetZobrist(Pieces piece, Colours colour, ulong location)
        {
            return Table[(int)piece | ((int)colour << 3), location];
        }

        public static ulong RecomputeHash(Position position)
        {
            ulong hash = 0;
            foreach (var piece in position.ToArray())
            {
                hash ^= GetZobrist(piece.Piece, piece.Colour, piece.Location.SingleCoordinate);
            }
            return hash;
        }
    }
}
