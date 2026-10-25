using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.Engine.Util
{
    public static class UnicodeHelper
    {
        public static string GetUnicodeText(Pieces piece)
        {
            return (piece) switch
            {
                Pieces.King => "♔",
                Pieces.KingMoved => "♔",
                Pieces.Queen => "♕",
                Pieces.QueenMoved => "♕",
                Pieces.Rook => "♖",
                Pieces.Bishop => "♗",
                Pieces.Knight => "♘",
                Pieces.Pawn=> "♙",

                _ => throw new InvalidEnumArgumentException()
            };
        }
    }
}
