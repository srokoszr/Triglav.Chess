using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Interop
{
    public static partial class NativeInterface
    {
        #region Zobrist Table

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_GetZobristTablePointer")]
        private static unsafe partial ulong* GetZobristTablePointerInterop();

        public static unsafe ulong* GetZobristTablePointer() => GetZobristTablePointerInterop();

        #endregion

        #region Move

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_Move")]
        private static partial MoveSimulation MoveInterop(in PositionData position, in MoveWithPieceStruct move, Colours onMove);

        public static MoveSimulation Move(in PositionData position, in MoveWithPieceStruct move, Colours onMove) => MoveInterop(position, move, onMove);

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_MoveWhite")]
        private static partial MoveSimulation MoveWhiteInterop(in PositionData position, in MoveWithPieceStruct move);

        public static MoveSimulation MoveWhite(in PositionData position, in MoveWithPieceStruct move) => MoveWhiteInterop(position, move);

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_MoveBlack")]
        private static partial MoveSimulation MoveBlackInterop(in PositionData position, in MoveWithPieceStruct move);

        public static MoveSimulation MoveBlack(in PositionData position, in MoveWithPieceStruct move) => MoveBlackInterop(position, move);

        #endregion

        #region GetMovesNoCheckValidation

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_GetMovesNoCheckValidation")]
        private static unsafe partial int GetMovesNoCheckValidationInterop(in PositionData position, MoveWithPieceStruct* result, Colours onMove);

        public static unsafe int GetMovesNoCheckValidation(in PositionData position, MoveWithPieceStruct* result, Colours onMove) => GetMovesNoCheckValidationInterop(position, result, onMove);

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_GetMovesNoCheckValidationWhite")]
        private static unsafe partial int GetMovesNoCheckValidationWhiteInterop(in PositionData position, MoveWithPieceStruct* result);

        public static unsafe int GetMovesNoCheckValidationWhite(in PositionData position, MoveWithPieceStruct* result) => GetMovesNoCheckValidationWhiteInterop(position, result);


        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_GetMovesNoCheckValidationBlack")]
        private static unsafe partial int GetMovesNoCheckValidationBlackInterop(in PositionData position, MoveWithPieceStruct* result);

        public static unsafe int GetMovesNoCheckValidationBlack(in PositionData position, MoveWithPieceStruct* result) => GetMovesNoCheckValidationBlackInterop(position, result);

        #endregion


        #region Evaluation

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_EvaluateWhite")]
        private static partial Evaluation EvaluateWhiteInterop(in EvaluationArguments arguments);

        public static Evaluation EvaluateWhite(in PositionData position, in EvaluationModifiersData modifiers, in int move) => EvaluateWhiteInterop(new EvaluationArguments(position, modifiers, move));

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_EvaluateBlack")]
        private static partial Evaluation EvaluateBlackInterop(in EvaluationArguments arguments);

        public static Evaluation EvaluateBlack(in PositionData position, in EvaluationModifiersData modifiers, in int move) => EvaluateBlackInterop(new EvaluationArguments(position, modifiers, move));

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_EvaluateNonTerminalPosition")]
        private static partial Evaluation EvaluateNonTerminalPositionInterop(in EvaluationArguments arguments);

        public static Evaluation EvaluateNonTerminalPosition(in PositionData position, in EvaluationModifiersData modifiers, in int move) => EvaluateNonTerminalPositionInterop(new EvaluationArguments(position, modifiers, move));

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_EvaluateTerminalPositionExternal")]
        private static partial Evaluation EvaluateTerminalPositionInterop(in EvaluationArguments arguments);

        public static Evaluation EvaluateTerminalPosition(in PositionData position, in EvaluationModifiersData modifiers, in int move, in Colours onMove) => EvaluateTerminalPositionInterop(new EvaluationArguments(position, modifiers, move, onMove));

        #endregion

        #region Min Max

        public const Int32 MaximumComparisonPlies = 10;

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_MinMaxGetTopMoveFixedDepth")]
        private static partial MinMaxResult MinMaxGetTopMoveFixedDepthInterop(in MinMaxArguments arguments, out Int32 deepestPly);

        /// <summary>Fixed-horizon correctness comparison; no capture/check extensions or extra depth allowance.</summary>
        public static unsafe MinMaxResult MinMaxGetTopMoveFixedDepth(in PositionData position, in EvaluationModifiersData modifiers, Int32 plies, Colours onMove, PositionStack* stack, out Int32 deepestPly)
        {
            ValidateComparisonPlies(plies);
            return MinMaxGetTopMoveFixedDepthInterop(new MinMaxArguments(position, modifiers, plies, (Int32)onMove, stack), out deepestPly);
        }

        private static void ValidateComparisonPlies(Int32 plies)
        {
            if (plies < 1 || plies > MaximumComparisonPlies)
            {
                throw new ArgumentOutOfRangeException(nameof(plies), plies, "Correctness comparisons support 1–10 plies.");
            }
        }

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_MinMaxGetTopMove")]
        private static partial MinMaxResult MinMaxGetTopMoveInterop(in MinMaxArguments arguments);

        public static unsafe MinMaxResult MinMaxGetTopMove(in PositionData position, in EvaluationModifiersData modifiers, in int depth, in Colours onMove, PositionStack* stack) => MinMaxGetTopMoveInterop(new MinMaxArguments(position, modifiers, depth, (int)onMove, stack));


        #endregion

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_Evaluation_CompareTo_External")]
        private static partial int EvaluationCompareToInterop(EvaluationCompare args);

        public static int EvaluationCompareTo(in Evaluation @this, in Evaluation other) => EvaluationCompareToInterop(new EvaluationCompare(@this, other));

    }
}
