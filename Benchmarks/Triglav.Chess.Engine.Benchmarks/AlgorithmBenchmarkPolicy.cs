using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Perfolizer.Horology;
using Perfolizer.Mathematics.OutlierDetection;
using System;
using System.Runtime.InteropServices;
using Triglav.Chess.Interop;

namespace Triglav.Chess.Engine.Benchmarks
{
    public sealed class AlgorithmBenchmarkConfig : ManualConfig
    {
        public AlgorithmBenchmarkConfig()
        {
            _ = this.AddJob(Job.Default.WithId("ReleaseSearch")
                .WithCustomBuildConfiguration("Release")
                .WithLaunchCount(1)
                .WithWarmupCount(1)
                .WithIterationCount(16)
                .WithIterationTime(TimeInterval.FromMilliseconds(150))
                .WithOutlierMode(OutlierMode.DontRemove));
        }
    }

    internal static partial class AlgorithmBenchmarkPolicy
    {
        [LibraryImport("kernel32.dll")]
        private static partial UInt32 SetErrorMode(UInt32 mode);

        internal static void EnsureRelease()
        {
#if DEBUG
            throw new InvalidOperationException("Algorithm benchmarks require a Release managed build. Rebuild with -c Release.");
#else
            if (OperatingSystem.IsWindows())
            {
                // Child benchmark processes inherit this mode. Report native failures
                // in the run log instead of blocking an unattended run on a modal dialog.
                _ = SetErrorMode(0x0001 | 0x0002);
            }
            try
            {
                if (!BuildInfoNativeInterface.IsReleaseBuild)
                {
                    throw new InvalidOperationException("Algorithm benchmarks require a Release native DLL. Rebuild the native project with Configuration=Release.");
                }
            }
            catch (EntryPointNotFoundException error)
            {
                throw new InvalidOperationException("Rebuild the native DLL: its Release build identity cannot be verified.", error);
            }
#endif
        }
    }
}
