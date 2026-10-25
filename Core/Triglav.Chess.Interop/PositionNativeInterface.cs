using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs.Interoperable;
using PositionKey = Triglav.Chess.Model.Structs.Interoperable.PositionKey;

namespace Triglav.Chess.Interop
{
    public static partial class PositionNativeInterface
    {

        #region GetMoves

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_Interop_GetMoves")]
        private static unsafe partial int GetMovesInterop(in PositionData position, MoveSimulation* result, Colours onMove);

        public static unsafe int GetMoves(in PositionData position, MoveSimulation* result, Colours onMove) => GetMovesInterop(position, result, onMove);

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_Interop_GetMovesWhite")]
        private static unsafe partial int GetMovesWhiteInterop(in PositionData position, MoveSimulation* result);

        public static unsafe int GetMovesWhite(in PositionData position, MoveSimulation* result) => GetMovesWhiteInterop(position, result);

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_Interop_GetMovesBlack")]
        private static unsafe partial int GetMovesBlackInterop(in PositionData position, MoveSimulation* result);

        public static unsafe int GetMovesBlack(in PositionData position, MoveSimulation* result) => GetMovesBlackInterop(position, result);

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_Interop_GetMovesNoFlags")]
        private static unsafe partial int GetMovesNoFlagsInterop(in PositionData position, MoveSimulation* result, Colours onMove);

        public static unsafe int GetMovesNoFlags(in PositionData position, MoveSimulation* result, Colours onMove) => GetMovesNoFlagsInterop(position, result, onMove);

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_MoveBasedOnTargetPositionKey")]
        private static unsafe partial MoveSimulation MoveBasedOnTargetPositionKeyInterop(PositionData position, PositionKey target, int onMove);

        public static unsafe MoveSimulation MoveBasedOnTargetPositionKey(in PositionData position, in PositionKey target, in Colours onMove) => MoveBasedOnTargetPositionKeyInterop(position, target, (int)onMove);

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_GetMoveBasedOnTargetPositionKey")]
        private static unsafe partial MoveWithPieceStruct GetMoveBasedOnTargetPositionKeyInterop(PositionData position, PositionKey target);

        public static unsafe MoveWithPieceStruct GetMoveBasedOnTargetPositionKey(in PositionData position, in PositionKey target) => GetMoveBasedOnTargetPositionKeyInterop(position, target);

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_GetTakenFields")]
        private static unsafe partial ulong GetTakenFieldsInterop(PositionKey position);

        public static unsafe ulong GetTakenFields(in PositionKey position) => GetTakenFieldsInterop(position);

        #endregion


        #region GetAttackedFields

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_GetAttackedFields")]
        private static partial ulong GetAttackedFieldsInterop(PositionData position, Colours onMove);

        public static ulong GetAttackedFields(PositionData position, Colours onMove) => GetAttackedFieldsInterop(position, onMove);

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_GetAttackedFieldsWhite")]
        private static partial ulong GetAttackedFieldsWhiteInterop(PositionData position);

        public static ulong GetAttackedFieldsWhite(PositionData position) => GetAttackedFieldsWhiteInterop(position);

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_GetAttackedFieldsBlack")]
        private static partial ulong GetAttackedFieldsBlackInterop(PositionData position);

        public static ulong GetAttackedFieldsBlack(PositionData position) => GetAttackedFieldsBlackInterop(position);

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_GetAttackedFieldsWhite_BitwiseLoop")]
        private static partial ulong GetAttackedFieldsWhiteInterop_BitwiseLoop(PositionData position);

        public static ulong GetAttackedFieldsWhite_BitwiseLoop(PositionData position) => GetAttackedFieldsWhiteInterop_BitwiseLoop(position);

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_GetAttackedFieldsBlack_BitwiseLoop")]
        private static partial ulong GetAttackedFieldsBlackInterop_BitwiseLoop(PositionData position);

        public static ulong GetAttackedFieldsBlack_BitwiseLoop(PositionData position) => GetAttackedFieldsBlackInterop_BitwiseLoop(position);


        #endregion

    }
}
