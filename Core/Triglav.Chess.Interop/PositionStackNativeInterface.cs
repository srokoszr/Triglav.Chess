using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Interop
{
    public static partial class PositionStackNativeInterface
    {
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_PositionStack_free")]
        private static unsafe partial void FreeInterop(PositionStack* stack);
        
        public static unsafe void Free(PositionStack* stack) => FreeInterop(stack);

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_PositionStack_new")]
        private static unsafe partial PositionStack* NewInterop();

        public static unsafe PositionStack* New() => NewInterop();

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_PositionStack_PushExternal")]
        private static unsafe partial void PushInterop(PositionStack* stack, in PositionData postion);

        public static unsafe void Push(PositionStack* stack, in PositionData postion) => PushInterop(stack, postion);

        [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_PositionStack_PopExternal")]
        private static unsafe partial PositionData PopInterop(PositionStack* stack);

        public static unsafe PositionData Pop(PositionStack* stack) => PopInterop(stack);
    }
}
