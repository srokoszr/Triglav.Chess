using Triglav.Chess.Engine.Model;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using System;
using System.Collections.Generic;
using Triglav.Chess.Interop;
using Triglav.Chess.Model.Structs.Interoperable;
using Triglav.Chess.Model.Extensions;

namespace Triglav.Chess.Engine.Core
{
    public sealed class PositionEvaluator
    {
        private const byte Pawn = (byte)Pieces.Pawn;

        private const byte Rook = (byte)Pieces.Rook;

        private const byte Knight = (byte)Pieces.Knight;

        private const byte Bishop = (byte)Pieces.Bishop;

        private const byte King = (byte)Pieces.King;

        private const byte KingMoved = (byte)Pieces.KingMoved;

        private const byte Queen = (byte)Pieces.Queen;

        private const byte QueenMoved = (byte)Pieces.QueenMoved;

        public EvaluationModifiers Modifiers
        {
            get;
        }

        public PositionEvaluator(EvaluationModifiers modifiers)
        {
            this.Modifiers = modifiers;

            this.Modifiers.Print(Console.Out);
        }

        public unsafe EvaluationWrapper Evaluate(in Position position, byte move)
        {
            if (position.OnMove == Colours.White)
            {
                return NativeInterface.EvaluateWhite(position.State, this.Modifiers.EvaluationModifiersData, move);
            }
            else
            {
                return NativeInterface.EvaluateBlack(position.State, this.Modifiers.EvaluationModifiersData, move);
            }
        }

        public EvaluationWrapper EvaluateNonTerminalPosition(in Position position, byte move)
        {
            return NativeInterface.EvaluateNonTerminalPosition(position.State, this.Modifiers.EvaluationModifiersData, move);
        }

        public unsafe EvaluationWrapper EvaluateTerminalPosition(in Position position, byte move)
        {
            return NativeInterface.EvaluateTerminalPosition(position.State, this.Modifiers.EvaluationModifiersData, move, position.OnMove);
        }

        public EvaluationWrapper EvaluateTerminalPosition(in Position position, PositionStackWrapper history, Byte move)
        {
            EvaluationWrapper terminal = this.EvaluateTerminalPosition(position, move);
            if (terminal.IsTerminal)
            {
                return terminal;
            }
            return history.IsThreefoldRepetition(position)
                ? new Evaluation((Int16)Evaluation.EvaluationNaN, EvaluationResult.DrawOrUknown, move)
                : terminal;
        }

        public static PositionEvaluator GetDefault() => new PositionEvaluator(new EvaluationModifiers());
    }
}
