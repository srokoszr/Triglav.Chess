using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Triglav.Chess.Engine.Benchmarks
{
    internal class Program
    {
        private static string BasePath =>
            $"{PathHelper.CurrentLocationDirectoryInfo?.Parent?.Parent?.Parent}\\Output";
        
        static void Main(string[] args)
        {
            if (args.Length > 0 && (args[0].StartsWith("--search-comparison", StringComparison.Ordinal)
                || args[0] == "--full-depth-comparison" || args[0] == "--algorithm-scenarios" || args[0].StartsWith("--algorithm-memory", StringComparison.Ordinal)))
            {
                try
                {
                    AlgorithmBenchmarkPolicy.EnsureRelease();
                }
                catch (InvalidOperationException error)
                {
                    Console.Error.WriteLine(error.Message);
                    Environment.ExitCode = 1;
                    return;
                }
            }
            if (args.Length == 1 && args[0] == "--algorithm-scenarios")
            {
                AlgorithmScenarioCatalog.Print();
                return;
            }
            if (args.Length == 3 && args[0] == "--algorithm-memory-case")
            {
                AlgorithmMemoryMeasurements.PrintOne(Int32.Parse(args[1], CultureInfo.InvariantCulture), args[2]);
                return;
            }
            if ((args.Length == 2 || args.Length == 3) && args[0] == "--algorithm-memory")
            {
                AlgorithmMemoryMeasurements.WriteAll(args[1], args.Length == 3 ? args[2] : null);
                return;
            }
            if (!new DirectoryInfo(BasePath).Exists)
            {
                Directory.CreateDirectory(BasePath);
            }
            IConfig benchmarkDotNetConfig = DefaultConfig.Instance;//.AddJob(Job.Default.WithRuntime(NativeAotRuntime.Net90));
            benchmarkDotNetConfig = benchmarkDotNetConfig.WithArtifactsPath(BasePath);
            //BenchmarkRunner.Run(Assembly.GetExecutingAssembly(), benchmarkDotNetConfig);//just run all
            //BenchmarkRunner.Run<HashTableBenchmarks>(benchmarkDotNetConfig);
            //BenchmarkRunner.Run<InteropBenchmarks>(benchmarkDotNetConfig);
            if (args.Length > 0 && args[0] == "--search-comparison-check")
            {
                SearchComparisonBenchmark.PrintChecks();
            }
            else if (args.Length > 0 && args[0] == "--search-comparison")
            {
                BenchmarkSwitcher.FromTypes([typeof(SearchComparisonBenchmark)]).Run(args.Skip(1).ToArray(), benchmarkDotNetConfig);
            }
            else if (args.Length > 0 && args[0] == "--full-depth-comparison")
            {
                BenchmarkSwitcher.FromTypes([typeof(FullDepthComparisonBenchmark)]).Run(args.Skip(1).ToArray(), benchmarkDotNetConfig);
            }
            else
            {
                BenchmarkRunner.Run<PositionEnumeratorBenchmark>(benchmarkDotNetConfig);
            }
            //BenchmarkRunner.Run<ColoursBenchmark>(benchmarkDotNetConfig);
        }
    }
}
