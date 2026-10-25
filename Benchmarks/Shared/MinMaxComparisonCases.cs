using System;
using System.Collections.Generic;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Extensions;

namespace Triglav.Chess.Engine.Comparison
{
    // Shared input data only: this file is linked into the test and benchmark projects.
    public sealed record MinMaxComparisonCase(String Name, Int32 Depth, Boolean Starting, Boolean Black, String Kind)
    {
        public override String ToString() => this.Name;

        public Position CreatePosition()
        {
            if (this.Starting)
            {
                Position starting = Position.GetStartingPosition();
                starting.OnMove = this.Black ? Colours.Black : Colours.White;
                return starting;
            }

            Position position = [];
            if (this.Kind == "MateInTwo")
            {
                // MinMaxTests.EndGameTests.MateInTwo_1_1.
                Add(Colours.White, Pieces.KingMoved, "c6");
                Add(Colours.White, Pieces.Rook, "h7");
                Add(Colours.Black, Pieces.KingMoved, "a8");
                Add(Colours.White, Pieces.Pawn, "a3");
                Add(Colours.Black, Pieces.Pawn, "a4");
                Add(Colours.White, Pieces.Pawn, "b4");
                Add(Colours.Black, Pieces.Pawn, "c4");
            }
            else if (this.Kind == "Stalemate")
            {
                // MinMaxTests.EndGameTests.StalemateInOne_1.
                Add(Colours.White, Pieces.KingMoved, "c6");
                Add(Colours.White, Pieces.Rook, "h7");
                Add(Colours.Black, Pieces.KingMoved, "a8");
                Add(Colours.White, Pieces.Pawn, "a2");
                Add(Colours.White, Pieces.Pawn, "h6");
                Add(Colours.Black, Pieces.Pawn, "a3");
            }
            else
            {
                // Nonterminal rook/pawn ending: exercise more than an immediate winning exit.
                Add(Colours.White, Pieces.KingMoved, "c3");
                Add(Colours.White, Pieces.Rook, "a1");
                Add(Colours.White, Pieces.Pawn, "d4");
                Add(Colours.Black, Pieces.KingMoved, "f6");
                Add(Colours.Black, Pieces.Rook, "h8");
                Add(Colours.Black, Pieces.Pawn, "e5");
            }
            position.OnMove = this.Black ? Colours.Black : Colours.White;
            return position;

            void Add(Colours colour, Pieces piece, String square)
            {
                String target = this.Black ? $"{square[0]}{9 - (square[1] - '0')}" : square;
                position.Add(this.Black ? colour.GetOpposite() : colour, piece, target);
            }
        }

        public static IEnumerable<MinMaxComparisonCase> All =>
        [
            new("StartWhiteD2", 2, true, false, ""),
            new("StartBlackD2", 2, true, true, ""),
            new("StartWhiteD3", 3, true, false, ""),
            new("StartBlackD3", 3, true, true, ""),
            new("MateTwoWhiteD4", 4, false, false, "MateInTwo"),
            new("MateTwoBlackD4", 4, false, true, "MateInTwo"),
            new("StalemateWhiteD4", 4, false, false, "Stalemate"),
            new("StalemateBlackD4", 4, false, true, "Stalemate"),
            new("RookEndingWhiteD4", 4, false, false, "RookEnding"),
            new("RookEndingBlackD4", 4, false, true, "RookEnding")
        ];
    }
}
