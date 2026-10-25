using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Triglav.Chess.Interop
{
    public static partial class BuildInfoNativeInterface
    {
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        [LibraryImport("Triglav.Chess.Native.dll", EntryPoint = "TriglavChess_External_Triglav_IsReleaseBuild")]
        private static partial Int32 IsReleaseBuildInterop();

        public static Boolean IsReleaseBuild => IsReleaseBuildInterop() == 1;
    }
}
