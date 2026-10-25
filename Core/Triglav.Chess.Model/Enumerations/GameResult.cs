using System;
using System.Collections.Generic;
using System.Text;

namespace Triglav.Chess.Model.Enumerations
{
    public enum GameResult
    {
        Unknown = 0,
        
        Error = -1,
        
        Draw = 1,
        DrawAgreed = 2,
        DrawThreefoldRepetition = 3,

        WhiteWinStalemate = 4,
        WhiteWinNakedMate = 5,
        WhiteWinMate = 6,
        WhiteWinResign = 7,

        BlackWinStalemate = 8,
        BlackWinNakedMate = 9,
        BlackWinMate = 10,
        BlackWinResign = 11,

        WhiteWin = 12,
        BlackWin = 13,
    }
}
