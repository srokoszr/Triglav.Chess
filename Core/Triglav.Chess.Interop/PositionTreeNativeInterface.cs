using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Interop
{
    public static partial class PositionTreeNativeInterface
    {
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_PositionTree_free")]
        private static partial void FreeInterop(PositionTree tree);
        
        public static void Free(PositionTree tree) => FreeInterop(tree);

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_PositionTree_Build")]
        private static partial PositionTree BuildInterop(in PositionData position, int onMove, int maxDepth);

        public static PositionTree Build(in PositionData position, Colours onMove, int maxDepth) => BuildInterop(position, (int)onMove, maxDepth);

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_PositionTree_Extend")]
        private static unsafe partial PositionTree ExtendInterop(in PositionTree tree, PositionTreeNode* startingNode, in PositionData position, int onMove, int maxDepth);

        public static unsafe PositionTree Extend(in PositionTree tree, PositionTreeNode* startingNode, in PositionData position, Colours onMove, int maxDepth) => ExtendInterop(tree, startingNode, position, (int)onMove, maxDepth);

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_PositionTree_MinMaxBuild")]
        private static partial PositionTree MinMaxBuildInterop(in MinMaxArguments arguments);

        public static unsafe PositionTree MinMaxBuild(in PositionData position, in EvaluationModifiersData modifiers, in int depth, in Colours onMove, PositionStack* stack) => MinMaxBuildInterop(new MinMaxArguments(position, modifiers, depth, (int)onMove, stack));

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_PositionTree_MinMaxBuild_WithCopying")]
        private static partial PositionTree MinMaxBuildWithCopyingInterop(in MinMaxArguments arguments);

        public static unsafe PositionTree MinMaxBuildWithCopying(in PositionData position, in EvaluationModifiersData modifiers, in int depth, in Colours onMove, PositionStack* stack) => MinMaxBuildWithCopyingInterop(new MinMaxArguments(position, modifiers, depth, (int)onMove, stack));

    }
}
