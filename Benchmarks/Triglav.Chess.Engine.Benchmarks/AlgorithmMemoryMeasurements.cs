using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Triglav.Chess.Engine.Comparison;

namespace Triglav.Chess.Engine.Benchmarks
{
    internal static class AlgorithmMemoryMeasurements
    {
        private static readonly String[] Algorithms = ["MinMax", "PositionTree", "PositionTreeWithCopying"];
        private const Int32 SamplesPerCase = 3;

        internal static void WriteAll(String destination, String scenarioPrefix = null)
        {
            AlgorithmBenchmarkPolicy.EnsureRelease();
            List<Object> samples = new();
            AlgorithmBenchmarkCase[] scenarios = [.. AlgorithmBenchmarkCase.All];
            for (Int32 index = 0; index < scenarios.Length; index++)
            {
                if (scenarioPrefix != null && !scenarios[index].Kind.StartsWith(scenarioPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                foreach (String algorithm in Algorithms)
                {
                    for (Int32 sample = 1; sample <= SamplesPerCase; sample++)
                    {
                        ProcessStartInfo start = new(Environment.ProcessPath)
                        {
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        };
                        if (Path.GetFileNameWithoutExtension(Environment.ProcessPath).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
                        {
                            start.ArgumentList.Add(typeof(Program).Assembly.Location);
                        }
                        start.ArgumentList.Add("--algorithm-memory-case");
                        start.ArgumentList.Add(index.ToString(CultureInfo.InvariantCulture));
                        start.ArgumentList.Add(algorithm);
                        using Process worker = Process.Start(start);
                        System.Threading.Tasks.Task<String> output = worker.StandardOutput.ReadToEndAsync();
                        System.Threading.Tasks.Task<String> errors = worker.StandardError.ReadToEndAsync();
                        worker.WaitForExit();
                        String text = output.GetAwaiter().GetResult();
                        if (worker.ExitCode != 0)
                        {
                            throw new InvalidOperationException(errors.GetAwaiter().GetResult());
                        }
                        using JsonDocument document = JsonDocument.Parse(text);
                        samples.Add(new { Sample = sample, Measurement = document.RootElement.Clone() });
                    }
                    Console.WriteLine($"Memory: {scenarios[index]} / {algorithm}: {SamplesPerCase} isolated samples complete.");
                }
                // Keep completed scenarios if a later, deeper measurement fails.
                File.WriteAllText(destination, JsonSerializer.Serialize(samples, new JsonSerializerOptions { WriteIndented = true }));
            }
        }

        internal static void PrintOne(Int32 index, String algorithm)
        {
            AlgorithmBenchmarkPolicy.EnsureRelease();
            if (!OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException("These process peak-memory measurements require Windows.");
            }
            AlgorithmBenchmarkCase scenario = AlgorithmBenchmarkCase.All.ElementAt(index);
            SearchComparisonBenchmark benchmark = new() { Scenario = scenario };
            // Do not call Setup: it would run other algorithms and contaminate peaks.
            TextWriter output = Console.Out;
            try
            {
                Console.SetOut(TextWriter.Null);
                benchmark.Initialize();
            }
            finally
            {
                Console.SetOut(output);
            }
            try
            {
                Stopwatch watch = new();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                using Process process = Process.GetCurrentProcess();
                process.Refresh();
                Int64 baselinePeakCommitted = process.PeakPagedMemorySize64;
                Int64 baselinePeakResident = process.PeakWorkingSet64;
                watch.Start();
                SearchComparisonResult result = algorithm switch
                {
                    "MinMax" => benchmark.MinMax(),
                    "PositionTree" => benchmark.PositionTree(),
                    "PositionTreeWithCopying" => benchmark.PositionTreeWithCopying(),
                    _ => throw new ArgumentException("Unknown algorithm.", nameof(algorithm)),
                };
                watch.Stop();
                process.Refresh();
                Int64 peakCommitted = process.PeakPagedMemorySize64;
                Int64 peakResident = process.PeakWorkingSet64;
                Console.WriteLine(JsonSerializer.Serialize(new
                {
                    Scenario = scenario.ToString(),
                    SearchPolicy = "FixedDepthNoExtensions",
                    scenario.Kind,
                    scenario.Black,
                    NominalFullMoves = scenario.Depth,
                    NominalPlies = scenario.Plies,
                    Algorithm = algorithm,
                    SingleSearchElapsedMilliseconds = watch.Elapsed.TotalMilliseconds,
                    BaselinePeakCommittedBytes = baselinePeakCommitted,
                    BaselinePeakResidentBytes = baselinePeakResident,
                    PeakCommittedBytes = peakCommitted,
                    PeakResidentBytes = peakResident,
                    AdditionalPeakCommittedBytes = Math.Max(0, peakCommitted - baselinePeakCommitted),
                    AdditionalPeakResidentBytes = Math.Max(0, peakResident - baselinePeakResident),
                    Move = result.Move.Move.ToString(),
                    Piece = result.Move.Piece.ToString(),
                    ResultCode = result.Evaluation.Result,
                    result.Evaluation.InNMoves,
                    result.Evaluation.EvaluationValue,
                    Result = result.Evaluation.ToString(),
                }));
            }
            finally
            {
                benchmark.Cleanup();
            }
        }
    }
}
